# ⚔️ Văn Phòng Linh Thú ĐTCL — Sếp Claude & 4 AI Agent (C# .NET 8 Core + CLI + Web 3D Isometric)

Ứng dụng có lõi **C# (.NET 8 ASP.NET Core & CLI)** kết hợp giao diện **Mô phỏng Văn phòng 3D Isometric**, biến các AI Agent xử lý tài liệu thành **4 Linh Thú Tí Nị (Chibi Champions)** trích xuất trực tiếp từ **4 video đầu tiên** trong danh sách phát *Hiệu ứng của các Linh Thú tí nị - Đấu Trường Chân Lý*:

## 🐉 Bảng Phân Vai 4 Linh Thú AI Agent & Sếp Tổng Claude

| Thứ tự Video | Nhân vật Linh Thú ĐTCL | Nguồn Video ĐTCL | Chức năng AI Agent | Kỹ năng & Hiệu ứng (Ctrl+1..4 / Boom / Finisher) |
| :--- | :--- | :--- | :--- | :--- |
| **Sếp Tổng** | **👑 Sếp Claude** (*Regional Manager*) | Orchestrator | Phân tích lệnh tự nhiên & bắn Chưởng Lực giao việc | Triệu tập họp chiến thuật tại Phòng Họp & điều phối đa tác vụ |
| **Video #1** | **🛡️ T1 Galio Tí Nị** (*Chibi T1 Galio*) | [ĐTCL 18.5 (`gMZ7AfAGuhY`)](https://www.youtube.com/watch?v=gMZ7AfAGuhY&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le) | **Chuyên Gia Sửa PDF (`.pdf`)** | Khiên Durand chuẩn hoá A4, đóng dấu bảo mật T1 Worlds, Finisher *Siêu Hùng Giáng Thế* |
| **Video #2** | **⚡ Zeri Đặc Vụ Siêu Linh Tí Nị** (*Chibi PsyOps Zeri*) | [ĐTCL 18.4 (`k_k_bQd-NDQ`)](https://www.youtube.com/watch?v=k_k_bQd-NDQ&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le) | **Chuyên Gia Sửa Word (`.docx`)** | Biên tập văn bản/hợp đồng tốc độ tia chớp, chuẩn hoá OpenXML Heading, Finisher *Điện Đạt Đỉnh Điểm* |
| **Video #3** | **🎭 Jhin Huyết Nguyệt Tí Nị** (*Chibi Blood Moon Jhin*) | [ĐTCL 18.3 (`gU-FPO7pJ90`)](https://www.youtube.com/watch?v=gU-FPO7pJ90&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le) | **Chuyên Gia Sửa Excel (`.xlsx`)** | Nghệ thuật Số 4 hoàn hảo: lập bảng tài chính 4 Quý, gắn công thức `SUM`/`AVERAGE`, Finisher *Sân Khấu Tử Thần* |
| **Video #4** | **🔥 Jinx Lam Hoả Bộc Phá Tí Nị** (*Chibi Soul Fighter Jinx*) | [ĐTCL 18.2 (`I1bCKGFfLsQ`)](https://www.youtube.com/watch?v=I1bCKGFfLsQ&list=PLNLR0XKc_IdScVOGha035XOyQ6SZk50le) | **Chuyên Gia Sửa PowerPoint (`.pptx`)** | Thiết kế Slide 16:9 Lam Hoả Bộc Phá, cô đọng Infographic, Finisher *Tên Lửa Đạn Đạo Siêu Khủng Khiếp* |

---

## 🏗️ Kiến Trúc Mã Nguồn C#

- [`Program.cs`](file:///home/antigravity/lequangvodoi-hub/App-Vp/Program.cs): Điểm khởi chạy tích hợp cả **Web Server REST API** (`/api/state`, `/api/command`, `/api/agent/{id}/action`, `/api/files`) và **C# CLI Mode** (`--cli`, `--command`).
- [`Core/Models/TftModels.cs`](file:///home/antigravity/lequangvodoi-hub/App-Vp/Core/Models/TftModels.cs): Hệ thống kiểu dữ liệu cho Linh Thú, toạ độ văn phòng 3D (`IsoCoord`), bước di chuyển (`SimulationTimelineStep`) và biên bản điều phối của Sếp Claude.
- [`Core/Agents/ClaudeBossOrchestrator.cs`](file:///home/antigravity/lequangvodoi-hub/App-Vp/Core/Agents/ClaudeBossOrchestrator.cs): Bộ não của **Sếp Claude** — phân tích yêu cầu tiếng Việt/tiếng Anh (hỗ trợ tích hợp `ANTHROPIC_API_KEY` tuỳ chọn), chọn đúng Linh Thú và sinh kịch bản di chuyển trên mặt bằng văn phòng 3D.
- [`Core/Agents/TftSpecialistAgents.cs`](file:///home/antigravity/lequangvodoi-hub/App-Vp/Core/Agents/TftSpecialistAgents.cs): 4 lớp AI Agent (`T1GalioPdfAgent`, `PsyOpsZeriWordAgent`, `BloodMoonJhinExcelAgent`, `SoulFighterJinxPptxAgent`).
- [`Core/Services/DocumentEngine.cs`](file:///home/antigravity/lequangvodoi-hub/App-Vp/Core/Services/DocumentEngine.cs): Engine C# thuần tạo và chỉnh sửa file thực tế chuẩn nhị phân `.pdf` (PDF 1.4) và OpenXML `.docx`, `.xlsx`, `.pptx` lưu tại `wwwroot/workspace/`.
- [`Core/Cli/TftCliRunner.cs`](file:///home/antigravity/lequangvodoi-hub/App-Vp/Core/Cli/TftCliRunner.cs): Giao diện dòng lệnh tương tác trực tiếp trên Terminal.

---

## 🚀 Cách Chạy Ứng Dụng

### 1. Chế độ Giao diện Web Mô phỏng Văn phòng 3D
```bash
dotnet run --project TftOfficeApp.csproj --urls "http://0.0.0.0:5050"
```

### 2. Chế độ C# CLI (Giao lệnh trực tiếp trên Terminal)
```bash
# Chạy 1 lệnh trực tiếp cho Sếp Claude:
dotnet run --project TftOfficeApp.csproj -- --command "Sếp Claude hãy phân tích dự án Quý 4: giao Galio sửa PDF, Zeri sửa Word, Jhin sửa Excel, Jinx sửa PowerPoint"

# Hoặc mở chế độ CLI tương tác liên tục:
dotnet run --project TftOfficeApp.csproj -- --cli
```
