using System.Diagnostics;
using TftOfficeApp.Core.Models;
using TftOfficeApp.Core.Services;

namespace TftOfficeApp.Core.Agents;

public abstract class TftAgentBase
{
    public TftAgentProfile Profile { get; protected set; } = new();

    public abstract (SubTaskAssignment Assignment, GeneratedDocumentInfo Document) ExecuteTask(
        string instruction,
        DocumentEngine docEngine,
        string? attachedContent = null);

    protected static TftAgentSprites BuildSprites(string id, string portraitFile)
    {
        return new TftAgentSprites
        {
            Portrait = $"/assets/portraits/{portraitFile}",
            Idle = $"/assets/sprites/{id}/idle.png",
            Work = $"/assets/sprites/{id}/work.png",
            Walk = $"/assets/sprites/{id}/walk.png",
            Ult = $"/assets/sprites/{id}/ult.png",
            Sheet = $"/assets/sprites/{id}/sheet.jpg",
            VideoAngles = new Dictionary<string, string>
            {
                ["ctrl1"] = $"/assets/video_angles/{id}/ctrl1.jpg",
                ["ctrl2"] = $"/assets/video_angles/{id}/ctrl2.jpg",
                ["ctrl3"] = $"/assets/video_angles/{id}/ctrl3.jpg",
                ["ctrl4"] = $"/assets/video_angles/{id}/ctrl4.jpg",
                ["finisher"] = $"/assets/video_angles/{id}/finisher.jpg",
                ["boom"] = $"/assets/video_angles/{id}/boom.jpg"
            },
            VideoClips = new Dictionary<string, string>
            {
                ["finisher"] = $"/assets/clips/{id}/finisher.mp4",
                ["showcase"] = $"/assets/clips/{id}/showcase.mp4",
                ["boom"] = $"/assets/clips/{id}/boom.mp4"
            }
        };
    }
}

