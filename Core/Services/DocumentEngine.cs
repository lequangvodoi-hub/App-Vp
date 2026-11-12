using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using TftOfficeApp.Core.Models;

namespace TftOfficeApp.Core.Services;

public class DocumentEngine
{
    private readonly string _workspaceDir;
    private readonly List<GeneratedDocumentInfo> _documents = new();
    private readonly object _lock = new();

    public DocumentEngine(IWebHostEnvironment env)
    {
        _workspaceDir = Path.Combine(env.WebRootPath, "workspace");
        Directory.CreateDirectory(_workspaceDir);
        SeedInitialSampleFiles();
    }

    public List<GeneratedDocumentInfo> GetDocuments()
    {
        lock (_lock)
        {
            return _documents.OrderByDescending(d => d.CreatedAt).ToList();
        }
    }

    private void SeedInitialSampleFiles()
    {
        if (_documents.Count > 0) return;

        GeneratePdfDocument(
            "Bao_Cao_Chien_Thuat_T1_Galio_Q4.pdf",
            "BÁO CÁO KIỂM ĐỊNH & CHUẨN HOÁ PDF - T1 GALIO TÍ NỊ",
            "Khởi tạo hệ thống văn phòng ĐTCL 18.5: Chuẩn hoá định dạng PDF, đóng dấu bảo mật Khiên Durand và tối ưu hoá cấu trúc trang.",
            new List<string>
            {
                "Đã đóng dấu xác thực T1 World Champions (Faker / Galio Signature) lên tiêu đề PDF.",
                "Chuẩn hoá bố cục A4, lề chuẩn doanh nghiệp và mã hoá metadata chống chỉnh sửa trái phép.",
                "Tối ưu dung lượng vector & nhúng bảng tổng hợp chỉ số chiến thuật văn phòng."
            }
        );

        GenerateDocxDocument(
            "Hop_Dong_Sieu_Linh_PsyOps_Zeri.docx",
            "BIÊN BẢN & HỢP ĐỒNG CHIẾN LƯỢC - ZERI ĐẶC VỤ SIÊU LINH",
            "Soạn thảo và rà soát văn bản Word (.docx) với tốc độ tia chớp Điện Đạt Đỉnh Điểm.",
            new List<string>
            {
                "Tự động sửa lỗi chính tả, chuẩn hoá văn phong hành chính - thương mại.",
                "Định dạng Heading 1/2, bảng điều khoản trách nhiệm và mục lục thông minh.",
                "Tích hợp chữ ký số Đặc Vụ Siêu Linh (PsyOps Classified Clearance)."
            }
        );

        GenerateXlsxDocument(
            "Bang_Tai_Chinh_Huyet_Nguyet_Jhin_4Q.xlsx",
            "BẢNG PHÂN TÍCH TÀI CHÍNH & KPI 4 NHỊP - JHIN HUYẾT NGUYỆT",
            "Xử lý bảng tính Excel (.xlsx) theo chuẩn nghệ thuật Số 4 hoàn hảo của Jhin Huyết Nguyệt.",
            new List<string>
            {
                "Thiết lập 4 phân khu dữ liệu: Doanh Thu, Chi Phí, Lợi Nhuận Ròng, Hiệu Suất Agent.",
                "Tự động gắn công thức tính tổng (SUM), trung bình (AVERAGE) và biên độ tăng trưởng 4 quý.",
                "Loại bỏ dữ liệu trùng lặp và chuẩn hoá định dạng tiền tệ VNĐ / USD."
            }
        );

        GeneratePptxDocument(
            "Slide_Thuyet_Trinh_Lam_Hoa_Jinx.pptx",
            "BẢN THUYẾT TRÌNH ĐỘT PHÁ DOANH SỐ - JINX LAM HOẢ BỘC PHÁ",
            "Thiết kế bộ Slide PowerPoint (.pptx) bùng nổ thị giác với hiệu ứng Tên Lửa Đạn Đạo Siêu Khủng Khiếp.",
            new List<string>
            {
                "Cấu trúc 4 Slide chiến lược: Tổng Quan Dự Án, Phân Tích Dữ Liệu, Giải Pháp Đột Phá, Lộ Trình Thực Thi.",
                "Phối màu Lam Hoả (Soul Fighter Cyan & Magenta) cùng bố cục Infographic hiện đại.",
                "Tự động trích xuất điểm nhấn từ PDF, Word, Excel thành bullet thuyết trình súc tích."
            }
        );
    }

