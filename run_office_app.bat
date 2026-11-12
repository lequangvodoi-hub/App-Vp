@echo off
chcp 65001 >nul
echo ============================================================================
echo   KHOI DONG VAN PHONG LINH THU DTCL - SEP CLAUDE ^& 4 AI AGENT (C# .NET 8)
echo ============================================================================
echo Dang mo trinh duyet tai http://localhost:5050 ...
start "" "http://localhost:5050"
dotnet run --project TftOfficeApp.csproj --urls "http://localhost:5050"
pause