/// <summary>
/// Video 1 in Playlist: Cửa Hàng Xoay Tua 18.5 - T1 Galio Tí Nị (Chibi T1 Galio)
/// Role: Chuyên Gia Sửa & Chuẩn Hoá PDF
/// </summary>
public class T1GalioPdfAgent : TftAgentBase
{
    public T1GalioPdfAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "galio",
            Name = "T1 Galio Tí Nị",
            EnglishName = "Chibi T1 Galio",
            Role = "Chuyên Gia Sửa PDF & Đóng Dấu Bảo Mật",
            Specialty = "pdf",
            FileExtension = ".pdf",
            PatchVersion = "ĐTCL 18.5 (Video #1)",
            VideoUrl = "https://www.youtube.com/watch?v=gMZ7AfAGuhY&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 1,
            Rarity = "Huyền Thoại T1 (Worlds Champion)",
            StarLevel = 3,
            Mana = 80,
            MaxMana = 100,
            Traits = new List<string> { "T1 Di Sản", "Vệ Quân PDF", "Siêu Hùng Giáng Thế" },
            BoomName = "Chưởng Lực: T1 Siêu Hùng Giáng Thế (Hero's Entrance)",
            FinisherName = "Kết Liễu: Vương Miện Quỷ Vương T1 & Đại Địa Chấn",
            PortalName = "Cổng Dịch Chuyển: T1 Vùng Đất Di Sản",
            ThemeColor = "#e11d48",
            SecondaryColor = "#f59e0b",
            Quote = "Khiên Durand bảo vệ mọi trang PDF! Không một lỗi lệch lề hay thiếu chữ ký nào lọt qua được T1 Galio!",
            HomeDesk = new IsoCoord(29.5, 56.5, "Bàn 1 — Khu PDF (T1 Galio)"),
            CurrentPos = new IsoCoord(29.5, 56.5, "Bàn 1 — Khu PDF (T1 Galio)"),
            Capabilities = new List<string>
            {
                "Sửa nội dung, căn chỉnh bố cục A4 và nhúng font chuẩn vào file PDF",
                "Đóng dấu bản quyền T1 & chữ ký bảo mật chống chỉnh sửa",
                "Nén dung lượng PDF, nối/tách trang và trích xuất bảng biểu chiến thuật",
                "Chuyển đổi báo cáo tổng hợp thành hồ sơ PDF chuẩn doanh nghiệp"
            },
            Sprites = BuildSprites("galio", "galio.png")
        };
    }

    public override (SubTaskAssignment Assignment, GeneratedDocumentInfo Document) ExecuteTask(
        string instruction,
        DocumentEngine docEngine,
        string? attachedContent = null)
    {
        var sw = Stopwatch.StartNew();
        var stamp = DateTime.UtcNow.ToString("HHmmss");
        var fileName = $"T1_Galio_PDF_Report_{stamp}.pdf";

        var highlights = new List<string>
        {
            $"Áp dụng Khiên Durand chuẩn hoá cấu trúc PDF theo yêu cầu: \"{instruction}\"",
            "Đóng dấu xác thực T1 Worlds Championship (Gold/Crimson Vector Header) ở đầu trang.",
            "Cố định lề chuẩn ISO A4, kiểm tra toàn vẹn bảng mã cross-reference (xref) và nhúng siêu dữ liệu.",
            "Hoàn tất kiểm định bảo mật PDF 1.4 — sẵn sàng in ấn hoặc gửi đối tác cấp cao."
        };

        var doc = docEngine.GeneratePdfDocument(
            fileName,
            $"HỒ SƠ PDF ĐÃ XỬ LÝ: {instruction.ToUpperInvariant()}",
            instruction,
            highlights,
            attachedContent
        );
        sw.Stop();

        Profile.TasksCompleted++;
        Profile.Mana = 100;
        Profile.CurrentTaskTitle = $"Đã xuất bản {doc.FileName}";

        var assignment = new SubTaskAssignment
        {
            AgentId = Profile.Id,
            AgentName = Profile.Name,
            Specialty = "Sửa & Chuẩn Hoá PDF (.pdf)",
            ActionType = "edit_pdf",
            Instruction = instruction,
            TftSkillUsed = "Siêu Hùng Giáng Thế (Hero's Entrance - Ctrl+4 & Finisher)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"T1 Galio Tí Nị đã sửa & đóng dấu bảo mật file PDF `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Góc Ctrl+1] Khởi động đôi cánh vàng-đỏ T1, quét cấu trúc luồng đối tượng PDF.",
                "[Góc Ctrl+2] Dùng Lá Chắn Durand căn chỉnh lại toàn bộ khung trang A4 và tiêu đề.",
                "[Góc Ctrl+3] Triệu hồi ấn chương Quỷ Vương T1 đóng dấu xác thực lên đầu tài liệu.",
                "[Finisher 18.5] Siêu Hùng Giáng Thế! Xuất file PDF hoàn chỉnh với bảng xref chuẩn xác."
            },
            DurationMs = Math.Max(18, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}

/// <summary>
/// Video 2 in Playlist: Cửa Hàng Xoay Tua 18.4 - Zeri Đặc Vụ Siêu Linh Tí Nị (Chibi PsyOps Zeri)
/// Role: Chuyên Gia Sửa & Soạn Thảo Word (.docx)
/// </summary>
public class PsyOpsZeriWordAgent : TftAgentBase
{
    public PsyOpsZeriWordAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "zeri",
            Name = "Zeri Đặc Vụ Siêu Linh Tí Nị",
            EnglishName = "Chibi PsyOps Zeri",
            Role = "Chuyên Gia Sửa Word & Biên Tập Hợp Đồng",
            Specialty = "word",
            FileExtension = ".docx",
            PatchVersion = "ĐTCL 18.4 (Video #2)",
            VideoUrl = "https://www.youtube.com/watch?v=k_k_bQd-NDQ&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 2,
            Rarity = "Huyền Thoại Siêu Linh (PsyOps Mythic)",
            StarLevel = 2,
            Mana = 75,
            MaxMana = 100,
            Traits = new List<string> { "Đặc Vụ Siêu Linh", "Biên Tập Tốc Độ Ánh Sáng", "Xung Điện Word" },
            BoomName = "Chưởng Lực: Tia Chớp Siêu Linh (PsyOps Lightning Surge)",
            FinisherName = "Kết Liễu: Điện Đạt Đỉnh Điểm — Giải Mã Hồ Sơ Tuyệt Mật",
            PortalName = "Cổng Dịch Chuyển: Căn Cứ Đặc Vụ Siêu Linh",
            ThemeColor = "#06b6d4",
            SecondaryColor = "#a855f7",
            Quote = "Tốc độ gõ Word 1.000 từ/giây! Mọi lỗi câu chữ và định dạng .docx đều bị tia chớp Siêu Linh xử đẹp!",
            HomeDesk = new IsoCoord(40.5, 52.0, "Bàn 2 — Khu Word (PsyOps Zeri)"),
            CurrentPos = new IsoCoord(40.5, 52.0, "Bàn 2 — Khu Word (PsyOps Zeri)"),
            Capabilities = new List<string>
            {
                "Sửa lỗi chính tả, ngữ pháp và nâng cấp văn phong chuyên nghiệp trên file Word (.docx)",
                "Định dạng chuẩn OpenXML: Heading 1/2/3, bảng điều khoản, mục lục và header/footer",
                "Soạn thảo hợp đồng, công văn, biên bản cuộc họp và tài liệu quy trình",
                "Tái cấu trúc nội dung thô thành tài liệu Word hoàn chỉnh chỉ trong 1 xung điện"
            },
            Sprites = BuildSprites("zeri", "zeri.png")
        };
    }

    public override (SubTaskAssignment Assignment, GeneratedDocumentInfo Document) ExecuteTask(
        string instruction,
        DocumentEngine docEngine,
        string? attachedContent = null)
    {
        var sw = Stopwatch.StartNew();
        var stamp = DateTime.UtcNow.ToString("HHmmss");
        var fileName = $"PsyOps_Zeri_Word_{stamp}.docx";

        var highlights = new List<string>
        {
            $"Đã biên tập & chuẩn hoá văn bản Word theo lệnh: \"{instruction}\"",
            "Tự động rà soát chính tả, tối ưu cấu trúc câu và áp dụng Style Heading chuẩn doanh nghiệp.",
            "Thiết lập phân đoạn điều khoản rõ ràng, căn lề chuẩn A4 (1 inch margin) trong cấu trúc OpenXML.",
            "Đính kèm xác nhận bảo mật Đặc Vụ Siêu Linh (PsyOps Classified Verification)."
        };

        var doc = docEngine.GenerateDocxDocument(
            fileName,
            $"VĂN BẢN WORD (.DOCX): {instruction.ToUpperInvariant()}",
            instruction,
            highlights,
            attachedContent
        );
        sw.Stop();

        Profile.TasksCompleted++;
        Profile.Mana = 100;
        Profile.CurrentTaskTitle = $"Đã biên tập {doc.FileName}";

        var assignment = new SubTaskAssignment
        {
            AgentId = Profile.Id,
            AgentName = Profile.Name,
            Specialty = "Sửa & Biên Tập Word (.docx)",
            ActionType = "edit_docx",
            Instruction = instruction,
            TftSkillUsed = "Điện Đạt Đỉnh Điểm (Lightning Crash - Ctrl+2 & Finisher)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"Zeri Đặc Vụ Siêu Linh Tí Nị đã sửa & định dạng tài liệu Word `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Góc Ctrl+1] Nạp năng lượng từ trường Siêu Linh, phân tích ngữ nghĩa văn bản.",
                "[Góc Ctrl+2] Lướt tia chớp qua từng đoạn văn, sửa lỗi từ vựng và căn chỉnh Style Heading.",
                "[Góc Ctrl+3] Đóng gói cấu trúc OpenXML `word/document.xml` với bảng biểu và điều khoản chuẩn.",
                "[Finisher 18.4] Điện Đạt Đỉnh Điểm! Xuất bản file `.docx` hoàn chỉnh trong chớp mắt."
            },
            DurationMs = Math.Max(15, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}

