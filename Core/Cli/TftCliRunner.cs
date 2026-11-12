using TftOfficeApp.Core.Agents;
using TftOfficeApp.Core.Models;

namespace TftOfficeApp.Core.Cli;

public static class TftCliRunner
{
    public static async Task RunCliAsync(ClaudeBossOrchestrator orchestrator, string? singleCommand = null)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintHeader(orchestrator);

        if (!string.IsNullOrWhiteSpace(singleCommand))
        {
            await ExecuteAndPrintAsync(orchestrator, singleCommand);
            return;
        }

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n[Ra lệnh cho Sếp Claude (hoặc gõ 'exit' để thoát)] > ");
            Console.ResetColor();

            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Trim().Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Tạm biệt từ Văn Phòng Linh Thú ĐTCL & Sếp Claude!");
                break;
            }

            await ExecuteAndPrintAsync(orchestrator, input);
        }
    }

    private static void PrintHeader(ClaudeBossOrchestrator orchestrator)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     TFT AI AGENT OFFICE SIMULATOR — C# CORE & CLI ENGINE (SẾP CLAUDE ORCHESTRATOR)   ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  👑 Sếp Tổng : Claude (Regional Manager & TFT Tactician Orchestrator)                ║");
        foreach (var a in orchestrator.GetAgentProfiles())
        {
            var line = $"  [{a.VideoOrder}] {a.Name,-28} | {a.Role,-38} ({a.FileExtension})";
            Console.WriteLine($"║{line,-86}║");
        }
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    private static async Task ExecuteAndPrintAsync(ClaudeBossOrchestrator orchestrator, string command)
    {
        var result = await orchestrator.AnalyzeAndDispatchAsync(new CommandRequest
        {
            Command = command,
            TriggerFinisher = true
        });

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(result.ClaudeAnalysis);
        Console.ResetColor();

        Console.WriteLine("\n--- TIẾN TRÌNH ĐIỀU PHỐI TRONG VĂN PHÒNG 3D ISOMETRIC ---");
        foreach (var step in result.Timeline)
        {
            Console.ForegroundColor = step.ActorId == "claude" ? ConsoleColor.Yellow : ConsoleColor.Green;
            Console.WriteLine($"  [{step.DelayMs,4}ms] ({step.ActorId.ToUpperInvariant()} | {step.Angle.ToUpperInvariant()}) {step.Message}");
        }
        Console.ResetColor();

        Console.WriteLine("\n--- KẾT QUẢ XỬ LÝ CỦA CÁC LINH THÚ TÍ NỊ ---");
        foreach (var assign in result.Assignments)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n★ {assign.AgentName} [{assign.Specialty}]");
            Console.ResetColor();
            Console.WriteLine($"  • Kỹ năng ĐTCL : {assign.TftSkillUsed}");
            Console.WriteLine($"  • Chưởng lực   : {assign.BoomEffect}");
            foreach (var s in assign.ExecutionSteps)
            {
                Console.WriteLine($"    -> {s}");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✔ File xuất ra : wwwroot/workspace/{assign.OutputFileName}");
            Console.ResetColor();
        }
    }
}
