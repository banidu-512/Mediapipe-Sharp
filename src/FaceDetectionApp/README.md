# FaceDetectionApp

A Windows Forms application that demonstrates real-time face detection using Mediapipe library and OpenCV for webcam
capture.

## Features

- Real-time face detection using Mediapipe's FaceDetector
- Visualizes face bounding boxes and 6 keypoints (right eye, left eye, nose tip, mouth center, right ear, left ear)
- Webcam capture using OpenCV
- Face detection overlay on video feed for debugging purposes
- **Demo mode**: Runs without face detection when model file is missing or corrupted
- Robust error handling with clear user instructions

## Requirements

- .NET 9.0 Windows
- Windows Forms
- OpenCvSharp4 packages
- Mediapipe library (local project reference)
- Webcam device

## Model Files

The application requires `blaze_face_short_range.tflite` model file to be placed in Models directory for face detection
functionality. This file contains the trained face detection model.

**Important**: The application includes a placeholder file that is NOT a valid model. You must copy the actual model
file to enable face detection.

## Model Setup Instructions

### Model File Location

The `blaze_face_short_range.tflite` model should be placed in the `FaceDetectionApp/Models/` directory.

### Expected File Specifications

- **File name**: `blaze_face_short_range.tflite`
- **File size**: Approximately 1-2 MB
- **File format**: TensorFlow Lite model file (binary format)
- **Source**: Usually available from FaceMeshApp/Models directory in this project

### Installation Steps

1. Copy the `blaze_face_short_range.tflite` model file from `FaceMeshApp/Models/` directory
2. Place it in the `FaceDetectionApp/Models/` directory
3. Run the application

### Troubleshooting Model Issues

If you encounter model validation errors or the application runs in demo mode unexpectedly:

1. **Check file size**: Ensure the file is at least 1 MB in size (smaller files are likely placeholders)
2. **Verify copy**: Re-copy the model from the FaceMeshApp directory
3. **Check file location**: Ensure it's in the `Models/` directory
4. **File permissions**: Make sure the application has read access to the file
5. **Binary content**: Ensure the file is binary data, not a text placeholder

## Usage

### Normal Mode (With Face Detection)

1. Ensure the model file is properly placed as described above
2. Build the solution
3. Run the application
4. Allow webcam access when prompted
5. The application will display the webcam feed with face detection overlay

### Demo Mode (Without Face Detection)

If the model file is missing or corrupted, the application automatically runs in demo mode:

1. Build and run the application (no model file required)
2. The application will display the webcam feed without face detection
3. Status text will indicate "Demo Mode: Face detection unavailable"
4. You can still use the webcam functionality

## Face Detection Visualization

When face detection is working, the application draws:

- **Green rectangles**: Bounding boxes around detected faces
- **Red circles**: The 6 facial keypoints (eyes, nose tip, mouth center, ears)
- **White text**: Confidence scores and keypoint labels
- **Green text**: Status information at the bottom showing number of faces detected
- **Green text**: Status information at the top showing current mode

## Technical Details

- Uses `VisionRunningMode.VIDEO` for optimal real-time performance
- Processes frames at approximately 30 FPS
- Multi-threaded design for responsive UI
- Proper resource cleanup on application exit
- Graceful fallback to demo mode when the model is unavailable
- Comprehensive error handling with user-friendly messages
- Thread-safe UI updates to prevent disposed object errors

## Face Detection vs Face Mesh

This application uses **FaceDetector** which provides:

- **Face bounding boxes**: Rectangular regions around detected faces
- **6 keypoints**: Essential facial landmarks (right eye, left eye, nose tip, mouth center, right ear, left ear)
- **Faster processing**: Optimized for real-time face detection
- **Lower resource usage**: Compared to full face mesh detection

This is different from **FaceLandmarker** which provides:

- **468 landmarks**: Complete facial mesh with detailed points
- **Face mesh connections**: Lines connecting landmarks to form facial structure
- **More detailed analysis**: Suitable for facial expression analysis, AR filters, etc.

## Error Handling

The application handles various error scenarios gracefully:

### Model File Issues

- **Missing model file**: Shows a warning and runs in demo mode
- **Corrupted/invalid model**: Detects invalid files and runs in demo mode
- **Placeholder file**: Recognizes text placeholders and provides setup instructions

### Runtime Issues

- **Webcam access failures**: Clear error messages and troubleshooting steps
- **Processing errors**: Continues running with status indicators
- **Memory management**: Proper cleanup of resources
- **Thread synchronization**: Prevents disposed object errors during form closing

## Troubleshooting

### Common Issues and Solutions

**"Model validation failed" Error**

- **Cause**: The model file is a placeholder, corrupted, or not binary data
- **Solution**: Copy the valid model file from FaceMeshApp/Models directory
- **Verification**: Ensure the file is at least 1 MB in size and contains binary data

**"Failed to open webcam"**

- **Cause**: Webcam is disconnected or used by another application
- **Solution**: Connect a webcam and close other applications using it

**"Failed to initialize FaceDetector"**

- **Cause**: Model file is missing, corrupted, or incompatible
- **Solution**: Follow the model setup instructions above

**Performance Issues**

- **Solution**: Try reducing webcam resolution or closing other applications

**Application Shows Demo Mode Unexpectedly**

- **Cause**: Model file is not detected as valid
- **Solution**: Verify the model file size and integrity, re-copy from FaceMeshApp if necessary

**"Cannot access a disposed object" Error**

- **Cause**: Form is being closed while video processing is still running
- **Solution**: This issue has been fixed with proper thread synchronization

## Code Structure

- `MainForm.cs`: Main application logic, webcam processing, and error handling
- `MainForm.Designer.cs`: Windows Forms designer-generated code
- `FaceDetectionVisualizer.cs`: Utility class for drawing face detection results on OpenCV Mat
- `Program.cs`: Application entry point
- `Models/blaze_face_short_range.tflite`: Model file (copy from FaceMeshApp/Models)

## Development Notes

### Adding New Features

When extending the application, maintain demo mode functionality:

1. Check `_faceDetectorAvailable` before using face detection features
2. Provide appropriate fallback behavior in demo mode
3. Update status messages to reflect the current mode

### Error Handling Pattern

Follow the established error handling pattern:

1. Catch specific exceptions where possible
2. Provide clear, actionable error messages
3. Include instructions for resolving the issue
4. Allow the application to continue in a degraded mode when possible

### Thread Safety

When working with UI from background threads:

1. Use `Invoke` for UI updates
2. Check `IsDisposed` and `_isDisposing` flags before accessing UI controls
3. Handle `ObjectDisposedException` and `InvalidOperationException` gracefully
4. Ensure proper cleanup in form closing events

## FaceDetector API

This application uses the MediaPipe FaceDetector API with the following configuration:

```csharp
var options = new FaceDetectorOptions(
    baseOptions: baseOptions,
    runningMode: VisionRunningMode.VIDEO,
    minDetectionConfidence: 0.5f,
    minSuppressionThreshold: 0.3f,
    numFaces: 3
);
```

### Detection Results

The FaceDetector returns `DetectionResult` containing:

- **Detections**: List of detected faces
- **BoundingBox**: Normalized coordinates (0.0-1.0) for each face
- **Keypoints**: 6 facial keypoints with normalized coordinates
- **Categories**: Confidence scores for each detection
- **Score**: Detection confidence (0.0-1.0)

## License

This project uses MediaPipe and OpenCV libraries. Please refer to their respective licenses for usage terms.
