
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using amorphie.core.Base;
using amorphie.core.Enums;
using amorphie.core.IBase;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

public static class ConsumerModule
{
    public static void MapConsumerEndpoints(this WebApplication app)
    {

        app.MapGet("/workflow/consumer/{entity}/record/{recordid}/transition", getTransitions)
           .Produces<GetRecordWorkflowAndTransitionsResponse>(StatusCodes.Status200OK)
           .WithOpenApi(operation =>
           {
               operation.Summary = "Returns available workflows and related transitions for given record.";
               operation.Tags = new List<OpenApiTag> { new() { Name = "Consumer BFF" } };

               return operation;
           });

        app.MapPost("/workflow/consumer/{entity}/record/{recordid}/transition/{transition}", postTransition)
            .Produces<ConsumerPostTransitionResponse>(StatusCodes.Status200OK)
            .Produces<ConsumerPostTransitionResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Triggers transition for existing workflow instance or creates new one.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Consumer BFF" } };

                operation.Responses["200"].Description = "Instance triggered successfully in existing instance.";
                operation.Responses["201"].Description = "Workflow started successfully with new instance.";
                operation.Responses["404"].Description = "No suitable transaction found for entity or record.";

                return operation;
            });

        app.MapGet("/workflow/consumer/{entitY}/record/{recordid}/history/", getHistory)
            .Produces<GetRecordHistoryResponse>(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Returns record workflow history.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Consumer BFF" } };

                return operation;
            });

        app.MapGet("/workflow/consumer/{entitY}/record/{recordid}/history/{instanceId}", getHistoryDetail)
            .Produces<GetRecordHistoryDetailResponse>(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Summary = "Return the instance with full detailed history.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Consumer BFF" } };

                return operation;
            });
    }
    private static string TemplateEngineForm(string templateName, string entityData, string templateURL)
    {
        string form = string.Empty;
        var clientHttp = new HttpClient();
        var response = new HttpResponseMessage();

        amorphie.workflow.core.Dtos.TemplateEngineRequest request = new amorphie.workflow.core.Dtos.TemplateEngineRequest()
        {
            RenderId = Guid.NewGuid(),
            Name = templateName,
            RenderData = entityData,
            RenderDataForLog = entityData,
            ProcessName = "Workflow Get Transition",
            ItemId = string.Empty,
            Action = "TemplateEngineForm",
            Identity = string.Empty,
            Customer = ""
        };
        var serializeRequest = JsonSerializer.Serialize(request);
        try
        {

            response = clientHttp.PostAsync(templateURL, new StringContent(serializeRequest, System.Text.Encoding.UTF8, "application/json")).Result;
            var twiceSerialize = response!.Content!.ReadAsStringAsync().Result;
            form = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(twiceSerialize)!;
            form = ReplaceDropdown(form);
            //builder.Configuration["DAPR_SECRET_STORE_NAME"]
        }
        catch (Exception ex)
        {
            form = string.Empty;
        }

        return form;
    }
    private static string ReplaceDropdown(string form)
    {
        //
        int startindex=0;
        int selectCount=0;
        while(form.Contains("\"type\": \"select\"") && (form.Contains("\"dataSrc\": \"url\"")))
        {
            selectCount++;
            var regexDataSrc = new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape("\"dataSrc\": \"url\","));
            form = regexDataSrc.Replace(form, "\"dataSrc\": \"json\",", 1);
            string data = "\"data\": {";
            string formIndexStart=form.Substring(startindex);
            int indexofUrlStart =formIndexStart.IndexOf("\"url\": \"http");
            startindex=indexofUrlStart+startindex;
            string AfterUrl = form.Substring(startindex + 8);
            int indexofUrlEndSub = AfterUrl.IndexOf("\"");
            string OnlyUrl = AfterUrl.Substring(0, indexofUrlEndSub);
            var clientHttp = new HttpClient();
            var response = clientHttp.GetAsync(OnlyUrl).Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            form=form.Insert(startindex," \"json\":" + responseBody + " ,");
            startindex+=responseBody.Length+23+indexofUrlEndSub;
            var test = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(form);
        }

        return form;
    }
    static IResponse<GetRecordWorkflowAndTransitionsResponse> getTransitions(
           [FromServices] WorkflowDBContext dbContext,
           [FromRoute(Name = "entity")] string entity,
           [FromRoute(Name = "recordId")] Guid recordId,
           [FromServices] DaprClient client,
            IConfiguration configuration,
           [FromHeader(Name = "Accept-Language")] string language = "tr-TR"

       )
    {

        //**************************//
        // load all workflows available to entity
        var workflows = dbContext.WorkflowEntities!
                .Where(e => e.Name == entity)
                .Include(e => e.Workflow)
                    .ThenInclude(w => w.Titles.Where(l => l.Language == language))
                .Include(e => e.Workflow)
                    .ThenInclude(w => w.States)
                    .ThenInclude(s => s.Titles.Where(l => l.Language == language))
                .Include(e => e.Workflow)
                    .ThenInclude(w => w.States)
                    .ThenInclude(s => s.Transitions)
                    .ThenInclude(t => t.Titles.Where(l => l.Language == language))
                     .Include(e => e.Workflow)
                    .ThenInclude(w => w.States)
                    .ThenInclude(s => s.Transitions)
                    .ThenInclude(t => t.ToState)
                .Include(e => e.Workflow)
                    .ThenInclude(w => w.States)
                    .ThenInclude(s => s.Transitions)
                    .ThenInclude(t => t.Forms.Where(l => l.Language == language))
                .ToList();




        var stateManagerWorkflow = workflows.Where(item => item.IsStateManager == true).FirstOrDefault();

        // load all active workflows of record.
        var instanceRecords = dbContext.Instances.Where(i => i.EntityName == entity && i.RecordId == recordId && i.BaseStatus != StatusType.Completed).ToList();
        //   using var client = new DaprClientBuilder().Build();
        //         var tokenRequestData=new GetTokenRequest(){
        //             Scope=string.Empty,
        //             InstanceId=instanceRecords.FirstOrDefault()!.Id,
        //         };
        //  var token =  client.InvokeMethodAsync<GetTokenRequest, string>(HttpMethod.Post, "amorphie-workflow-hub", "security/create-token", tokenRequestData).Result;


        var response = new GetRecordWorkflowAndTransitionsResponse();
        //response.IsStateRecordRegistered = instanceRecords.Count > 0;
        // var templateURL = configuration["DAPR_TEMPLATE_URL_NAME"]!;
        var templateURL = "https://test-template-engine.burgan.com.tr/Template/Render";
        string lastTransitionEntitydata=string.Empty;
        if (stateManagerWorkflow != null)
        {
            var stateManagerInstace = instanceRecords.Where(i => i.WorkflowName == stateManagerWorkflow.WorkflowName).FirstOrDefault();

            if (stateManagerInstace != null)
            {
                InstanceTransition lastTransition = dbContext.InstanceTransitions
                .Where(f => f.InstanceId == stateManagerInstace.Id)
                .OrderByDescending(o => o.CreatedAt).First();
                lastTransitionEntitydata=lastTransition.EntityData;
                response.StateManager = workflows.Where(item => item.IsStateManager == true).Select(item =>
                  new GetRecordWorkflowAndTransitionsResponse.StateManagerWorkflow
                  {
                      Name = item.Workflow.Name,
                      Title = item.Workflow.Titles.First().Label,
                      Status = stateManagerInstace.StateName,
                      Transitions = item.Workflow.States.FirstOrDefault(s => s.Name == stateManagerInstace.StateName)?.Transitions.Where(w=>w.ToState==null||w.ToState.Type!=StateType.Fail).Select(t =>
                          new GetRecordWorkflowAndTransitionsResponse.Transition
                          {
                              Name = t.Name,
                              Title = t.Titles.First().Label,
                              Form = TemplateEngineForm(t.Forms.First().Label, lastTransition.EntityData, templateURL!)
                          }).ToArray()
                  }
                      ).FirstOrDefault();
            }
            else
            {
                // Return start state transitions
                response.StateManager = workflows.Where(item => item.IsStateManager == true).Select(item =>
                   new GetRecordWorkflowAndTransitionsResponse.StateManagerWorkflow
                   {
                       Name = item.Workflow.Name,
                       Title = item.Workflow.Titles.First().Label,
                       Transitions = item.Workflow.States.FirstOrDefault(s => s.Type == StateType.Start)!.Transitions!.Where(w=>w.ToState==null||w.ToState.Type!=StateType.Fail).Select(t =>
                           new GetRecordWorkflowAndTransitionsResponse.Transition
                           {
                               Name = t.Name,
                               Title = t.Titles.FirstOrDefault() == null ? string.Empty : t.Titles.FirstOrDefault()!.Label,
                               Form = t.Forms.FirstOrDefault() == null ? string.Empty : TemplateEngineForm(t.Forms.FirstOrDefault()!.Label, string.Empty, templateURL)
                           }).ToArray()
                   }
                       ).FirstOrDefault();
            }
        }

        response.AvailableWorkflows = workflows.Where(item => item.IsStateManager == false).Select(item =>
                new GetRecordWorkflowAndTransitionsResponse.Workflow
                {
                    Name = item.Workflow.Name,
                    Title = item.Workflow.Titles.First().Label,
                    Transitions = item.Workflow.States.Where(s => s.Type == StateType.Start).First().Transitions.Where(w=>w.ToState==null||w.ToState.Type!=StateType.Fail).Select(t =>
                        new GetRecordWorkflowAndTransitionsResponse.Transition
                        {
                            Name = t.Name,
                            Title = t.Titles.First().Label,
                            Form = TemplateEngineForm(t.Forms.First().Label, lastTransitionEntitydata, templateURL)
                        }).ToArray()
                }
            ).ToArray();
              response.RunningWorkflows = instanceRecords.Where(w=>w.Workflow.Entities.Any(a=>a.IsStateManager==false)).Select(item =>
                new GetRecordWorkflowAndTransitionsResponse.RunningWorkflow
                {
                    InstanceId=item.Id,
                    Name = item.Workflow.Name,
                    Title = item.Workflow.Titles.First().Label,
                    Transitions = item.State.Transitions.Where(w=>w.ToState==null||w.ToState.Type!=StateType.Fail).Select(t =>
                        new GetRecordWorkflowAndTransitionsResponse.Transition
                        {
                            Name = t.Name,
                            Title = t.Titles.First(f=>f.Language==language).Label,
                            Form = TemplateEngineForm(t.Forms.First(f=>f.Language==language).Label, dbContext.InstanceTransitions.OrderBy(o=>o.CreatedAt)
                            .FirstOrDefault(f=>f.InstanceId==item.Id)!.EntityData, templateURL)
                        }).ToArray()
                }
            ).ToArray();

        return new Response<GetRecordWorkflowAndTransitionsResponse>
        {
            Data = response,
            Result = new Result(Status.Success, "Success")
        };
        // return Results.Ok(response);
    }

    static IResponse postTransition(
            [FromServices] WorkflowDBContext dbContext,
            [FromHeader(Name = "User")] Guid user,
            [FromHeader(Name = "Behalf-Of-User")] Guid behalOfUser,
            [FromRoute(Name = "entity")] string entity,
            [FromRoute(Name = "recordId")] Guid recordId,
            [FromRoute(Name = "transition")] string transition,
            [FromBody] ConsumerPostTransitionRequest data,
            [FromServices] IPostTransactionService service,
            [FromServices] DaprClient client
        )
    {
        var result = service.Init(entity, recordId, transition, user, behalOfUser, data);

        if (result.Result.Status == Status.Success.ToString())
        {
            result = service.Execute();
        }

        // var response = client.InvokeMethodAsync<PostPublishStatusRequest, string>(
        //     HttpMethod.Post,
        //     "amorphie-workflow-hub.test-amorphie-workflow-hub",
        //     "workflow/publish-status",
        //     new PostPublishStatusRequest(
        //         recordId,
        //         "SendOtp",
        //         "SendOtp",
        //         "SendOtp"
        //     ));


        return result;
    }


    static IResponse<GetRecordHistoryResponse> getHistory(
         [FromRoute(Name = "entity")] string entity,
         [FromRoute(Name = "recordId")] Guid recordId,
          [FromServices] WorkflowDBContext dbContext,
           [FromHeader(Name = "Accept-Language")] string language = "en-EN"
     )
    {


        var response = new GetRecordHistoryResponse();
        try
        {
 var instanceRecords = dbContext.Instances.Where(i => i.EntityName == entity && i.RecordId == recordId).Include(s=>s.Workflow).ThenInclude(t=>t.Entities).ToList();

        response.StateManager = instanceRecords.Where(item => item.BaseStatus == StatusType.Completed&& item.Workflow.Entities.Any(a => a.IsStateManager == true)).Select(item =>
           new GetRecordHistoryResponse.Workflow(item.WorkflowName, dbContext.InstanceTransitions.Where(w => w.InstanceId == item.Id).Select(ITransaction =>
           new GetRecordHistoryResponse.Transition(dbContext.Transitions.FirstOrDefault(f=>f.FromStateName== ITransaction.FromStateName
           &&f.ToStateName==ITransaction.ToStateName)!.Name,
            ITransaction.FromStateName, ITransaction.ToStateName, ITransaction.CreatedAt, ITransaction.CreatedBy)
           {

           }).ToList())
           {
               InstanceId = item.Id
           }
               ).FirstOrDefault();

        response.RunningWorkflows = instanceRecords.Where(item => item.BaseStatus != StatusType.Completed).Select(item =>
  new GetRecordHistoryResponse.Workflow(item.WorkflowName, dbContext.InstanceTransitions.Where(w => w.InstanceId == item.Id).Select(ITransaction =>
  new GetRecordHistoryResponse.Transition(dbContext.Transitions.FirstOrDefault(f=>f.FromStateName== ITransaction.FromStateName
           &&f.ToStateName==ITransaction.ToStateName)!.Name, ITransaction.FromStateName, ITransaction.ToStateName, ITransaction.CreatedAt, ITransaction.CreatedBy)
  {

  }).ToList())
  {
      InstanceId = item.Id
  }
      ).ToList();
        response.CompletedWorkflows = instanceRecords.Where(item => item.BaseStatus == StatusType.Completed).Select(item =>
   new GetRecordHistoryResponse.Workflow(item.WorkflowName, dbContext.InstanceTransitions.Where(w => w.InstanceId == item.Id).Select(ITransaction =>
   new GetRecordHistoryResponse.Transition(dbContext.Transitions.FirstOrDefault(f=>f.FromStateName== ITransaction.FromStateName
           &&f.ToStateName==ITransaction.ToStateName)!.Name, ITransaction.FromStateName, ITransaction.ToStateName, ITransaction.CreatedAt, ITransaction.CreatedBy)
   {

   }).ToList())
   {
       InstanceId = item.Id
   }
       ).ToList();


    
        }
        catch(Exception ex)
        {
            return   new Response<GetRecordHistoryResponse>
        {
            Result = new Result(Status.Error, "Unexpected Error:"+ex.ToString())
        };
        }
            return   new Response<GetRecordHistoryResponse>
        {
            Data = response,
            Result = new Result(Status.Success, "Success")
        };
       
    }


    static IResult getHistoryDetail(
         [FromRoute(Name = "entity")] string entity,
         [FromRoute(Name = "recordId")] Guid recordId,
         [FromRoute(Name = "instanceId")] Guid instanceId,
           [FromServices] WorkflowDBContext dbContext
     )
    {

        //  var instanceTransRecords = dbContext.InstanceTransitions.Include(e => e.Instance)
        //  .Where(i =>i.InstanceId==instanceId&&i.Instance.EntityName==entity&&i.Instance.RecordId==recordId).ToList();
        // GetRecordHistoryDetailResponse response= new GetRecordHistoryDetailResponse(instanceTransRecords.Select(s=>new GetRecordHistoryDetailResponse.Transition(
        //     s.Id,
        //     "",
        //     s.FromStateName,
        //     s.ToStateName,
        //     "",

        // )).ToList());
        return Results.Ok();
    }


    private static void postTransitionNoFlowNotInstance(Transition transition) { }
    private static void postTransitionNoFlowHasInstance(Transition transition) { }
    private static void postTransitionHasFlowHasInstance(Transition transition) { }
    private static void postTransitionHasFlowNoInstance(Transition transition) { }
}


