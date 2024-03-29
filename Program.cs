using UvA.Ans.ExtraTimeWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AnsConfig>(builder.Configuration.GetSection("Ans"));
builder.Services.Configure<SisConfig>(builder.Configuration.GetSection("Sis"));

builder.Services.AddScoped<AnsClientFactory>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();