@echo off
echo Building FragifyTracker...
dotnet restore
dotnet build

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build successful! Running application...
    echo.
    dotnet run
) else (
    echo.
    echo Build failed! Please check the errors above.
    pause
)