public record GetRecordWorkflowAndTransitionsResponse
{
    public StateManagerWorkflow? StateManager { get; set; }
    public ICollection<RunningWorkflow>? RunningWorkflows { get; set; }
    public ICollection<Workflow>? AvailableWorkflows { get; set; }

    public record Workflow
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public ICollection<Transition>? Transitions { get; set; }
    }

    public record StateManagerWorkflow : Workflow
    {
        public string? Status { get; set; }
    }

    public record RunningWorkflow : Workflow
    {
        public Guid InstanceId { get; set; }
    }

    public record Transition
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Form { get; set; }
    }
}


public record ConsumerPostTransitionRequest
{

    public dynamic EntityData { get; set; } = default!;
    public dynamic? FormData { get; set; }
    public dynamic? AdditionalData { get; set; }
    public bool GetSignalRHub { get; set; }
    public dynamic? RouteData { get; set; }
    public dynamic? QueryData { get; set; }
}

public record ConsumerPostTransitionResponse(
    string? newStatus,
    Dictionary<string, dynamic> fieldUpdates,
    string? signalRHub,
    string? signalRHubToken
    );



public record GetRecordHistoryResponse
{
    public Workflow? StateManager { get; set; }
    public ICollection<Workflow>? RunningWorkflows { get; set; }
    public ICollection<Workflow>? CompletedWorkflows { get; set; }

