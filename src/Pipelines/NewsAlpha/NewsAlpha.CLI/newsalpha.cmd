@echo off
@setlocal
set ERROR_CODE=0

dotnet ".\bin\Debug\netcoreapp2.1\NewsAlpha.CLI.dll" %*
goto end

:end
exit /B %ERROR_CODE%