# Pose Landmarks Detection App

A real-time pose landmarks detection application using MediaPipe and OpenCV for .NET.

## Features

- **Real-time Pose Detection**: Detects human poses using webcam input
- **33 Landmark Points**: Full body skeleton with 33 landmark points per pose
- **Skeleton Visualization**: Draws connected skeleton structure on detected poses
- **Multi-person Support**: Can detect up to 2 people simultaneously
- **Color-coded Body Parts**: Different colors for face, arms, legs, etc.
- **Demo Mode**: Runs without pose detection when model is missing
- **Confidence Scoring**: Shows detection confidence for each pose

## Requirements

- .NET 9.0 or later
- Windows OS (uses Windows Forms)
- Webcam for real-time detection
- MediaPipe Pose Landmarker model file

## Model Setup

1. Download the `pose_landmarker.task` model from:
   https://developers.google.com/mediapipe/solutions/vision/pose_landmarker

2. Place the model file in the `Models/` directory:
   ```
   PoseLandmarksApp/
   ├── Models/
   │   └── pose_landmarker.task
   ```

3. The model file should be approximately 10-20MB in size

## Building and Running

### Build

```bash
cd PoseLandmarksApp
dotnet build
```

### Run

```bash
dotnet run
```

Or use the included build script:

```bash
build.bat
```

## Usage

1. **Launch the application** - It will automatically start webcam capture
2. **Position yourself** in view of the webcam
3. **View pose detection** - Skeleton overlay will appear on detected poses
4. **Multiple poses** - The app can track up to 2 people simultaneously

## Controls

- **Close window** - Stops webcam and exits application
- **Demo Mode** - If model is missing, shows webcam feed without detection

## Pose Landmark Structure

The app detects 33 landmark points per pose:

- **Face (0-10)**: Nose, eyes, ears, mouth
- **Arms (11-22)**: Shoulders, elbows, wrists, hands
- **Torso (11-12, 23-24)**: Shoulders and hips
- **Legs (23-32)**: Hips, knees, ankles, feet

## Visualization Colors

- **Red**: Face landmarks
- **Green**: Torso connections
- **Blue**: Right arm
- **Cyan**: Left arm
- **Magenta**: Right leg
- **Yellow**: Left leg

## Troubleshooting

### Model Not Found

If you see "Demo Mode" message:

1. Download `pose_landmarker.task` from MediaPipe website
2. Place in `Models/` directory
3. Restart application

### No Detection

- Ensure good lighting conditions
- Position yourself clearly in camera view
- Check webcam permissions

### Performance Issues

- Reduce confidence thresholds in MainForm.cs
- Close other applications using webcam
- Ensure adequate system resources

## Technical Details

- **Framework**: .NET 9.0 Windows Forms
- **Computer Vision**: MediaPipe Pose Landmarker
- **Image Processing**: OpenCVSharp4
- **Model**: BlazePose/Ghost pose detection model
- **Running Mode**: VIDEO mode for real-time processing

## Error Handling

The application includes comprehensive error handling:

- **Model validation**: Checks file size and binary format
- **Graceful degradation**: Runs in demo mode without model
- **Memory management**: Proper disposal of OpenCV resources
- **Thread safety**: Safe UI updates from background processing

## License

This project uses:

- MediaPipe: Apache License 2.0
- OpenCV: Apache License 2.0
- OpenCVSharp4: BSD 3-Clause License
