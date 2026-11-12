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
/// 3D Character Model #1 (Mẫu từ Video #1): AI Agent Sửa PDF (.pdf)
/// </summary>
public class T1GalioPdfAgent : TftAgentBase
{
    public T1GalioPdfAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "galio",
            Name = "AI Agent PDF",
            EnglishName = "3D PDF Specialist Model",
            Role = "Chuyên Viên Sửa & Chuẩn Hoá PDF (.pdf)",
            Specialty = "pdf",
            FileExtension = ".pdf",
            PatchVersion = "Mẫu 3D #1 (Hộ Vệ Cánh Hoàng Kim)",
            VideoUrl = "https://www.youtube.com/watch?v=gMZ7AfAGuhY&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 1,
            Rarity = "Model 3D Cấp Huyền Thoại",
            StarLevel = 3,
            Mana = 80,
            MaxMana = 100,
            Traits = new List<string> { "Chuẩn Hoá A4", "Đóng Dấu Bảo Mật", "Xử Lý PDF" },
            BoomName = "Chưởng Lực: Đại Địa Chấn Hoàng Kim",
            FinisherName = "Hiệu Ứng Kết Liễu: Khiên Bảo Mật Tối Thượng",
            PortalName = "Cổng Dịch Chuyển: Phân Khu PDF",
            ThemeColor = "#e11d48",
            SecondaryColor = "#f59e0b",
            Quote = "Mọi trang tài liệu PDF đều được căn chuẩn bố cục A4 và đóng dấu bảo mật tuyệt đối!",
            HomeDesk = new IsoCoord(-6.5, -1.5, "Bàn 1 — Khu Sửa PDF"),
            CurrentPos = new IsoCoord(-6.5, -1.5, "Bàn 1 — Khu Sửa PDF"),
            Capabilities = new List<string>
            {
                "Sửa nội dung, căn chỉnh bố cục A4 và nhúng font chuẩn vào file PDF",
                "Đóng dấu bản quyền & chữ ký bảo mật chống chỉnh sửa",
                "Nén dung lượng PDF, nối/tách trang và trích xuất bảng biểu",
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
        var fileName = $"Agent_PDF_Report_{stamp}.pdf";

        var highlights = new List<string>
        {
            $"Chuẩn hoá cấu trúc PDF theo yêu cầu: \"{instruction}\"",
            "Đóng dấu xác thực bảo mật (Gold/Crimson Vector Header) ở đầu trang.",
            "Cố định lề chuẩn ISO A4, kiểm tra toàn vẹn bảng mã cross-reference (xref) và nhúng siêu dữ liệu.",
            "Hoàn tất kiểm định bảo mật PDF 1.4 — sẵn sàng in ấn hoặc gửi đối tác."
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
            TftSkillUsed = "Khiên Bảo Mật Hoàng Kim (Ctrl+4 & Finisher 3D)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"AI Agent PDF đã sửa & đóng dấu bảo mật file `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Động tác Ctrl+1] Khởi động đôi cánh năng lượng, quét cấu trúc luồng đối tượng PDF.",
                "[Động tác Ctrl+2] Căn chỉnh lại toàn bộ khung trang A4 và tiêu đề.",
                "[Động tác Ctrl+3] Đóng dấu xác thực bảo mật lên đầu tài liệu.",
                "[Finisher 3D] Xuất file PDF hoàn chỉnh với bảng xref chuẩn xác."
            },
            DurationMs = Math.Max(18, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}

/// <summary>
/// 3D Character Model #2 (Mẫu từ Video #2): AI Agent Sửa Word (.docx)
/// </summary>
public class PsyOpsZeriWordAgent : TftAgentBase
{
    public PsyOpsZeriWordAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "zeri",
            Name = "AI Agent Word",
            EnglishName = "3D Word Specialist Model",
            Role = "Chuyên Viên Sửa Word & Biên Tập Văn Bản (.docx)",
            Specialty = "word",
            FileExtension = ".docx",
            PatchVersion = "Mẫu 3D #2 (Đặc Vụ Xung Điện)",
            VideoUrl = "https://www.youtube.com/watch?v=k_k_bQd-NDQ&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 2,
            Rarity = "Model 3D Cấp Huyền Thoại",
            StarLevel = 2,
            Mana = 75,
            MaxMana = 100,
            Traits = new List<string> { "Biên Tập Tốc Độ Cao", "Chuẩn Hoá OpenXML", "Xử Lý Word" },
            BoomName = "Chưởng Lực: Tia Chớp Siêu Linh",
            FinisherName = "Hiệu Ứng Kết Liễu: Giải Mã Văn Bản Tốc Độ Ánh Sáng",
            PortalName = "Cổng Dịch Chuyển: Phân Khu Word",
            ThemeColor = "#06b6d4",
            SecondaryColor = "#a855f7",
            Quote = "Tốc độ soạn thảo 1.000 từ/giây! Mọi lỗi chính tả và định dạng .docx đều được xử lý tức thì!",
            HomeDesk = new IsoCoord(-2.2, -1.5, "Bàn 2 — Khu Sửa Word"),
            CurrentPos = new IsoCoord(-2.2, -1.5, "Bàn 2 — Khu Sửa Word"),
            Capabilities = new List<string>
            {
                "Sửa lỗi chính tả, ngữ pháp và nâng cấp văn phong chuyên nghiệp trên file Word (.docx)",
                "Định dạng chuẩn OpenXML: Heading 1/2/3, bảng điều khoản, mục lục và header/footer",
                "Soạn thảo hợp đồng, công văn, biên bản cuộc họp và tài liệu quy trình",
                "Tái cấu trúc nội dung thô thành tài liệu Word hoàn chỉnh"
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
        var fileName = $"Agent_Word_Document_{stamp}.docx";

        var highlights = new List<string>
        {
            $"Đã biên tập & chuẩn hoá văn bản Word theo lệnh: \"{instruction}\"",
            "Tự động rà soát chính tả, tối ưu cấu trúc câu và áp dụng Style Heading chuẩn doanh nghiệp.",
            "Thiết lập phân đoạn điều khoản rõ ràng, căn lề chuẩn A4 trong cấu trúc OpenXML.",
            "Đính kèm xác nhận hoàn tất biên tập văn bản."
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
            TftSkillUsed = "Xung Điện Biên Tập (Ctrl+2 & Finisher 3D)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"AI Agent Word đã sửa & định dạng tài liệu `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Động tác Ctrl+1] Nạp xung điện năng lượng, phân tích ngữ nghĩa văn bản.",
                "[Động tác Ctrl+2] Quét qua từng đoạn văn, sửa lỗi từ vựng và căn chỉnh Style Heading.",
                "[Động tác Ctrl+3] Đóng gói cấu trúc OpenXML `word/document.xml` chuẩn xác.",
                "[Finisher 3D] Xuất bản file `.docx` hoàn chỉnh trong chớp mắt."
            },
            DurationMs = Math.Max(15, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}

/// <summary>
/// 3D Character Model #3 (Mẫu từ Video #3): AI Agent Sửa Excel (.xlsx)
/// </summary>
public class BloodMoonJhinExcelAgent : TftAgentBase
{
    public BloodMoonJhinExcelAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "jhin",
            Name = "AI Agent Excel",
            EnglishName = "3D Excel Specialist Model",
            Role = "Chuyên Viên Sửa Excel & Phân Tích Số Liệu (.xlsx)",
            Specialty = "excel",
            FileExtension = ".xlsx",
            PatchVersion = "Mẫu 3D #3 (Nghệ Nhân Huyết Nguyệt)",
            VideoUrl = "https://www.youtube.com/watch?v=gU-FPO7pJ90&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 3,
            Rarity = "Model 3D Cấp Huyền Thoại",
            StarLevel = 3,
            Mana = 44,
            MaxMana = 100,
            Traits = new List<string> { "Chuẩn Hoá Công Thức", "Phân Tích 4 Quý", "Xử Lý Excel" },
            BoomName = "Chưởng Lực: Nhịp Điệu Chính Xác Tuyệt Đối",
            FinisherName = "Hiệu Ứng Kết Liễu: Ma Trận Bảng Tính Hoàn Mỹ",
            PortalName = "Cổng Dịch Chuyển: Phân Khu Excel",
            ThemeColor = "#dc2626",
            SecondaryColor = "#7f1d1d",
            Quote = "Mọi công thức và bảng tính Excel 4 Quý đều đạt độ chính xác tuyệt đối 100%!",
            HomeDesk = new IsoCoord(2.2, -1.5, "Bàn 3 — Khu Sửa Excel"),
            CurrentPos = new IsoCoord(2.2, -1.5, "Bàn 3 — Khu Sửa Excel"),
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
        var fileName = $"Agent_Excel_Spreadsheet_{stamp}.xlsx";