/// <summary>
/// Video 3 in Playlist: Cửa Hàng Xoay Tua 18.3 - Jhin Huyết Nguyệt Tí Nị (Chibi Blood Moon Jhin)
/// Role: Chuyên Gia Sửa Excel & Phân Tích Số Liệu (.xlsx)
/// </summary>
public class BloodMoonJhinExcelAgent : TftAgentBase
{
    public BloodMoonJhinExcelAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "jhin",
            Name = "Jhin Huyết Nguyệt Tí Nị",
            EnglishName = "Chibi Blood Moon Jhin",
            Role = "Chuyên Gia Sửa Excel & Nghệ Sĩ Công Thức Số 4",
            Specialty = "excel",
            FileExtension = ".xlsx",
            PatchVersion = "ĐTCL 18.3 (Video #3)",
            VideoUrl = "https://www.youtube.com/watch?v=gU-FPO7pJ90&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 3,
            Rarity = "Huyền Thoại Huyết Nguyệt (Blood Moon Mythic)",
            StarLevel = 3,
            Mana = 44,
            MaxMana = 100,
            Traits = new List<string> { "Huyết Nguyệt", "Nghệ Nhân Số 4", "Bậc Thầy Bảng Tính Excel" },
            BoomName = "Chưởng Lực: Phát Bắn Thầm Lặng Thứ 4 (Fourth Shot Whisper)",
            FinisherName = "Kết Liễu: Sân Khấu Tử Thần — Tuyệt Tác Huyết Nguyệt",
            PortalName = "Cổng Dịch Chuyển: Đền Thờ Huyết Nguyệt",
            ThemeColor = "#dc2626",
            SecondaryColor = "#7f1d1d",
            Quote = "Một... Hai... Ba... BỐN! Mọi bảng tính Excel đều phải đạt tới sự hoàn mỹ tuyệt đối của con số 4!",
            HomeDesk = new IsoCoord(50.5, 47.0, "Bàn 3 — Khu Excel (Blood Moon Jhin)"),
            CurrentPos = new IsoCoord(50.5, 47.0, "Bàn 3 — Khu Excel (Blood Moon Jhin)"),
            Capabilities = new List<string>
            {
                "Sửa lỗi công thức Excel (#REF!, #VALUE!, #N/A) và tối ưu hàm SUM, AVERAGE, XLOOKUP",
                "Thiết kế bảng tài chính 4 Quý, báo cáo doanh thu - chi phí - lợi nhuận chuẩn xác",
                "Làm sạch dữ liệu, loại bỏ trùng lặp và chuẩn hoá định dạng số liệu KPI",
                "Xuất file `.xlsx` OpenXML thực tế có sẵn công thức tính toán tự động"
            },
            Sprites = BuildSprites("jhin", "jhin.png")
        };
    }

    public override (SubTaskAssignment Assignment, GeneratedDocumentInfo Document) ExecuteTask(
        string instruction,
        DocumentEngine docEngine,
        string? attachedContent = null)
    {
        var sw = Stopwatch.StartNew();
        var stamp = DateTime.UtcNow.ToString("HHmmss");
        var fileName = $"BloodMoon_Jhin_Excel_4Q_{stamp}.xlsx";

        var highlights = new List<string>
        {
            $"Nhịp 1 — Làm sạch & chuẩn hoá dữ liệu bảng tính theo lệnh: \"{instruction}\"",
            "Nhịp 2 — Thiết lập công thức tự động `SUM(C5:C8)` và `AVERAGE(D5:D8)` không một sai số.",
            "Nhịp 3 — Phân bổ ngân sách & KPI theo 4 Quý chiến thuật với độ chính xác 97.4% - 100%.",
            "Nhịp 4 (Phát Bắn Cuối) — Hoàn thiện tuyệt tác `.xlsx` OpenXML dưới ánh trăng Huyết Nguyệt!"
        };

        var doc = docEngine.GenerateXlsxDocument(
            fileName,
            $"BẢNG TÍNH EXCEL (.XLSX): {instruction.ToUpperInvariant()}",
            instruction,
            highlights,
            attachedContent
        );
        sw.Stop();

        Profile.TasksCompleted++;
        Profile.Mana = 100;
        Profile.CurrentTaskTitle = $"Đã lập bảng {doc.FileName}";

        var assignment = new SubTaskAssignment
        {
            AgentId = Profile.Id,
            AgentName = Profile.Name,
            Specialty = "Sửa & Phân Tích Excel (.xlsx)",
            ActionType = "edit_xlsx",
            Instruction = instruction,
            TftSkillUsed = "Sân Khấu Tử Thần — 4 Nhịp Huyết Nguyệt (Curtain Call - Ctrl+4 & Finisher)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"Jhin Huyết Nguyệt Tí Nị đã xử lý bảng tính Excel 4 nhịp `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Góc Ctrl+1 — Nhịp 1] Nâng mặt nạ Huyết Nguyệt, rà soát toàn bộ hàng và cột dữ liệu.",
                "[Góc Ctrl+2 — Nhịp 2] Khắc công thức SUM và AVERAGE vào từng ô dữ liệu tài chính.",
                "[Góc Ctrl+3 — Nhịp 3] Cân đối 4 chỉ số KPI chiến lược đạt độ cân bằng nghệ thuật.",
                "[Finisher 18.3 — Nhịp 4!] Sân Khấu Tử Thần khai mở! Xuất file `.xlsx` hoàn mỹ tuyệt đối."
            },
            DurationMs = Math.Max(16, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}

/// <summary>
/// Video 4 in Playlist: Cửa Hàng Xoay Tua 18.2 - Jinx Lam Hoả Bộc Phá Tí Nị (Chibi Soul Fighter Jinx)
/// Role: Chuyên Gia Sửa PowerPoint & Thiết Kế Slide (.pptx)
/// </summary>
public class SoulFighterJinxPptxAgent : TftAgentBase
{
    public SoulFighterJinxPptxAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "jinx",
            Name = "Jinx Lam Hoả Bộc Phá Tí Nị",
            EnglishName = "Chibi Soul Fighter Jinx",
            Role = "Chuyên Gia Sửa PowerPoint & Thiết Kế Slide Bùng Nổ",
            Specialty = "pptx",
            FileExtension = ".pptx",
            PatchVersion = "ĐTCL 18.2 (Video #4)",
            VideoUrl = "https://www.youtube.com/watch?v=I1bCKGFfLsQ&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 4,
            Rarity = "Huyền Thoại Tinh Võ Sư (Soul Fighter Mythic)",
            StarLevel = 2,
            Mana = 90,
            MaxMana = 100,
            Traits = new List<string> { "Tinh Võ Sư", "Lam Hoả Bộc Phá", "Kiến Trúc Sư Slide PPTX" },
            BoomName = "Chưởng Lực: Tên Lửa Lam Hoả Siêu Khủng Khiếp!",
            FinisherName = "Kết Liễu: Sàn Đấu Tinh Võ — Bùng Nổ Hiệu Ứng Slide",
            PortalName = "Cổng Dịch Chuyển: Đấu Trường Lam Hoả",
            ThemeColor = "#38bdf8",
            SecondaryColor = "#f43f5e",
            Quote = "Slide nhàm chán ư? Để Jinx thổi bùng bằng Lam Hoả Bộc Phá! Cả phòng họp sẽ phải trầm trồ!",
            HomeDesk = new IsoCoord(47.5, 68.5, "Bàn 4 — Khu PowerPoint (Soul Fighter Jinx)"),
            CurrentPos = new IsoCoord(47.5, 68.5, "Bàn 4 — Khu PowerPoint (Soul Fighter Jinx)"),
            Capabilities = new List<string>
            {
                "Sửa bố cục, làm đẹp Slide PowerPoint (.pptx) và thiết kế Storyboard thuyết trình",
                "Chuyển hoá dữ liệu khô khan từ PDF/Word/Excel thành Infographic & điểm nhấn trực quan",
                "Phối màu Lam Hoả (Cyan/Neon Rose) và tạo cấu trúc thuyết phục ban giám đốc",
                "Xuất file `.pptx` OpenXML chuẩn 16:9 Widescreen sẵn sàng trình chiếu"
            },
            Sprites = BuildSprites("jinx", "jinx.png")
        };
    }

    public override (SubTaskAssignment Assignment, GeneratedDocumentInfo Document) ExecuteTask(
        string instruction,
        DocumentEngine docEngine,
        string? attachedContent = null)
    {
        var sw = Stopwatch.StartNew();
        var stamp = DateTime.UtcNow.ToString("HHmmss");
        var fileName = $"SoulFighter_Jinx_Slides_{stamp}.pptx";

        var highlights = new List<string>
        {
            $"Tái thiết kế bộ Slide thuyết trình 16:9 theo chủ đề: \"{instruction}\"",
            "Tạo Slide tiêu đề ấn tượng với hiệu ứng thị giác Lam Hoả Bộc Phá (Soul Fighter Theme).",
            "Cô đọng thông điệp cốt lõi thành các bullet hành động sắc bén cho buổi họp cùng Sếp Claude.",
            "Đóng gói chuẩn OpenXML `.pptx` tương thích hoàn toàn với PowerPoint & Google Slides."
        };

        var doc = docEngine.GeneratePptxDocument(
            fileName,
            $"SLIDE THUYẾT TRÌNH (.PPTX): {instruction.ToUpperInvariant()}",
            instruction,
            highlights,
            attachedContent
        );
        sw.Stop();

        Profile.TasksCompleted++;
        Profile.Mana = 100;
        Profile.CurrentTaskTitle = $"Đã thiết kế {doc.FileName}";

        var assignment = new SubTaskAssignment
        {
            AgentId = Profile.Id,
            AgentName = Profile.Name,
            Specialty = "Sửa & Thiết Kế PowerPoint (.pptx)",
            ActionType = "edit_pptx",
            Instruction = instruction,
            TftSkillUsed = "Tên Lửa Đạn Đạo Lam Hoả Siêu Khủng Khiếp! (Super Mega Death Rocket - Ctrl+3 & Finisher)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"Jinx Lam Hoả Bộc Phá Tí Nị đã thiết kế & nâng cấp bộ slide `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Góc Ctrl+1] Khởi động súng Xương Cá & Trapper Lam Hoả, phác thảo bố cục Slide 16:9.",
                "[Góc Ctrl+2] Phun sơn nghệ thuật Tinh Võ Sư, sắp xếp hệ thống phân cấp tiêu đề & biểu đồ.",
                "[Góc Ctrl+3] Nhồi hiệu ứng chuyển cảnh bùng nổ và cô đọng thông điệp thuyết trình.",
                "[Finisher 18.2] Tên Lửa Lam Hoả Bộc Phá! Xuất xưởng bộ Slide `.pptx` đỉnh cao!"
            },
            DurationMs = Math.Max(17, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}