    public GeneratedDocumentInfo GeneratePdfDocument(
        string fileName,
        string title,
        string userInstruction,
        List<string> highlights,
        string? existingContent = null)
    {
        fileName = EnsureExtension(SanitizeFileName(fileName), ".pdf");
        var fullPath = Path.Combine(_workspaceDir, fileName);

        var asciiTitle = RemoveDiacritics(title);
        var asciiInstruction = RemoveDiacritics(userInstruction);
        var asciiHighlights = highlights.Select(RemoveDiacritics).ToList();
        var asciiExtra = string.IsNullOrWhiteSpace(existingContent)
            ? ""
            : RemoveDiacritics(existingContent.Length > 350 ? existingContent[..350] + "..." : existingContent);

        var pdfBytes = BuildValidPdfBinary(asciiTitle, asciiInstruction, asciiHighlights, asciiExtra);
        File.WriteAllBytes(fullPath, pdfBytes);

        var previewSb = new StringBuilder();
        previewSb.AppendLine($"[PDF 1.4 DOCUMENT — T1 GALIO TÍ NỊ SHIELD ENGINE]");
        previewSb.AppendLine($"Tiêu đề: {title}");
        previewSb.AppendLine($"Yêu cầu từ Sếp Claude: {userInstruction}");
        previewSb.AppendLine("Các hạng mục đã xử lý & đóng dấu PDF:");
        foreach (var h in highlights)
        {
            previewSb.AppendLine($"  • {h}");
        }
        if (!string.IsNullOrWhiteSpace(existingContent))
        {
            previewSb.AppendLine($"Nội dung gốc đã tích hợp: {existingContent[..Math.Min(200, existingContent.Length)]}");
        }

        return RegisterDoc(fileName, "pdf", "galio", "T1 Galio Tí Nị", pdfBytes.Length, previewSb.ToString(), highlights);
    }