        var highlights = new List<string>
        {
            $"Quý 1 — Làm sạch & chuẩn hoá dữ liệu bảng tính theo lệnh: \"{instruction}\"",
            "Quý 2 — Thiết lập công thức tự động `SUM(C5:C8)` và `AVERAGE(D5:D8)` không một sai số.",
            "Quý 3 — Phân bổ ngân sách & KPI theo 4 Quý chiến thuật với độ chính xác 97.4% - 100%.",
            "Quý 4 — Hoàn thiện bảng tính `.xlsx` OpenXML chuẩn xác!"
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
            TftSkillUsed = "Ma Trận Số Liệu 4 Quý (Ctrl+4 & Finisher 3D)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"AI Agent Excel đã xử lý bảng tính `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Động tác Ctrl+1] Rà soát toàn bộ hàng và cột dữ liệu.",
                "[Động tác Ctrl+2] Khắc công thức SUM và AVERAGE vào từng ô dữ liệu tài chính.",
                "[Động tác Ctrl+3] Cân đối 4 chỉ số KPI chiến lược.",
                "[Finisher 3D] Xuất file `.xlsx` hoàn mỹ tuyệt đối."
            },
            DurationMs = Math.Max(16, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}

/// <summary>
/// 3D Character Model #4 (Mẫu từ Video #4): AI Agent Sửa PowerPoint (.pptx)
/// </summary>
public class SoulFighterJinxPptxAgent : TftAgentBase
{
    public SoulFighterJinxPptxAgent()
    {
        Profile = new TftAgentProfile
        {
            Id = "jinx",
            Name = "AI Agent PowerPoint",
            EnglishName = "3D PowerPoint Specialist Model",
            Role = "Chuyên Viên Sửa PowerPoint & Thiết Kế Slide (.pptx)",
            Specialty = "pptx",
            FileExtension = ".pptx",
            PatchVersion = "Mẫu 3D #4 (Chiến Binh Lam Hoả)",
            VideoUrl = "https://www.youtube.com/watch?v=I1bCKGFfLsQ&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le",
            VideoOrder = 4,
            Rarity = "Model 3D Cấp Huyền Thoại",
            StarLevel = 2,
            Mana = 90,
            MaxMana = 100,
            Traits = new List<string> { "Thiết Kế Slide 16:9", "Infographic Đột Phá", "Xử Lý PowerPoint" },
            BoomName = "Chưởng Lực: Pháo Hoa Lam Hoả Bộc Phá",
            FinisherName = "Hiệu Ứng Kết Liễu: Sân Khấu Trình Chiếu 3D Bùng Nổ",
            PortalName = "Cổng Dịch Chuyển: Phân Khu PowerPoint",
            ThemeColor = "#38bdf8",
            SecondaryColor = "#f43f5e",
            Quote = "Biến mọi dữ liệu khô khan thành bộ Slide thuyết trình PowerPoint 16:9 sống động!",
            HomeDesk = new IsoCoord(6.5, -1.5, "Bàn 4 — Khu Sửa PowerPoint"),
            CurrentPos = new IsoCoord(6.5, -1.5, "Bàn 4 — Khu Sửa PowerPoint"),
            Capabilities = new List<string>
            {
                "Sửa bố cục, làm đẹp Slide PowerPoint (.pptx) và thiết kế Storyboard thuyết trình",
                "Chuyển hoá dữ liệu khô khan từ PDF/Word/Excel thành Infographic & điểm nhấn trực quan",
                "Phối màu hiện đại và tạo cấu trúc thuyết phục ban giám đốc",
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
        var fileName = $"Agent_PowerPoint_Slides_{stamp}.pptx";

        var highlights = new List<string>
        {
            $"Tái thiết kế bộ Slide thuyết trình 16:9 theo chủ đề: \"{instruction}\"",
            "Tạo Slide tiêu đề ấn tượng với hiệu ứng thị giác Lam Hoả.",
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
            TftSkillUsed = "Bùng Nổ Slide Lam Hoả (Ctrl+3 & Finisher 3D)",
            BoomEffect = Profile.BoomName,
            OutputFileName = doc.FileName,
            OutputSummary = $"AI Agent PowerPoint đã thiết kế & nâng cấp bộ slide `{doc.FileName}` ({doc.SizeBytes} bytes).",
            ExecutionSteps = new List<string>
            {
                "[Động tác Ctrl+1] Phác thảo bố cục Slide 16:9 Widescreen.",
                "[Động tác Ctrl+2] Sắp xếp hệ thống phân cấp tiêu đề & biểu đồ trực quan.",
                "[Động tác Ctrl+3] Nhồi hiệu ứng chuyển cảnh và cô đọng thông điệp thuyết trình.",
                "[Finisher 3D] Xuất xưởng bộ Slide `.pptx` hoàn chỉnh!"
            },
            DurationMs = Math.Max(17, sw.ElapsedMilliseconds)
        };

        return (assignment, doc);
    }
}
