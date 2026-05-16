using Regira.McpServer;

var builder = WebApplication.CreateBuilder(args);

var kbPath = builder.Configuration["KnowledgeBasePath"]
    ?? Path.Combine(AppContext.BaseDirectory, "knowledge-base.json");

var kb = KnowledgeBase.Load(kbPath);
builder.Services.AddSingleton(kb);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<PackageTools>();

var app = builder.Build();

app.MapMcp("/mcp");

app.MapGet("/health", (KnowledgeBase kb) => new
{
    status = "ok",
    packages = kb.PackageCount,
    packagesWithDocs = kb.PackagesWithDocs,
    generated = kb.Generated,
});

app.Run();
