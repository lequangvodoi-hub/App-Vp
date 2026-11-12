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
            ? "Tổng hợp báo cáo quý IV: sửa PDF bảo mật, soạn hợp đồng Word, lập bảng tính Excel 4 nhịp và thiết kế Slide PowerPoint thuyết trình."
            : request.Command.Trim();

        var selectedAgentIds = DetermineTargetAgents(userCmd, request.TargetAgent, request.AttachedFileName);

        // Optional live Anthropic Claude API call if key is provided
        string? liveClaudeInsight = await TryCallAnthropicApiAsync(userCmd, selectedAgentIds, request.ClaudeApiKey);

        var result = new ClaudeOrchestrationResult
        {
            UserCommand = userCmd,
            ActiveSynergies = ComputeActiveSynergies(selectedAgentIds)
        };

        var subInstructions = DecomposeInstructionsForAgents(userCmd, selectedAgentIds, request.AttachedFileName);

        var analysisBuilder = new StringBuilder();
        analysisBuilder.AppendLine($"👑 [SẾP CLAUDE — PHÂN TÍCH CHIẾN THUẬT VĂN PHÒNG ĐTCL]");
        analysisBuilder.AppendLine($"• Yêu cầu gốc từ người dùng: \"{userCmd}\"");
        if (!string.IsNullOrWhiteSpace(liveClaudeInsight))
        {
            analysisBuilder.AppendLine($"• Phân tích chuyên sâu (Claude API): {liveClaudeInsight}");
        }
        analysisBuilder.AppendLine($"• Đội hình Linh Thú Tí Nị được kích hoạt ({selectedAgentIds.Count}/4 Agent): {string.Join(", ", selectedAgentIds.Select(id => _agents[id].Profile.Name))}.");
        analysisBuilder.AppendLine($"• Tộc/Hệ kích hoạt: {string.Join(" + ", result.ActiveSynergies)}.");

        result.ClaudeAnalysis = analysisBuilder.ToString().Trim();
        result.TacticalPlanSummary = $"Sếp Claude đã phân rã yêu cầu thành {selectedAgentIds.Count} mệnh lệnh chuyên biệt và bắn Chưởng Lực điều phối tới từng bàn làm việc trong văn phòng 3D.";

        int delayMs = 0;
        int stepIdx = 1;

        // Step 1: Boss Claude analyzes at Manager's Office & walks to Conference Room
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
            Message = $"Sếp Claude: \"Đã nhận lệnh! Đang phân tích yêu cầu và triệu tập {selectedAgentIds.Count} Linh Thú AI Agent tại Phòng Họp Chiến Thuật!\"",
            EffectColor = "#f59e0b"
        });

        delayMs += 900;

        // Step 2: Each selected agent receives Boom dispatch, walks to Conference Room, works at desk, uses Ult/Finisher, and outputs file
        foreach (var agentId in selectedAgentIds)
        {
            var agent = _agents[agentId];
            var specificInstruction = subInstructions[agentId];

            var (assignment, generatedDoc) = agent.ExecuteTask(specificInstruction, _docEngine, request.AttachedContent);
            result.Assignments.Add(assignment);
            result.GeneratedFiles.Add(generatedDoc);

            // Claude fires Boom command projectile to agent
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
                Message = $"Sếp Claude giao việc cho {agent.Profile.Name}: \"{specificInstruction}\"",
                EffectColor = agent.Profile.ThemeColor
            });

            delayMs += 700;

            // Agent walks to Conference Room briefly or acknowledges
            var confSpot = new IsoCoord(
                BossProfile.ConferencePos.X + (agent.Profile.VideoOrder - 2.5) * 3.2,
                BossProfile.ConferencePos.Y + 4.5,
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

            // Agent walks back to their specialized desk and enters working state (Ctrl+2 / Ctrl+3)
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
                Message = $"{agent.Profile.Name} đang thực thi: {assignment.ExecutionSteps[1]}",
                EffectColor = agent.Profile.ThemeColor
            });

            delayMs += 1100;

            // Agent casts Ultimate / Finisher skill (Ctrl+4) and produces the document
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
                Message = $"💥 {agent.Profile.FinisherName}! Hoàn tất file `{generatedDoc.FileName}`!",
                EffectColor = agent.Profile.SecondaryColor
            });

            delayMs += 1000;
        }

        // Final step: Boss Claude returns to Manager's Office & announces completion
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
            Message = $"Sếp Claude: \"Hoàn tất 100% chỉ tiêu! {result.GeneratedFiles.Count} tài liệu ({string.Join(", ", result.GeneratedFiles.Select(f => f.FileName))}) đã sẵn sàng tải về!\"",
            EffectColor = "#10b981"
        });

        BossProfile.CommandsDispatched++;
        BossProfile.CurrentThought = $"Vừa điều phối {selectedAgentIds.Count} Linh Thú hoàn thành: \"{userCmd}\"";

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
                    p.CurrentPos = new IsoCoord(
                        Math.Clamp(req.TargetX.Value, 8, 92),
                        Math.Clamp(req.TargetY.Value, 12, 88),
                        "Di chuyển chiến thuật"
                    );
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

        if (lower.Contains("pdf") || lower.Contains("galio") || lower.Contains("t1") || lower.Contains("đóng dấu") || lower.Contains("in ấn") || lower.Contains("bảo mật"))
            matched.Add("galio");

        if (lower.Contains("word") || lower.Contains("wowrd") || lower.Contains("docx") || lower.Contains("doc") || lower.Contains("zeri") || lower.Contains("hợp đồng") || lower.Contains("văn bản") || lower.Contains("biên bản") || lower.Contains("soạn"))
            matched.Add("zeri");

        if (lower.Contains("excel") || lower.Contains("xlsx") || lower.Contains("xls") || lower.Contains("jhin") || lower.Contains("bảng tính") || lower.Contains("tài chính") || lower.Contains("số liệu") || lower.Contains("kpi") || lower.Contains("công thức") || lower.Contains("doanh thu"))
            matched.Add("jhin");

        if (lower.Contains("pp") || lower.Contains("pptx") || lower.Contains("powerpoint") || lower.Contains("slide") || lower.Contains("jinx") || lower.Contains("thuyết trình") || lower.Contains("trình chiếu"))
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
                "galio" => $"[Lệnh từ Sếp Claude -> T1 Galio] Sửa & chuẩn hoá hồ sơ PDF, đóng dấu bảo mật Khiên Durand cho nhiệm vụ: {userCmd}{fileContext}",
                "zeri" => $"[Lệnh từ Sếp Claude -> PsyOps Zeri] Soạn thảo & sửa lỗi văn bản Word (.docx), chuẩn hoá điều khoản cho nhiệm vụ: {userCmd}{fileContext}",
                "jhin" => $"[Lệnh từ Sếp Claude -> Blood Moon Jhin] Xử lý bảng tính Excel (.xlsx), lập công thức 4 Quý & đối soát KPI cho nhiệm vụ: {userCmd}{fileContext}",
                "jinx" => $"[Lệnh từ Sếp Claude -> Soul Fighter Jinx] Thiết kế & sửa bộ Slide PowerPoint (.pptx) Lam Hoả Bộc Phá cho nhiệm vụ: {userCmd}{fileContext}",
                _ => userCmd
            };
        }

        return dict;
    }

    private static List<string> ComputeActiveSynergies(List<string> activeAgentIds)
    {
        var list = new List<string> { "👑 Sếp Tổng Claude (1/1): +100% Trí Tuệ Điều Phối" };
        if (activeAgentIds.Contains("galio"))
            list.Add("🛡️ T1 Di Sản (Galio 18.5): Bảo Mật & Chuẩn Hoá PDF Tuyệt Đối");
        if (activeAgentIds.Contains("zeri"))
            list.Add("⚡ Đặc Vụ Siêu Linh (Zeri 18.4): Biên Tập Word Tốc Độ Tia Chớp");
        if (activeAgentIds.Contains("jhin"))
            list.Add("🎭 Nghệ Nhân Huyết Nguyệt (Jhin 18.3): Chuẩn Xác Tuyệt Đối Số 4 Excel");
        if (activeAgentIds.Contains("jinx"))
            list.Add("🔥 Tinh Võ Sư Lam Hoả (Jinx 18.2): Slide PowerPoint Bùng Nổ");
        if (activeAgentIds.Count == 4)
            list.Add("🌟 ĐỘI HÌNH HOÀN HẢO (4/4 Linh Thú Tí Nị): Kích hoạt Cổng Dịch Chuyển Đa Vũ Trụ Văn Phòng!");
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
                        content = $"Bạn là Sếp Claude quản lý văn phòng 4 Linh Thú ĐTCL: T1 Galio (sửa PDF), Zeri Siêu Linh (sửa Word), Jhin Huyết Nguyệt (sửa Excel), Jinx Lam Hoả (sửa PowerPoint). Hãy tóm tắt ngắn gọn 2 câu chiến lược phân công cho yêu cầu: {userCmd}"
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
