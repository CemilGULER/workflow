using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPostTransactionService, PostTransactionService>();
builder.Services.AddDbContext<WorkflowDBContext>
    ((options) =>
    {
        //options.UseLazyLoadingProxies();
        options.UseNpgsql("Host=localhost:5432;Database=workflow;Username=postgres;Password=postgres");
    });

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddDaprClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.UseAllOfToExtendReferenceSchemas();
    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();
    c.CustomSchemaIds(s => s.FullName?.Replace("+", "."));

    // Add self documentation
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Add data documentation
    xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.data.xml";
    xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

var app = builder.Build();


app.UseCloudEvents();
app.UseRouting();
app.MapSubscribeHandler();


app.UseSwagger();
app.UseSwaggerUI();

app.Logger.LogInformation("Registering Routes");

app.MapConsumerEndpoints();
app.MapDefinitionEndpoints();
app.MapInstanceEndpoints();


app.Run();

