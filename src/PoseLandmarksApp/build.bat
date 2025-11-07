@echo off
echo Building Pose Landmarks Detection App...
echo.

dotnet build --configuration Release

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build completed successfully!
    echo.
    echo Run the application with:
    echo   dotnet run --configuration Release
    echo.
    echo Or run directly from:
    echo   bin\Release\net9.0-windows\PoseLandmarksApp.exe
) else (
    echo.
    echo Build failed with error code %ERRORLEVEL%
    echo.
    echo Please check the error messages above for details.
)

echo.
pause
