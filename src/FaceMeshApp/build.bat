@echo off
echo Building FaceMeshApp...
dotnet build FaceMeshApp.csproj

if %ERRORLEVEL% EQU 0 (
    echo Build successful!
    echo.
    echo ========================================
    echo FaceMeshApp Setup Instructions
    echo ========================================
    echo.
    echo The application can run in two modes:
    echo.
    echo 1. DEMO MODE (No setup required):
    echo    - Runs webcam without face detection
    echo    - Perfect for testing webcam functionality
    echo    - No model file needed
    echo.
    echo 2. FACE DETECTION MODE (Requires model):
    echo    - Download face_landmarker.task from:
    echo      https://developers.google.com/mediapipe/solutions/vision/face_landmarker
    echo    - File should be 10-20 MB in size
    echo    - Replace Models\face_landmarker.task with actual model file
    echo    - Restart application after replacing model
    echo.
    echo To run the application:
    echo   dotnet run --project FaceMeshApp.csproj
    echo.
    echo Note: If model file is missing or invalid, the app will
    echo automatically run in demo mode with clear instructions.
    echo ========================================
) else (
    echo Build failed with error code %ERRORLEVEL%
)
pause