    public GeneratedDocumentInfo GenerateDocxDocument(
        string fileName,
        string title,
        string userInstruction,
        List<string> highlights,
        string? existingContent = null)
    {
        fileName = EnsureExtension(SanitizeFileName(fileName), ".docx");
        var fullPath = Path.Combine(_workspaceDir, fileName);

        using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            WriteZipEntry(archive, "[Content_Types].xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>
</Types>");

            WriteZipEntry(archive, "_rels/.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>
</Relationships>");

            var paragraphsXml = new StringBuilder();
            paragraphsXml.Append(BuildWordParagraph(title, true, "28", "0EA5E9"));
            paragraphsXml.Append(BuildWordParagraph($"Phụ trách biên tập: Zeri Đặc Vụ Siêu Linh Tí Nị (TFT 18.4) | Thời gian: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", false, "20", "64748B"));
            paragraphsXml.Append(BuildWordParagraph($"Chỉ thị từ Sếp Claude: {userInstruction}", true, "22", "1E293B"));
            paragraphsXml.Append(BuildWordParagraph("CHI TIẾT NỘI DUNG VĂN BẢN ĐÃ CHỈNH SỬA & CHUẨN HOÁ:", true, "24", "0284C7"));

            int idx = 1;
            foreach (var item in highlights)
            {
                paragraphsXml.Append(BuildWordParagraph($"{idx}. {item}", false, "22", "0F172A"));
                idx++;
            }

            if (!string.IsNullOrWhiteSpace(existingContent))
            {
                paragraphsXml.Append(BuildWordParagraph("PHỤ LỤC DỮ LIỆU ĐẦU VÀO ĐÃ XỬ LÝ:", true, "22", "0284C7"));
                paragraphsXml.Append(BuildWordParagraph(existingContent, false, "20", "334155"));
            }

            paragraphsXml.Append(BuildWordParagraph("Xác nhận hoàn tất bởi: Zeri Đặc Vụ Siêu Linh Tí Nị — Chưởng Lực Tia Chớp Siêu Linh.", true, "20", "10B981"));

            WriteZipEntry(archive, "word/document.xml", $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    {paragraphsXml}
    <w:sectPr>
      <w:pgSz w:w=""11906"" w:h=""16838""/>
      <w:pgMar w:top=""1440"" w:right=""1440"" w:bottom=""1440"" w:left=""1440""/>
    </w:sectPr>
  </w:body>
</w:document>");
        }

        var fileInfo = new FileInfo(fullPath);
        var previewSb = new StringBuilder();
        previewSb.AppendLine($"[WORD OPENXML (.DOCX) — ZERI ĐẶC VỤ SIÊU LINH ENGINE]");
        previewSb.AppendLine($"Tiêu đề văn bản: {title}");
        previewSb.AppendLine($"Yêu cầu từ Sếp Claude: {userInstruction}");
        previewSb.AppendLine("Nội dung các đoạn văn & điều khoản đã biên tập:");
        foreach (var h in highlights)
        {
            previewSb.AppendLine($"  ⚡ {h}");
        }

        return RegisterDoc(fileName, "docx", "zeri", "Zeri Đặc Vụ Siêu Linh Tí Nị", fileInfo.Length, previewSb.ToString(), highlights);
    }

    public GeneratedDocumentInfo GenerateXlsxDocument(
        string fileName,
        string title,
        string userInstruction,
        List<string> highlights,
        string? existingContent = null)
    {
        fileName = EnsureExtension(SanitizeFileName(fileName), ".xlsx");
        var fullPath = Path.Combine(_workspaceDir, fileName);

        using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            WriteZipEntry(archive, "[Content_Types].xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
</Types>");

            WriteZipEntry(archive, "_rels/.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");

            WriteZipEntry(archive, "xl/_rels/workbook.xml.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
</Relationships>");

            WriteZipEntry(archive, "xl/workbook.xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <sheets>
    <sheet name=""Jhin_BloodMoon_4Q"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>");

            var sheetXml = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <sheetData>
    <row r=""1"">
      <c r=""A1"" t=""inlineStr""><is><t>{EscapeXml(title)}</t></is></c>
      <c r=""B1"" t=""inlineStr""><is><t>Jhin Huyết Nguyệt Tí Nị (TFT 18.3)</t></is></c>
    </row>
    <row r=""2"">
      <c r=""A2"" t=""inlineStr""><is><t>Chỉ thị Sếp Claude</t></is></c>
      <c r=""B2"" t=""inlineStr""><is><t>{EscapeXml(userInstruction)}</t></is></c>
    </row>
    <row r=""4"">
      <c r=""A4"" t=""inlineStr""><is><t>Nhịp Chiến Thuật (4 Quý)</t></is></c>
      <c r=""B4"" t=""inlineStr""><is><t>Hạng Mục Phân Tích</t></is></c>
      <c r=""C4"" t=""inlineStr""><is><t>Ngân Sách (Triệu VNĐ)</t></is></c>
      <c r=""D4"" t=""inlineStr""><is><t>Hiệu Suất Đạt Được (%)</t></is></c>
    </row>
    <row r=""5"">
      <c r=""A5"" t=""inlineStr""><is><t>Nhịp 1 (Quý I)</t></is></c>
      <c r=""B5"" t=""inlineStr""><is><t>{EscapeXml(highlights.ElementAtOrDefault(0) ?? "Chuẩn hoá dữ liệu đầu vào")}</t></is></c>
      <c r=""C5""><v>440</v></c>
      <c r=""D5""><v>94.4</v></c>
    </row>
    <row r=""6"">
      <c r=""A6"" t=""inlineStr""><is><t>Nhịp 2 (Quý II)</t></is></c>
      <c r=""B6"" t=""inlineStr""><is><t>{EscapeXml(highlights.ElementAtOrDefault(1) ?? "Tối ưu công thức tài chính")}</t></is></c>
      <c r=""C6""><v>880</v></c>
      <c r=""D6""><v>96.8</v></c>
    </row>
    <row r=""7"">
      <c r=""A7"" t=""inlineStr""><is><t>Nhịp 3 (Quý III)</t></is></c>
      <c r=""B7"" t=""inlineStr""><is><t>{EscapeXml(highlights.ElementAtOrDefault(2) ?? "Kiểm toán &amp; đối soát KPI")}</t></is></c>
      <c r=""C7""><v>1320</v></c>
      <c r=""D7""><v>98.4</v></c>
    </row>
    <row r=""8"">
      <c r=""A8"" t=""inlineStr""><is><t>Nhịp 4 (Phát Bắn Cuối - Quý IV)</t></is></c>
      <c r=""B8"" t=""inlineStr""><is><t>Đỉnh Cao Nghệ Thuật Số 4 (Hoàn Tất)</t></is></c>
      <c r=""C8""><v>1760</v></c>
      <c r=""D8""><v>100.0</v></c>
    </row>
    <row r=""9"">
      <c r=""A9"" t=""inlineStr""><is><t>TỔNG CỘNG / TRUNG BÌNH</t></is></c>
      <c r=""B9"" t=""inlineStr""><is><t>Sân Khấu Tử Thần — Đã Kiểm Chứng</t></is></c>
      <c r=""C9""><f>SUM(C5:C8)</f><v>4400</v></c>
      <c r=""D9""><f>AVERAGE(D5:D8)</f><v>97.4</v></c>
    </row>
  </sheetData>
</worksheet>";
            WriteZipEntry(archive, "xl/worksheets/sheet1.xml", sheetXml);
        }

        var fileInfo = new FileInfo(fullPath);
        var previewSb = new StringBuilder();
        previewSb.AppendLine($"[EXCEL OPENXML (.XLSX) — JHIN HUYẾT NGUYỆT 4-BEAT ENGINE]");
        previewSb.AppendLine($"Bảng tính: {title}");
        previewSb.AppendLine($"Chỉ thị từ Sếp Claude: {userInstruction}");
        previewSb.AppendLine("┌──────────────────────┬────────────────────────────────────────────┬──────────────┬───────────┐");
        previewSb.AppendLine("│ Nhịp (4 Quý)         │ Hạng Mục Xử Lý                             │ Ngân Sách    │ Hiệu Suất │");
        previewSb.AppendLine("├──────────────────────┼────────────────────────────────────────────┼──────────────┼───────────┤");
        previewSb.AppendLine("│ Nhịp 1 (Quý I)       │ Chuẩn hoá dữ liệu & lọc trùng lặp          │   440 Tr VNĐ │    94.4%  │");
        previewSb.AppendLine("│ Nhịp 2 (Quý II)      │ Tự động hoá công thức SUM/AVERAGE/XLOOKUP  │   880 Tr VNĐ │    96.8%  │");
        previewSb.AppendLine("│ Nhịp 3 (Quý III)     │ Đối soát KPI & biểu đồ xu hướng            │ 1,320 Tr VNĐ │    98.4%  │");
        previewSb.AppendLine("│ Nhịp 4 (Phát Cuối!)  │ Đỉnh Cao Nghệ Thuật Số 4                   │ 1,760 Tr VNĐ │   100.0%  │");
        previewSb.AppendLine("├──────────────────────┼────────────────────────────────────────────┼──────────────┼───────────┤");
        previewSb.AppendLine("│ TỔNG (=SUM/AVERAGE)  │ Sân Khấu Tử Thần Verified                  │ 4,400 Tr VNĐ │    97.4%  │");
        previewSb.AppendLine("└──────────────────────┴────────────────────────────────────────────┴──────────────┴───────────┘");

        return RegisterDoc(fileName, "xlsx", "jhin", "Jhin Huyết Nguyệt Tí Nị", fileInfo.Length, previewSb.ToString(), highlights);
    }

    public GeneratedDocumentInfo GeneratePptxDocument(
        string fileName,
        string title,
        string userInstruction,
        List<string> highlights,
        string? existingContent = null)
    {
        fileName = EnsureExtension(SanitizeFileName(fileName), ".pptx");
        var fullPath = Path.Combine(_workspaceDir, fileName);

        using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            WriteZipEntry(archive, "[Content_Types].xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/ppt/presentation.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml""/>
  <Override PartName=""/ppt/slides/slide1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/ppt/slides/slide2.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
</Types>");

            WriteZipEntry(archive, "_rels/.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""ppt/presentation.xml""/>
</Relationships>");

            WriteZipEntry(archive, "ppt/_rels/presentation.xml.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide1.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"" Target=""slides/slide2.xml""/>
</Relationships>");

            WriteZipEntry(archive, "ppt/presentation.xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:presentation xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:sldIdLst>
    <p:sldId id=""256"" r:id=""rId1""/>
    <p:sldId id=""257"" r:id=""rId2""/>
  </p:sldIdLst>
  <p:sldSz cx=""12192000"" cy=""6858000""/>
  <p:notesSz cx=""6858000"" cy=""9144000""/>
</p:presentation>");

            WriteZipEntry(archive, "ppt/slides/slide1.xml", BuildSlideXml(
                title,
                $"Thiết kế bởi: Jinx Lam Hoả Bộc Phá Tí Nị (TFT 18.2) | Chỉ thị: {userInstruction}"
            ));

            WriteZipEntry(archive, "ppt/slides/slide2.xml", BuildSlideXml(
                "ĐIỂM NHẤN CHIẾN LƯỢC & HIỆU ỨNG BỘC PHÁ",
                string.Join(" | ", highlights)
            ));
        }

        var fileInfo = new FileInfo(fullPath);
        var previewSb = new StringBuilder();
        previewSb.AppendLine($"[POWERPOINT OPENXML (.PPTX) — JINX LAM HOẢ BỘC PHÁ ENGINE]");
        previewSb.AppendLine($"Bộ Slide: {title}");
        previewSb.AppendLine($"Chỉ thị từ Sếp Claude: {userInstruction}");
        previewSb.AppendLine("Cấu trúc các Slide đã thiết kế:");
        previewSb.AppendLine("  [Slide 01] Trang Bìa Chiến Lược Lam Hoả — Tóm tắt mục tiêu điều hành từ Sếp Claude");
        for (int i = 0; i < highlights.Count; i++)
        {
            previewSb.AppendLine($"  [Slide 0{i + 2}] {highlights[i]}");
        }

        return RegisterDoc(fileName, "pptx", "jinx", "Jinx Lam Hoả Bộc Phá Tí Nị", fileInfo.Length, previewSb.ToString(), highlights);
    }

    private GeneratedDocumentInfo RegisterDoc(
        string fileName,
        string fileType,
        string agentId,
        string agentName,
        long sizeBytes,
        string previewText,
        List<string> highlights)
    {
        var doc = new GeneratedDocumentInfo
        {
            FileName = fileName,
            FileType = fileType,
            CreatedByAgentId = agentId,
            CreatedByAgentName = agentName,
            SizeBytes = sizeBytes,
            CreatedAt = DateTime.UtcNow,
            DownloadUrl = $"/workspace/{fileName}",
            PreviewText = previewText,
            Highlights = highlights
        };

        lock (_lock)
        {
            _documents.RemoveAll(d => d.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            _documents.Insert(0, doc);
        }

        return doc;
    }

    private static string BuildWordParagraph(string text, bool bold, string fontSizeHalfPt, string hexColor)
    {
        var boldTag = bold ? "<w:b/>" : "";
        return $@"<w:p>
      <w:r>
        <w:rPr>{boldTag}<w:color w:val=""{hexColor}""/><w:sz w:val=""{fontSizeHalfPt}""/></w:rPr>
        <w:t xml:space=""preserve"">{EscapeXml(text)}</w:t>
      </w:r>
    </w:p>";
    }

    private static string BuildSlideXml(string heading, string body)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sld xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:spTree>
      <p:nvGrpSpPr><p:cNvPr id=""1"" name=""""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
      <p:grpSpPr/>
      <p:sp>
        <p:nvSpPr><p:cNvPr id=""2"" name=""Title""/><p:cNvSpPr/><p:nvPr/></p:nvSpPr>
        <p:spPr/>
        <p:txBody>
          <a:bodyPr/>
          <a:lstStyle/>
          <a:p><a:r><a:t>{EscapeXml(heading)}</a:t></a:r></a:p>
          <a:p><a:r><a:t>{EscapeXml(body)}</a:t></a:r></a:p>
        </p:txBody>
      </p:sp>
    </p:spTree>
  </p:cSld>
</p:sld>";
    }

    private static byte[] BuildValidPdfBinary(
        string title,
        string instruction,
        List<string> highlights,
        string extra)
    {
        var streamContent = new StringBuilder();
        // Header banner in T1 Crimson/Gold
        streamContent.AppendLine("0.08 0.11 0.18 rg");
        streamContent.AppendLine("36 740 523 64 re f");
        streamContent.AppendLine("0.85 0.68 0.32 RG 2 w");
        streamContent.AppendLine("36 740 523 64 re S");

        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F1 14 Tf 0.96 0.82 0.45 rg");
        streamContent.AppendLine($"50 778 Td ({EscapePdfString("T1 GALIO TI NI - PDF SHIELD ENGINE (TFT 18.5)")}) Tj");
        streamContent.AppendLine("/F1 10 Tf 0.90 0.94 0.98 rg");
        streamContent.AppendLine($"0 -20 Td ({EscapePdfString(Truncate(title, 75))}) Tj");
        streamContent.AppendLine("ET");

        streamContent.AppendLine("BT");
        streamContent.AppendLine("/F1 11 Tf 0.10 0.14 0.22 rg");
        streamContent.AppendLine($"42 705 Td ({EscapePdfString("Directive from Boss Claude: " + Truncate(instruction, 80))}) Tj");

        int yOffset = -28;
        streamContent.AppendLine("/F1 11 Tf 0.72 0.12 0.18 rg");
        streamContent.AppendLine($"0 {yOffset} Td ({EscapePdfString("Executed PDF Modifications & Verification Checkpoints:")}) Tj");

        streamContent.AppendLine("/F1 10 Tf 0.15 0.20 0.28 rg");
        int idx = 1;
        foreach (var h in highlights)
        {
            streamContent.AppendLine($"0 -22 Td ({EscapePdfString($"{idx}. {Truncate(h, 88)}")}) Tj");
            idx++;
        }

        if (!string.IsNullOrWhiteSpace(extra))
        {
            streamContent.AppendLine($"0 -28 Td ({EscapePdfString("Source Content Extract: " + Truncate(extra, 85))}) Tj");
        }

        streamContent.AppendLine("0.08 0.52 0.35 rg");
        streamContent.AppendLine($"0 -36 Td ({EscapePdfString("SEALED BY CHIBI T1 GALIO - HERO'S ENTRANCE VERIFIED")}) Tj");
        streamContent.AppendLine("ET");

        var contentBytes = Encoding.ASCII.GetBytes(streamContent.ToString());

        using var ms = new MemoryStream();
        void WriteAscii(string s) => ms.Write(Encoding.ASCII.GetBytes(s));

        WriteAscii("%PDF-1.4\n");
        var offsets = new List<long> { 0 };

        // Object 1: Catalog
        offsets.Add(ms.Position);
        WriteAscii("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        // Object 2: Pages
        offsets.Add(ms.Position);
        WriteAscii("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        // Object 3: Page
        offsets.Add(ms.Position);
        WriteAscii("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");

        // Object 4: Font
        offsets.Add(ms.Position);
        WriteAscii("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n");

        // Object 5: Content stream
        offsets.Add(ms.Position);
        WriteAscii($"5 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        ms.Write(contentBytes);
        WriteAscii("\nendstream\nendobj\n");

        long xrefPos = ms.Position;
        WriteAscii($"xref\n0 {offsets.Count}\n");
        WriteAscii("0000000000 65535 f \n");
        for (int i = 1; i < offsets.Count; i++)
        {
            WriteAscii($"{offsets[i]:D10} 00000 n \n");
        }

        WriteAscii($"trailer\n<< /Size {offsets.Count} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF\n");
        return ms.ToArray();
    }

    private static void WriteZipEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string EscapeXml(string s) =>
        s.Replace("&", "&amp;")
         .Replace("<", "&lt;")
         .Replace(">", "&gt;")
         .Replace("\"", "&quot;")
         .Replace("'", "&apos;");

    private static string EscapePdfString(string s) =>
        s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 3)] + "...";

    private static string SanitizeFileName(string name)
    {
        var cleaned = Regex.Replace(name.Trim(), @"[^a-zA-Z0-9_\-\.]", "_");
        return string.IsNullOrWhiteSpace(cleaned) ? $"TftDoc_{DateTime.UtcNow:HHmmss}" : cleaned;
    }

    private static string EnsureExtension(string fileName, string ext) =>
        fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ? fileName : fileName + ext;

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        string normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                char mapped = c switch
                {
                    'đ' => 'd',
                    'Đ' => 'D',
                    '•' => '-',
                    '—' => '-',
                    _ => c <= 126 && c >= 32 ? c : ' '
                };
                sb.Append(mapped);
            }
        }
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }
}
