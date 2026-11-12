using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TftOfficeApp.Core.Models;
using TftOfficeApp.Core.Services;

namespace TftOfficeApp.Core.Agents;

public class ClaudeBossOrchestrator
{
    private readonly DocumentEngine _docEngine;
    private readonly Dictionary<string, TftAgentBase> _agents;
    private readonly List<ClaudeOrchestrationResult> _history = new();
    private readonly object _lock = new();

    public BossClaudeProfile BossProfile { get; }

    public ClaudeBossOrchestrator(DocumentEngine docEngine)
    {
        _docEngine = docEngine;
        _agents = new Dictionary<string, TftAgentBase>(StringComparer.OrdinalIgnoreCase)
        {
            ["galio"] = new T1GalioPdfAgent(),
            ["zeri"] = new PsyOpsZeriWordAgent(),
            ["jhin"] = new BloodMoonJhinExcelAgent(),
            ["jinx"] = new SoulFighterJinxPptxAgent()
        };

        BossProfile = new BossClaudeProfile
        {
            OfficePos = new IsoCoord(-7.5, -7.2, "Phòng Sếp Claude"),
            ConferencePos = new IsoCoord(0.0, -7.2, "Phòng Họp Chiến Thuật"),
            CurrentPos = new IsoCoord(-7.5, -7.2, "Phòng Sếp Claude"),
            Sprites = new TftAgentSprites
            {
                Portrait = "/assets/sprites/claude/idle.png",
                Idle = "/assets/sprites/claude/idle.png",
                Work = "/assets/sprites/claude/work.png",
                Walk = "/assets/sprites/claude/walk.png",
                Ult = "/assets/sprites/claude/ult.png",
                Sheet = "/assets/sprites/claude/sheet.jpg"
            }
        };
    }

    public List<TftAgentProfile> GetAgentProfiles() =>
        _agents.Values.OrderBy(a => a.Profile.VideoOrder).Select(a => a.Profile).ToList();

    public List<ClaudeOrchestrationResult> GetHistory()
    {
        lock (_lock)
        {
            return _history.OrderByDescending(h => h.Timestamp).Take(25).ToList();
        }
    }

