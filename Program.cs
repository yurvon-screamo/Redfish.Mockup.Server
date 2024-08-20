using DotnetRedfish;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

IConfigurationSection section = builder.Configuration.GetSection("ContentRelativePath");
builder.Services.Configure<string>(section);
string? contentRelativePath = section.Get<string>();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppSerializerContext.Default));

builder.Services.AddSingleton<EndpointHandler>(s => new(
    contentRelativePath,
    s.GetRequiredService<ILogger<EndpointHandler>>()));

WebApplication app = builder.Build();

EndpointHandler handler = app.Services.GetRequiredService<EndpointHandler>();

app.MapGet("/redfish/v1/{**catch-all}", handler.Handle);

app.Run();
