using System.Text.Json.Serialization;

using StepCa.Client;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.WebHost.UseKestrelHttpsConfiguration();
builder.WebHost.ConfigureKestrel(d => d.ConfigureEndpointDefaults(a =>
{
    a.UseHttps();
}));

StepCaOption options = new()
{
    Domain = "localhost",
    StepCaHost = "step-ca",
    StepCaPort = 443,
    Fingerprint = "44f640d55de5c2d862ac71a2c8c8d618cc89d4b1d50a9a127c326b3faf518d6c",
    StepCaProvisioner = "admin",
    StepCaProvisionerPassword = "password",
};

await builder.ConfigureStepCaHotreload(options);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

WebApplication app = builder.Build();

Todo[] sampleTodos = [
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
];

RouteGroupBuilder todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
