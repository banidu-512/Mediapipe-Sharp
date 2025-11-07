# HandMeshApp

A Windows Forms application that demonstrates real-time face mesh detection using Mediapipe library and OpenCV for webcam capture.

## Features

- Real-time face landmark detection using Mediapipe's FaceLandmarker
- Visualizes face mesh with 468 landmark points and connections
- Webcam capture using OpenCV
- Face mesh overlay on video feed for debugging purposes
- **Demo mode**: Runs without face detection when model file is missing or corrupted
- Robust error handling with clear user instructions

## Requirements

- .NET 9.0 Windows
- Windows Forms
- OpenCvSharp4 packages
- Mediapipe library (local project reference)
- Webcam device

## Model Files

The application requires `face_landmarker.task` model file to be placed in Models directory for face detection functionality. This file contains the trained face landmark detection model.

**Important**: The application includes a placeholder file that is NOT a valid model. You must download the actual model file to enable face detection.

## Model Download Instructions

### Where to Download
Download `face_landmarker.task` model file from the official MediaPipe website:
https://developers.google.com/mediapipe/solutions/vision/face_landmarker

### Expected File Specifications
- **File name**: `face_landmarker.task`
- **File size**: Approximately 3-10 MB (3.7MB is common for recent versions)
- **File format**: MediaPipe model file (binary format)
- **Required version**: Compatible with MediaPipe .NET bindings

### Installation Steps
1. Navigate to [MediaPipe Face Landmarker](https://developers.google.com/mediapipe/solutions/vision/face_landmarker)
2. Download the face_landmarker.task model file
3. Verify the downloaded file is at least 3 MB in size (smaller files are likely placeholders)
4. Place the file in the `HandMeshApp/Models/` directory, replacing the existing placeholder
5. Run the application

### Troubleshooting Model Issues
If you encounter model validation errors or the application runs in demo mode unexpectedly:

1. **Check file size**: Ensure the file is at least 3 MB (valid models are typically 3-10MB)
2. **Verify download**: Re-download the model if the file seems corrupted
3. **Check file location**: Ensure it's in the `Models/` directory
4. **File permissions**: Make sure the application has read access to the file
5. **Binary content**: Ensure the file is binary data, not a text placeholder

## Usage

### Normal Mode (With Face Detection)
1. Download and install the model file as described above
2. Build the solution
3. Run the application
4. Allow webcam access when prompted
5. The application will display the webcam feed with face mesh overlay

### Demo Mode (Without Face Detection)
If the model file is missing or corrupted, the application automatically runs in demo mode:
1. Build and run the application (no model file required)
2. The application will display the webcam feed without face detection
3. Status text will indicate "Demo Mode: Face detection unavailable"
4. You can still use the webcam functionality

## Face Mesh Visualization

When face detection is working, the application draws:
- **Green dots**: Individual landmark points (468 points per face)
- **Yellow lines**: Connections between landmarks forming the face mesh structure
- **Visibility filtering**: Only draws landmarks and connections with sufficient visibility confidence
- **Status text**: Shows the number of faces detected or the current mode

## Technical Details

- Uses `VisionRunningMode.VIDEO` for optimal real-time performance
- Processes frames at approximately 30 FPS
- Multi-threaded design for responsive UI
- Proper resource cleanup on application exit
- Graceful fallback to demo mode when the model is unavailable
- Comprehensive error handling with user-friendly messages
- Thread-safe UI updates to prevent disposed object errors

## Error Handling

The application handles various error scenarios gracefully:

### Model File Issues
- **Missing model file**: Shows a warning and runs in demo mode
- **Corrupted/invalid model**: Detects invalid files and runs in demo mode
- **Placeholder file**: Recognizes text placeholders and provides download instructions

### Runtime Issues
- **Webcam access failures**: Clear error messages and troubleshooting steps
- **Processing errors**: Continues running with status indicators
- **Memory management**: Proper cleanup of resources
- **Thread synchronization**: Prevents disposed object errors during form closing

## Troubleshooting

### Common Issues and Solutions

**"Model validation failed" Error**
- **Cause**: The model file is a placeholder, corrupted, or not binary data
- **Solution**: Download the actual model file from the official MediaPipe site
- **Verification**: Ensure the file is at least 3 MB in size and contains binary data

**"Failed to open webcam"**
- **Cause**: Webcam is disconnected or used by another application
- **Solution**: Connect a webcam and close other applications using it

**"Failed to initialize FaceLandmarker"**
- **Cause**: Model file is missing, corrupted, or incompatible
- **Solution**: Follow the model download instructions above

**Performance Issues**
- **Solution**: Try reducing webcam resolution or closing other applications

**Application Shows Demo Mode Unexpectedly**
- **Cause**: Model file is not detected as valid
- **Solution**: Verify the model file size and integrity, re-download if necessary

**"Cannot access a disposed object" Error**
- **Cause**: Form is being closed while video processing is still running
- **Solution**: This issue has been fixed with proper thread synchronization

## Code Structure

- `MainForm.cs`: Main application logic, webcam processing, and error handling
- `MainForm.Designer.cs`: Windows Forms designer-generated code
- `FaceMeshVisualizer.cs`: Utility class for drawing face mesh on OpenCV Mat
- `Program.cs`: Application entry point
- `Models/face_landmarker.task`: Model file (replace placeholder with actual model)

## Development Notes

### Adding New Features
When extending the application, maintain demo mode functionality:
1. Check `_faceLandmarkerAvailable` before using face detection features
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

## License

This project uses MediaPipe and OpenCV libraries. Please refer to their respective licenses for usage terms.