    public record Workflow
    {
        public Guid InstanceId { get; init; }
        public string Name { get; init; }
        public ICollection<Transition> Transitions { get; init; }

        public Workflow(string name, ICollection<Transition> transitions) => (Name, Transitions) = (name, transitions);
    }

    public record Transition
    {
        public string Name { get; init; }
        public string FromState { get; init; }
        public string ToState { get; init; }

        public DateTime CalledAt { get; init; }
        public Guid CalledBy { get; init; }

        public Transition(string name, string fromState, string toState, DateTime calledAt, Guid calledBy) => (Name, FromState, ToState, CalledAt, CalledBy) = (name, fromState, toState, calledAt, calledBy);
    }
}

public record GetRecordHistoryDetailResponse
{
    public ICollection<Transition> Transitions { get; set; }

    public GetRecordHistoryDetailResponse( ICollection<Transition> transitions) => ( Transitions) = ( transitions);


    public record Transition
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string FromState { get; init; }
        public string ToState { get; init; }
        public string FormSchema { get; init; }

        public ICollection<Event> Events { get; init; }

        public SubmitDataSet SubmitData { get; init; }
        public ResponseDataSet ResponseData { get; init; }

        public DateTime CalledAt { get; init; }
        public DateTime CompletedAt { get; init; }
        public Guid CalledBy { get; init; }

