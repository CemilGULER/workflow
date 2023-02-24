
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

        var targetState = request.Headers["TARGET_STATE"].ToString();
        var transitionName = body.GetProperty("LastTransition").ToString();
        var instanceIdAsString = body.GetProperty("InstanceId").ToString();

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
            .FirstOrDefault();

        if (instance is null)
        {
            return Results.NotFound($"Instance not found with instance id : {instanceId} ");
        }

        var transition = instance.State.Transitions.Where(t => t.Name == transitionName).FirstOrDefault();

        if (transition is null)
        {
            return Results.NotFound($"Transition not found with transition name : {transitionName} ");
        }

        if (targetState is null || targetState.ToLower() == "default")
        {

            if (transition.ToStateName is null)
            {
                return Results.BadRequest($"Target state is not provided nor defined on transition");
            }

            //var transitionData = JsonSerializer.Deserialize<dynamic>(body.GetProperty("LastTransitionData").ToString());


            var newInstanceTransition = new InstanceTransition
            {
                InstanceId = instance.Id,
                FromStateName = instance.StateName,
                ToStateName = transition.ToStateName,
                EntityData = body.GetProperty($"TRX-{transitionName}").GetProperty("Data").GetProperty("entityData").ToString(),
                FormData = body.GetProperty($"TRX-{transitionName}").GetProperty("Data").GetProperty("formData").ToString(),
                AdditionalData = body.GetProperty($"TRX-{transitionName}").GetProperty("Data").GetProperty("additionalData").ToString(),
                CreatedBy = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredBy").ToString()),
                CreatedByBehalfOf = Guid.Parse(body.GetProperty($"TRX-{transitionName}").GetProperty("TriggeredByBehalfOf").ToString()),
            };

            dbContext.Add(newInstanceTransition);

            instance.BaseStatus = transition.ToState!.BaseStatus;
            instance.StateName = transition.ToStateName;

            dbContext.SaveChanges();

            return Results.Ok();
        }
        else
        {

        }

        return Results.NotFound();
    }
}