using System.Text.Json.Serialization;

namespace TftOfficeApp.Core.Models;

public record IsoCoord(double X, double Y, string ZoneName);

public class TftAgentSprites
{
    public string Portrait { get; set; } = "";
    public string Idle { get; set; } = "";
    public string Work { get; set; } = "";
    public string Walk { get; set; } = "";
    public string Ult { get; set; } = "";
    public string Sheet { get; set; } = "";
    public Dictionary<string, string> VideoAngles { get; set; } = new();
    public Dictionary<string, string> VideoClips { get; set; } = new();
}

public class TftAgentProfile
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string EnglishName { get; set; } = "";
    public string Role { get; set; } = "";
    public string Specialty { get; set; } = ""; // pdf, word, excel, pptx
    public string FileExtension { get; set; } = "";
    public string PatchVersion { get; set; } = "";
    public string VideoUrl { get; set; } = "";
    public int VideoOrder { get; set; }
    public string Rarity { get; set; } = "Huyền Thoại (Mythic)";
    public int StarLevel { get; set; } = 2; // 1, 2, 3 stars
    public int Mana { get; set; } = 40;
    public int MaxMana { get; set; } = 100;
    public List<string> Traits { get; set; } = new();
    public string BoomName { get; set; } = "";
    public string FinisherName { get; set; } = "";
    public string PortalName { get; set; } = "";
    public string ThemeColor { get; set; } = "#f59e0b";
    public string SecondaryColor { get; set; } = "#ef4444";
    public string Quote { get; set; } = "";
    public IsoCoord HomeDesk { get; set; } = new(50, 50, "Desk");
    public IsoCoord CurrentPos { get; set; } = new(50, 50, "Desk");
    public string State { get; set; } = "idle"; // idle, walking, briefing, working, ult, completed
    public string CurrentAngle { get; set; } = "ctrl1"; // ctrl1, ctrl2, ctrl3, ctrl4
    public string CurrentTaskTitle { get; set; } = "Đang trực tại bàn làm việc";
    public int TasksCompleted { get; set; } = 0;
    public List<string> Capabilities { get; set; } = new();
    public TftAgentSprites Sprites { get; set; } = new();
}

public class BossClaudeProfile
{
    public string Id { get; set; } = "claude";
    public string Name { get; set; } = "Sếp Claude (Regional Manager)";
    public string Title { get; set; } = "Giám Đốc Điều Hành & Kiến Trúc Sư Chiến Thuật ĐTCL";
    public string Model { get; set; } = "Claude 3.7 Sonnet / Opus Orchestrator";
    public IsoCoord OfficePos { get; set; } = new(30.5, 34.5, "Phòng Sếp Claude (Manager's Office)");
    public IsoCoord ConferencePos { get; set; } = new(44.5, 29.0, "Phòng Họp Chiến Thuật (Conference Room)");
    public IsoCoord CurrentPos { get; set; } = new(30.5, 34.5, "Phòng Sếp Claude");
    public string State { get; set; } = "idle";
    public string CurrentThought { get; set; } = "Sẵn sàng nhận lệnh từ bạn để phân tích và điều phối 4 Linh Thú AI Agent.";
    public int CommandsDispatched { get; set; } = 0;
    public TftAgentSprites Sprites { get; set; } = new();
}

public class SubTaskAssignment
{
    public string SubTaskId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string AgentId { get; set; } = "";
    public string AgentName { get; set; } = "";
    public string Specialty { get; set; } = "";
    public string ActionType { get; set; } = ""; // edit, generate, analyze, convert, format
    public string Instruction { get; set; } = "";
    public string TftSkillUsed { get; set; } = "";
    public string BoomEffect { get; set; } = "";
    public string Status { get; set; } = "completed";
    public string OutputFileName { get; set; } = "";
    public string OutputSummary { get; set; } = "";
    public List<string> ExecutionSteps { get; set; } = new();
    public long DurationMs { get; set; }
}

public class SimulationTimelineStep
{
    public int StepIndex { get; set; }
    public int DelayMs { get; set; }
    public string ActorId { get; set; } = "";
    public string EventType { get; set; } = ""; // claude_analyze, summon_conference, boom_dispatch, agent_walk, agent_work, agent_ult, agent_finisher, file_ready
    public string Pose { get; set; } = "idle";
    public string Angle { get; set; } = "ctrl1";
    public IsoCoord? FromPos { get; set; }
    public IsoCoord? ToPos { get; set; }
    public string Message { get; set; } = "";
    public string? EffectColor { get; set; }
    public string? TargetAgentId { get; set; }
}

public class ClaudeOrchestrationResult
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString("N")[..10];
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserCommand { get; set; } = "";
    public string ClaudeAnalysis { get; set; } = "";
    public string TacticalPlanSummary { get; set; } = "";
    public List<string> ActiveSynergies { get; set; } = new();
    public List<SubTaskAssignment> Assignments { get; set; } = new();
    public List<SimulationTimelineStep> Timeline { get; set; } = new();
    public List<GeneratedDocumentInfo> GeneratedFiles { get; set; } = new();
}

public class GeneratedDocumentInfo
{
    public string FileId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string FileName { get; set; } = "";
    public string FileType { get; set; } = ""; // pdf, docx, xlsx, pptx
    public string CreatedByAgentId { get; set; } = "";
    public string CreatedByAgentName { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DownloadUrl { get; set; } = "";
    public string PreviewText { get; set; } = "";
    public List<string> Highlights { get; set; } = new();
}

public class CommandRequest
{
    public string Command { get; set; } = "";
    public string? TargetAgent { get; set; } // null or "all" or "galio"/"zeri"/"jhin"/"jinx"
    public string? AttachedFileName { get; set; }
    public string? AttachedContent { get; set; }
    public string? ClaudeApiKey { get; set; }
    public bool TriggerFinisher { get; set; } = true;
}

public class AgentActionRequest
{
    public string Action { get; set; } = "emote"; // ctrl1, ctrl2, ctrl3, ctrl4, star_up, finisher, walk
    public double? TargetX { get; set; }
    public double? TargetY { get; set; }
}