    public async Task<ClaudeOrchestrationResult> AnalyzeAndDispatchAsync(CommandRequest request)
    {
        var userCmd = string.IsNullOrWhiteSpace(request.Command)
            ? "Tổng hợp báo cáo quý IV: sửa PDF bảo mật, soạn hợp đồng Word, lập bảng tính Excel 4 quý và thiết kế Slide PowerPoint thuyết trình."
            : request.Command.Trim();

        var selectedAgentIds = DetermineTargetAgents(userCmd, request.TargetAgent, request.AttachedFileName);
        string? liveClaudeInsight = await TryCallAnthropicApiAsync(userCmd, selectedAgentIds, request.ClaudeApiKey);

        var result = new ClaudeOrchestrationResult
        {
            UserCommand = userCmd,
            ActiveSynergies = ComputeActiveSynergies(selectedAgentIds)
        };

        var subInstructions = DecomposeInstructionsForAgents(userCmd, selectedAgentIds, request.AttachedFileName);

        var analysisBuilder = new StringBuilder();
        analysisBuilder.AppendLine($"👑 [SẾP CLAUDE — PHÂN TÍCH & ĐIỀU PHỐI MÔ HÌNH VĂN PHÒNG 3D]");
        analysisBuilder.AppendLine($"• Yêu cầu gốc: \"{userCmd}\"");
        if (!string.IsNullOrWhiteSpace(liveClaudeInsight))
        {
            analysisBuilder.AppendLine($"• Phân tích chuyên sâu (Claude API): {liveClaudeInsight}");
        }
        analysisBuilder.AppendLine($"• Các AI Agent 3D được kích hoạt ({selectedAgentIds.Count}/4): {string.Join(", ", selectedAgentIds.Select(id => _agents[id].Profile.Name))}.");

        result.ClaudeAnalysis = analysisBuilder.ToString().Trim();
        result.TacticalPlanSummary = $"Sếp Claude đã phân rã yêu cầu thành {selectedAgentIds.Count} mệnh lệnh chuyên biệt và bắn Chưởng Lực điều phối tới từng Model 3D trong văn phòng.";

        int delayMs = 0;
        int stepIdx = 1;

        result.Timeline.Add(new SimulationTimelineStep
        {
            StepIndex = stepIdx++,
            DelayMs = delayMs,
            ActorId = "claude",
            EventType = "claude_analyze",
            Pose = "work",
            Angle = "ctrl2",
            FromPos = BossProfile.OfficePos,
            ToPos = BossProfile.ConferencePos,
            Message = $"Sếp Claude: \"Đã phân tích xong! Triệu tập {selectedAgentIds.Count} AI Agent tại Phòng Họp Chiến Thuật 3D!\"",
            EffectColor = "#f59e0b"
        });

        delayMs += 900;

        foreach (var agentId in selectedAgentIds)
        {
            var agent = _agents[agentId];
            var specificInstruction = subInstructions[agentId];

            var (assignment, generatedDoc) = agent.ExecuteTask(specificInstruction, _docEngine, request.AttachedContent);
            result.Assignments.Add(assignment);
            result.GeneratedFiles.Add(generatedDoc);

            result.Timeline.Add(new SimulationTimelineStep
            {
                StepIndex = stepIdx++,
                DelayMs = delayMs,
                ActorId = "claude",
                TargetAgentId = agentId,
                EventType = "boom_dispatch",
                Pose = "ult",
                Angle = "ctrl4",
                FromPos = BossProfile.ConferencePos,
                ToPos = agent.Profile.HomeDesk,
                Message = $"Sếp Claude bắn Chưởng Lực giao việc cho {agent.Profile.Name}: \"{specificInstruction}\"",
                EffectColor = agent.Profile.ThemeColor
            });

            delayMs += 700;

            var confSpot = new IsoCoord(
                (agent.Profile.VideoOrder - 2.5) * 2.2,
                -5.2,
                "Phòng Họp Chiến Thuật"
            );

            result.Timeline.Add(new SimulationTimelineStep
            {
                StepIndex = stepIdx++,
                DelayMs = delayMs,
                ActorId = agentId,
                EventType = "agent_walk",
                Pose = "walk",
                Angle = "ctrl1",
                FromPos = agent.Profile.HomeDesk,
                ToPos = confSpot,
                Message = $"{agent.Profile.Name}: \"Rõ thưa Sếp Claude! Nhận lệnh xử lý {agent.Profile.FileExtension.ToUpperInvariant()}!\"",
                EffectColor = agent.Profile.ThemeColor
            });

            delayMs += 900;

            result.Timeline.Add(new SimulationTimelineStep
            {
                StepIndex = stepIdx++,
                DelayMs = delayMs,
                ActorId = agentId,
                EventType = "agent_work",
                Pose = "work",
                Angle = "ctrl2",
                FromPos = confSpot,
                ToPos = agent.Profile.HomeDesk,
                Message = $"{agent.Profile.Name} đang thực thi tại bàn 3D: {assignment.ExecutionSteps[1]}",
                EffectColor = agent.Profile.ThemeColor
            });

            delayMs += 1100;

            result.Timeline.Add(new SimulationTimelineStep
            {
                StepIndex = stepIdx++,
                DelayMs = delayMs,
                ActorId = agentId,
                EventType = request.TriggerFinisher ? "agent_finisher" : "agent_ult",
                Pose = "ult",
                Angle = "ctrl4",
                FromPos = agent.Profile.HomeDesk,
                ToPos = agent.Profile.HomeDesk,
                Message = $"💥 {agent.Profile.FinisherName}! Xuất bản thành công `{generatedDoc.FileName}`!",
                EffectColor = agent.Profile.SecondaryColor
            });

            delayMs += 1000;
        }

        result.Timeline.Add(new SimulationTimelineStep
        {
            StepIndex = stepIdx++,
            DelayMs = delayMs,
            ActorId = "claude",
            EventType = "file_ready",
            Pose = "idle",
            Angle = "ctrl1",
            FromPos = BossProfile.ConferencePos,
            ToPos = BossProfile.OfficePos,
            Message = $"Sếp Claude: \"Hoàn tất 100% chỉ tiêu! {result.GeneratedFiles.Count} tài liệu ({string.Join(", ", result.GeneratedFiles.Select(f => f.FileName))}) đã sẵn sàng!\"",
            EffectColor = "#10b981"
        });

        BossProfile.CommandsDispatched++;
        BossProfile.CurrentThought = $"Vừa điều phối {selectedAgentIds.Count} AI Agent 3D hoàn thành: \"{userCmd}\"";

        lock (_lock)
        {
            _history.Add(result);
        }

        return result;
    }

    public TftAgentProfile? TriggerAgentAction(string agentId, AgentActionRequest req)
    {
        if (!_agents.TryGetValue(agentId, out var agent)) return null;

        var p = agent.Profile;
        switch (req.Action.ToLowerInvariant())
        {
            case "ctrl1":
            case "ctrl2":
            case "ctrl3":
            case "ctrl4":
                p.CurrentAngle = req.Action.ToLowerInvariant();
                p.State = req.Action == "ctrl4" ? "ult" : "working";
                p.Mana = Math.Min(100, p.Mana + 20);
                break;
            case "star_up":
                p.StarLevel = p.StarLevel >= 3 ? 1 : p.StarLevel + 1;
                p.State = "ult";
                break;
            case "finisher":
                p.CurrentAngle = "ctrl4";
                p.State = "ult";
                p.Mana = 100;
                break;
            case "walk":
                if (req.TargetX.HasValue && req.TargetY.HasValue)
                {
                    p.CurrentPos = new IsoCoord(req.TargetX.Value, req.TargetY.Value, "Di chuyển 3D");
                    p.State = "walking";
                }
                break;
            case "reset_desk":
                p.CurrentPos = p.HomeDesk;
                p.State = "idle";
                p.CurrentAngle = "ctrl1";
                break;
        }

        return p;
    }

