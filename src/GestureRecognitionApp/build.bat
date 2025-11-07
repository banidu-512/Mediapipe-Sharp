@echo off
echo Building GestureRecognitionApp...

REM Clean previous build artifacts
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM Build the project
dotnet build --configuration Release

REM Check if build was successful
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Build completed successfully!
    echo.
    echo Running GestureRecognitionApp...
    echo.
    
    REM Change to output directory and run the application
    cd "bin\Release\net9.0-windows"
    
    REM Check if executable exists
    if exist "GestureRecognitionApp.exe" (
        echo Starting GestureRecognitionApp...
        start GestureRecognitionApp.exe
    ) else (
        echo Error: GestureRecognitionApp.exe not found in output directory
        echo Please check the build output above for any errors.
        pause
    )
) else (
    echo.
    echo Build failed with error code %ERRORLEVEL%
    echo Please check the error messages above for details.
    pause
)
