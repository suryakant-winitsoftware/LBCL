@echo off  
powershell -Command "(Get-Content 'Winit.Modules.Promotion.BL.csproj') -replace '8\.0\.0', '8.0.1' | Set-Content 'Winit.Modules.Promotion.BL.csproj'"  