    private List<string> DetermineTargetAgents(string command, string? explicitTarget, string? attachedFileName)
    {
        if (!string.IsNullOrWhiteSpace(explicitTarget) &&
            !explicitTarget.Equals("all", StringComparison.OrdinalIgnoreCase) &&
            !explicitTarget.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            if (_agents.ContainsKey(explicitTarget))
                return new List<string> { explicitTarget.ToLowerInvariant() };
        }

        var lower = (command + " " + (attachedFileName ?? "")).ToLowerInvariant();
        var matched = new List<string>();

        if (lower.Contains("pdf") || lower.Contains("đóng dấu") || lower.Contains("in ấn") || lower.Contains("bảo mật"))
            matched.Add("galio");

        if (lower.Contains("word") || lower.Contains("wowrd") || lower.Contains("docx") || lower.Contains("doc") || lower.Contains("hợp đồng") || lower.Contains("văn bản") || lower.Contains("biên bản") || lower.Contains("soạn"))
            matched.Add("zeri");

        if (lower.Contains("excel") || lower.Contains("xlsx") || lower.Contains("xls") || lower.Contains("bảng tính") || lower.Contains("tài chính") || lower.Contains("số liệu") || lower.Contains("kpi") || lower.Contains("công thức") || lower.Contains("doanh thu"))
            matched.Add("jhin");

        if (lower.Contains("pp") || lower.Contains("pptx") || lower.Contains("powerpoint") || lower.Contains("slide") || lower.Contains("thuyết trình") || lower.Contains("trình chiếu"))
            matched.Add("jinx");

        if (matched.Count == 0 || lower.Contains("tất cả") || lower.Contains("toàn bộ") || lower.Contains("4 agent") || lower.Contains("cả 4") || explicitTarget == "all")
        {
            return new List<string> { "galio", "zeri", "jhin", "jinx" };
        }

        return matched.Distinct().ToList();
    }

    private Dictionary<string, string> DecomposeInstructionsForAgents(
        string userCmd,
        List<string> agentIds,
        string? attachedFileName)
    {
        var dict = new Dictionary<string, string>();
        var fileContext = string.IsNullOrWhiteSpace(attachedFileName) ? "" : $" (File đính kèm: {attachedFileName})";

        foreach (var id in agentIds)
        {
            dict[id] = id switch
            {
                "galio" => $"[Lệnh từ Sếp Claude -> AI Agent PDF] Sửa & chuẩn hoá hồ sơ PDF, đóng dấu bảo mật cho nhiệm vụ: {userCmd}{fileContext}",
                "zeri" => $"[Lệnh từ Sếp Claude -> AI Agent Word] Soạn thảo & sửa lỗi văn bản Word (.docx), chuẩn hoá điều khoản cho nhiệm vụ: {userCmd}{fileContext}",
                "jhin" => $"[Lệnh từ Sếp Claude -> AI Agent Excel] Xử lý bảng tính Excel (.xlsx), lập công thức 4 Quý & đối soát KPI cho nhiệm vụ: {userCmd}{fileContext}",
                "jinx" => $"[Lệnh từ Sếp Claude -> AI Agent PowerPoint] Thiết kế & sửa bộ Slide PowerPoint (.pptx) 16:9 cho nhiệm vụ: {userCmd}{fileContext}",
                _ => userCmd
            };
        }

        return dict;
    }

    private static List<string> ComputeActiveSynergies(List<string> activeAgentIds)
    {
        var list = new List<string> { "👑 Sếp Tổng Claude (1/1): Điều Phối Toàn Cục" };
        if (activeAgentIds.Contains("galio"))
            list.Add("🛡️ AI Agent PDF (#1): Chuẩn Hoá & Bảo Mật PDF");
        if (activeAgentIds.Contains("zeri"))
            list.Add("⚡ AI Agent Word (#2): Biên Tập Word Tốc Độ Cao");
        if (activeAgentIds.Contains("jhin"))
            list.Add("🎭 AI Agent Excel (#3): Bảng Tính & Công Thức 4 Quý");
        if (activeAgentIds.Contains("jinx"))
            list.Add("🔥 AI Agent PowerPoint (#4): Thiết Kế Slide Thuyết Trình");
        return list;
    }

    private async Task<string?> TryCallAnthropicApiAsync(string userCmd, List<string> agents, string? requestKey)
    {
        var apiKey = !string.IsNullOrWhiteSpace(requestKey)
            ? requestKey
            : Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            http.DefaultRequestHeaders.Add("x-api-key", apiKey);
            http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var payload = new
            {
                model = "claude-3-5-sonnet-20241022",
                max_tokens = 220,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = $"Bạn là Sếp Claude quản lý văn phòng 4 AI Agent 3D: AI Agent PDF (sửa PDF), AI Agent Word (sửa Word), AI Agent Excel (sửa Excel), AI Agent PowerPoint (sửa PowerPoint). Hãy tóm tắt ngắn gọn 2 câu chiến lược phân công cho yêu cầu: {userCmd}"
                    }
                }
            };

            var res = await http.PostAsync(
                "https://api.anthropic.com/v1/messages",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            if (!res.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
        }
        catch
        {
            return null;
        }
    }
}
