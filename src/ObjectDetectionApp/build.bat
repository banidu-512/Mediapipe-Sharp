@echo off
echo Building ObjectDetectionApp...
echo.

REM Check if dotnet is available
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET SDK not found. Please install .NET 9.0 SDK or later.
    pause
    exit /b 1
)

REM Create Models directory if it doesn't exist
if not exist "Models" mkdir Models

REM Build the project
echo Building project in Release configuration...
dotnet build --configuration Release

if %errorlevel% neq 0 (
    echo Error: Build failed. Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
echo To run the application:
echo 1. Download efficientdet_lite0.tflite model from MediaPipe model zoo
echo 2. Place it in Models\ directory
echo 3. Run: bin\Release\net9.0-windows\ObjectDetectionApp.exe
echo.
echo The application will run in demo mode if the model file is missing.
echo.

REM Check if executable was created
if exist "bin\Release\net9.0-windows\ObjectDetectionApp.exe" (
    echo Executable created: bin\Release\net9.0-windows\ObjectDetectionApp.exe
) else (
    echo Warning: Executable not found. Build may have failed.
)

pause
