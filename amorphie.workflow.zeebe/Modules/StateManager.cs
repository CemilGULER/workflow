using System.Text.Json;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

public static class StateManagerModule
{

    public static void MapStateManagerEndpoints(this WebApplication app)
    {
        app.MapPost("/amorphie-workflow-set-state", postWorkflowCompleted)
            .Produces(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Maps amorphie-workflow-set-state service worker on Zeebe";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Zeebe" } };
                return operation;
            });
    }


    static IResult postWorkflowCompleted(
            [FromBody] dynamic body,
            [FromServices] WorkflowDBContext dbContext,
            HttpRequest request,
            HttpContext httpContext,
            [FromServices] DaprClient client
        )
    {
        // TODO : Include a parameter for the cancelation token and add cancelation token to FirstOrDefault

        var targetState = request.Headers["TARGET_STATE"].ToString();
        var transitionName = body.GetProperty("LastTransition").ToString();
        var instanceIdAsString = body.GetProperty("InstanceId").ToString();
        var data = body.GetProperty($"TRX-{transitionName}").GetProperty("Data");
        Guid instanceId;
        if (!Guid.TryParse(instanceIdAsString, out instanceId))
        {
            return Results.BadRequest("InstanceId not provided or not as a GUID");
        }

        Instance? instance = dbContext.Instances
            .Where(i => i.Id == instanceId)
            .Include(i => i.State)
                .ThenInclude(s => s.Transitions)
                .ThenInclude(t => t.ToState)
                .ThenInclude(t => t!.Workflow)
                .ThenInclude(t => t!.Entities)
            .Include(i => i.State)
                .ThenInclude(s => s.Transitions)
                .ThenInclude(s => s.Page)
                .ThenInclude(s => s!.Pages)
            .FirstOrDefault();

        if (instance is null)
        {
            return Results.NotFound($"Instance not found with instance id : {instanceId} ");
        }
        bool error=false;
        Transition? transition = null;
        if (targetState is null || targetState.ToLower() == "default")
        {
            transition = instance.State.Transitions.Where(t => t.Name == transitionName).FirstOrDefault();
            

        }
        else if (targetState.ToLower() == "error")
        {
            string transitionNameAsString = transitionName.ToString();
            transition = dbContext.Transitions.Include(i => i.ToState).ThenInclude(t => t!.Workflow)
                .ThenInclude(t => t!.Entities).Where(t => t.Name == transitionNameAsString
           && instance.WorkflowName == t.ToState!.WorkflowName && t.ToState.Type == StateType.Fail).FirstOrDefault();
            error=true;
        }
        else
        {
            return Results.BadRequest($"Target state is not provided ");
        }
        //var transitionData = JsonSerializer.Deserialize<dynamic>(body.GetProperty("LastTransitionData").ToString());
        if (transition is null)
        {
            return Results.NotFound($"Transition not found with transition name : {transitionName} ");
        }

        if (transition.ToStateName is null)
        {
            return Results.BadRequest($"Target state is not provided nor defined on transition");
        }
        InstanceTransition? newInstanceTransition;
        if(!error)
        {
            newInstanceTransition=dbContext.InstanceTransitions.OrderByDescending(o=>o.StartedAt)
            .FirstOrDefault(f=>f.InstanceId==instance.Id&&f.TransitionName==transition.Name);
        }
        else
        {
            newInstanceTransition=dbContext.InstanceTransitions.Include(s=>s.Transition).OrderByDescending(o=>o.StartedAt)
            .FirstOrDefault(f=>f.InstanceId==instance.Id&&f.Transition!.FromStateName==transition.FromStateName);
             newInstanceTransition!.TransitionName=transition.Name;
              newInstanceTransition!.Transition=transition;
        }
        
        newInstanceTransition!.EntityData=body.GetProperty($"TRX-{transitionName}").GetProperty("Data").GetProperty("entityData").ToString();
        newInstanceTransition!.ToStateName=transition.ToStateName;
       
        newInstanceTransition!.CreatedBy=Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString());
        newInstanceTransition!.CreatedByBehalfOf=Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString());

        string eventInfo = "worker-completed";



        if (!string.IsNullOrEmpty(transition.ServiceName))
        {
            // TODO : Use refit rather than httpclient and consider resiliency.
            var clientHttp = new HttpClient();
            amorphie.workflow.core.Dtos.SendTransitionInfoRequest sendTransitionInfoRequest = new amorphie.workflow.core.Dtos.SendTransitionInfoRequest()
            {
                recordId = instance.RecordId,
                newStatus = transition.ToStateName!,
                entityData = JsonSerializer.Deserialize<object>(newInstanceTransition.EntityData),
                user = newInstanceTransition.CreatedBy,
                behalfOfUser = newInstanceTransition.CreatedByBehalfOf,
                workflowName = instance.WorkflowName
            };
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize(sendTransitionInfoRequest);
            var response = clientHttp.PostAsync(transition.ServiceName, new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json")).Result;
            //var content=new FormUrlEncodedContent(newInstanceTransition!.EntityData!);

            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.Created||
                 response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    instance.BaseStatus = transition.ToState!.BaseStatus;
                    instance.StateName = transition.ToStateName;
                    if (instance.WorkflowName != transition.ToState.WorkflowName)
                    {
                        instance.WorkflowName = transition.ToState!.WorkflowName!;
                        if (!transition.ToState.Workflow!.Entities.Any(a => a.Name == instance.EntityName))
                        {
                            instance.EntityName = transition.ToState.Workflow.Entities.FirstOrDefault()!.Name;
                        }
                    }
                }
                else
                {
                    instance.BaseStatus = transition.FromState!.BaseStatus;
                    eventInfo = "worker-error-with-service-" + transition.ServiceName;
                }
            }
            catch (Exception ex)
            {

            }
        }
        else
        {
            instance.BaseStatus = transition.ToState!.BaseStatus;
            instance.StateName = transition.ToStateName;
            if (instance.WorkflowName != transition.ToState.WorkflowName)
            {
                instance.WorkflowName = transition.ToState!.WorkflowName!;
                if (!transition.ToState.Workflow!.Entities.Any(a => a.Name == instance.EntityName))
                {
                    instance.EntityName = transition.ToState.Workflow.Entities.FirstOrDefault()!.Name;
                }
            }

        }
        newInstanceTransition!.FinishedAt=DateTime.Now;
        // dbContext.Add(newInstanceTransition);
        // TODO : Include a parameter for the cancelation token and convert SaveChanges to SaveChangesAsync with the cancelation token.
        dbContext.SaveChanges();


        var responseSignalR = client.InvokeMethodAsync<PostSignalRData, string>(
                   HttpMethod.Post,
                   "amorphie-workflow-hub.test-amorphie-workflow-hub",
                   "sendMessage",
                   new PostSignalRData(
                       newInstanceTransition.CreatedBy,
                       instance.RecordId,
                      eventInfo,
                       instance.Id,
                       instance.EntityName,
                     newInstanceTransition.EntityData, DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc), newInstanceTransition.ToStateName, transition.Name, instance.BaseStatus,
              transition.Page == null ? null :
              new PostPageSignalRData(transition.Page.Operation.ToString(), transition.Page.Type.ToString(), new amorphie.core.Base.MultilanguageText(transition.Page.Pages!.FirstOrDefault()!.Language, transition.Page.Pages!.FirstOrDefault()!.Label),
              transition.Page.Timeout)
                   ));
        return Results.Ok(createMessageVariables(newInstanceTransition, transitionName.ToString(), data));


        return Results.NotFound();
    }
    private static void SendSignalRData(InstanceTransition instanceTransition, string eventInfo, DaprClient _client, Instance instance)
    {

    }
    private static dynamic createMessageVariables(InstanceTransition instanceTransition, string _transitionName, dynamic _data)
    {
        dynamic variables = new Dictionary<string, dynamic>();

        variables.Add("EntityName", instanceTransition.Instance.EntityName);
        variables.Add("RecordId", instanceTransition.Instance.RecordId);
        variables.Add("InstanceId", instanceTransition.InstanceId);
        variables.Add("LastTransition", _transitionName);

        dynamic targetObject = new System.Dynamic.ExpandoObject();
        targetObject.Data = _data;
        targetObject.TriggeredBy = instanceTransition.CreatedBy;
        targetObject.TriggeredByBehalfOf = instanceTransition.CreatedByBehalfOf;

        variables.Add($"TRX-{_transitionName}", targetObject);
        return variables;
    }

}