        public Transition(Guid id,
            string name,
            string fromState,
            string toState,
            string formSchema,
            ICollection<Event> events,
            SubmitDataSet submitData,
            ResponseDataSet responseData,
            DateTime ralledAt,
            DateTime completedAt,
            Guid calledBy
        ) => (Name, FromState, ToState, FormSchema, Events, SubmitData, ResponseData, CalledAt, CompletedAt, CalledBy) = (name, fromState, toState, formSchema, events, submitData, responseData, ralledAt, completedAt, calledBy);



        public record SubmitDataSet
        {
            public string? EntityData { get; init; }
            public string? FormData { get; init; }
            public string? AdditionalData { get; init; }
        }

        public record ResponseDataSet
        {
            public string? FieldUpdates { get; init; }
            public string? Status { get; init; }
        }

        public record Event
        {
            public Guid Id { get; init; }
            public string Name { get; init; }

            public DateTime ExecutedAt { get; init; }
            public int Duration { get; init; }

            public Dictionary<string, string>? InputData { get; init; }
            public Dictionary<string, string>? OutputData { get; init; }
            public Dictionary<string, string>? Details { get; init; }

            public Event(
                Guid id,
                string name,
                DateTime executedAt,
                int duration,
                Dictionary<string, string>? inputData,
                Dictionary<string, string>? outputData,
                Dictionary<string, string>? details
                ) => (Id, Name, ExecutedAt, Duration, InputData, OutputData, Details) = (id, name, executedAt, duration, inputData, outputData, details);
        }
    }
}

