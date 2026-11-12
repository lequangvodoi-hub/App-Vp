using Microsoft.AspNetCore.StaticFiles;
using TftOfficeApp.Core.Agents;
using TftOfficeApp.Core.Cli;
using TftOfficeApp.Core.Models;
using TftOfficeApp.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DocumentEngine>();
builder.Services.AddSingleton<ClaudeBossOrchestrator>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

// Check if invoked in C# CLI mode (--cli or --command "...")
if (args.Contains("--cli", StringComparer.OrdinalIgnoreCase) ||
    args.Contains("--command", StringComparer.OrdinalIgnoreCase))
{
    var orchestrator = app.Services.GetRequiredService<ClaudeBossOrchestrator>();
    string? singleCmd = null;
    int cmdIdx = Array.FindIndex(args, a => a.Equals("--command", StringComparison.OrdinalIgnoreCase));
    if (cmdIdx >= 0 && cmdIdx + 1 < args.Length)
    {
        singleCmd = args[cmdIdx + 1];
    }
    await TftCliRunner.RunCliAsync(orchestrator, singleCmd);
    return;
}

var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
contentTypeProvider.Mappings[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
contentTypeProvider.Mappings[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
contentTypeProvider.Mappings[".pdf"] = "application/pdf";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.MapGet("/api/state", (ClaudeBossOrchestrator orchestrator, DocumentEngine docEngine) =>
{
    return Results.Ok(new
    {
        boss = orchestrator.BossProfile,
        agents = orchestrator.GetAgentProfiles(),
        documents = docEngine.GetDocuments(),
        history = orchestrator.GetHistory(),
        officeZones = new[]
        {
            new { id = "claude_office", name = "Phòng Sếp Claude (Manager's Office)", x = 30.5, y = 34.5, icon = "👑" },
            new { id = "conference", name = "Phòng Họp Chiến Thuật (Conference Room)", x = 44.5, y = 29.0, icon = "📋" },
            new { id = "desk_galio", name = "Bàn 1: Sửa PDF (T1 Galio Tí Nị)", x = 29.5, y = 56.5, icon = "🛡️" },
            new { id = "desk_zeri", name = "Bàn 2: Sửa Word (PsyOps Zeri Tí Nị)", x = 40.5, y = 52.0, icon = "⚡" },
            new { id = "desk_jhin", name = "Bàn 3: Sửa Excel (Blood Moon Jhin Tí Nị)", x = 50.5, y = 47.0, icon = "🎭" },
            new { id = "desk_jinx", name = "Bàn 4: Sửa PowerPoint (Soul Fighter Jinx Tí Nị)", x = 47.5, y = 68.5, icon = "🔥" },
            new { id = "vault", name = "Kho Lưu Trữ Tài Liệu & Cổng Dịch Chuyển", x = 79.0, y = 19.5, icon = "🌀" }
        }
    });
});

app.MapPost("/api/command", async (CommandRequest request, ClaudeBossOrchestrator orchestrator) =>
{
    var result = await orchestrator.AnalyzeAndDispatchAsync(request);
    return Results.Ok(result);
});

app.MapPost("/api/agent/{id}/action", (string id, AgentActionRequest request, ClaudeBossOrchestrator orchestrator) =>
{
    var updated = orchestrator.TriggerAgentAction(id, request);
    return updated is not null ? Results.Ok(updated) : Results.NotFound(new { error = "Agent not found" });
});

app.MapGet("/api/files", (DocumentEngine docEngine) =>
{
    return Results.Ok(docEngine.GetDocuments());
});

app.Run();
