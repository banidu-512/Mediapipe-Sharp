# Gesture Recognition App

A comprehensive Windows Forms application that demonstrates real-time hand gesture recognition using MediaPipe library and OpenCV for webcam capture.

## Features

- **Real-time Gesture Recognition**: Recognizes 19 different hand gestures using MediaPipe's HandLandmarker
- **21 Hand Landmarks**: Full hand skeleton with 21 landmark points per hand
- **Multi-hand Support**: Can detect and recognize gestures on up to 2 hands simultaneously
- **Color-coded Gestures**: Each gesture type has its own distinct color for easy identification
- **Confidence Scoring**: Shows confidence scores for each recognized gesture
- **Hand Identification**: Distinguishes between left and right hands
- **Webcam Integration**: Real-time webcam capture and processing at ~30 FPS
- **Demo Mode**: Runs without gesture recognition when model file is missing or corrupted
- **Robust Error Handling**: Comprehensive error handling with clear user instructions
- **Gesture History**: Maintains a history of recognized gestures for smooth detection

## Recognized Gestures

### Basic Gestures
- **Thumbs Up**: Thumb extended upward, other fingers folded
- **Thumbs Down**: Thumb extended downward, other fingers folded
- **Point**: Index finger extended, other fingers folded
- **Fist**: All fingers folded into fist
- **Open Palm**: All fingers extended

### Number Gestures
- **One**: Index finger extended upward
- **Two**: Index and middle fingers extended (Peace/Victory)
- **Three**: Index, middle, and ring fingers extended
- **Four**: All fingers except thumb extended
- **Five**: All fingers extended (same as Open Palm)

### Game Gestures
- **Rock**: Fist with thumb and pinky extended
- **Paper**: All fingers extended flat
- **Scissors**: Index and middle fingers extended, others folded

### Advanced Gestures
- **OK**: Thumb and index finger tips touching in a circle
- **Victory**: Index and middle fingers extended in V-shape (Peace sign)
- **Call Me**: Thumb and pinky extended, other fingers folded
- **Shaka**: Thumb and pinky extended, middle/ring folded (Hawaiian greeting)
- **Heart Sign**: Thumb and index forming heart shape
- **Come Here**: Index finger pointing downward in beckoning motion
- **Go Away**: Open hand facing forward in pushing motion
- **Stop Hand**: Open hand facing forward in stop gesture

## Requirements

- .NET 9.0 Windows
- Windows Forms
- OpenCvSharp4 packages
- MediaPipe library (local project reference)
- Webcam device

## Model Files

The application requires `hand_landmarker.task` model file to be placed in Models directory for gesture recognition functionality. This file contains the trained hand landmark detection model.

**Important**: The application includes a placeholder file that is NOT a valid model. You must download the actual model file to enable gesture recognition.

## Model Download Instructions

### Where to Download
Download `hand_landmarker.task` model file from the official MediaPipe website:
https://developers.google.com/mediapipe/solutions/vision/hand_landmarker

### Expected File Specifications
- **File name**: `hand_landmarker.task`
- **File size**: Approximately 10-20 MB (typical: ~13MB)
- **File format**: MediaPipe model file (binary format)
- **Required version**: Compatible with MediaPipe .NET bindings

### Installation Steps
1. Navigate to [MediaPipe Hand Landmarker](https://developers.google.com/mediapipe/solutions/vision/hand_landmarker)
2. Download the hand_landmarker.task model file
3. Verify the downloaded file is at least 5MB in size (smaller files are likely placeholders)
4. Place the file in the `GestureRecognitionApp/Models/` directory, replacing the existing placeholder
5. Run the application

### Troubleshooting Model Issues
If you encounter model validation errors or the application runs in demo mode unexpectedly:

1. **Check file size**: Ensure the file is at least 5MB in size (valid models are typically 10-20MB)
2. **Verify download**: Re-download the model if the file seems corrupted
3. **Check file location**: Ensure it's in the `Models/` directory
4. **File permissions**: Make sure the application has read access to the file
5. **Binary content**: Ensure the file is binary data, not a text placeholder

## Usage

### Normal Mode (With Gesture Recognition)
1. Download and install the model file as described above
2. Build the solution
3. Run the application
4. Allow webcam access when prompted
5. Position your hands in view of the webcam
6. Make gestures and see real-time recognition results

### Demo Mode (Without Gesture Recognition)
If the model file is missing or corrupted, the application automatically runs in demo mode:
1. Build and run the application (no model file required)
2. The application will display the webcam feed without gesture recognition
3. Status text will indicate "Demo Mode: Gesture recognition unavailable"
4. You can still use the webcam functionality

## Gesture Recognition Visualization

When gesture recognition is working, the application displays:

### Visual Elements
- **Colored Landmarks**: 21 landmark points per hand, color-coded by gesture type
- **Hand Connections**: Lines connecting landmarks to show hand structure
- **Gesture Labels**: Text labels showing recognized gesture with confidence score
- **Hand Indicators**: (L) for left hand, (R) for right hand
- **Confidence Scores**: Numerical confidence values (0.00-1.00) for each gesture
- **Status Bar**: Current mode and detected gestures summary
- **Performance Indicator**: FPS counter in top-right corner

### Color Coding
Each gesture type has a unique color:
- **Thumbs Up**: Green
- **Thumbs Down**: Red
- **OK**: Cyan
- **Point**: Orange
- **Fist**: Magenta
- **Open Palm**: Yellow
- **Victory**: Pink
- **One**: Gold
- **Two**: Spring Green
- **Three**: Light Pink
- **Four**: Blue Violet
- **Five**: Light Yellow
- **Rock**: Saddle Brown
- **Paper**: White
- **Scissors**: Deep Sky Blue
- **Call Me**: Deep Pink
- **Shaka**: Spring Green
- **Heart**: Light Pink
- **Come Here**: Orange
- **Go Away**: Red Orange
- **Stop Hand**: Red
- **Unknown**: Gray

## Technical Details

- **Framework**: .NET 9.0 Windows Forms
- **Computer Vision**: MediaPipe HandLandmarker with custom gesture recognition
- **Image Processing**: OpenCVSharp4
- **Hand Detection**: 21 landmark points per hand
- **Running Mode**: VIDEO mode for real-time processing
- **Processing Rate**: Approximately 30 FPS
- **Multi-threaded Design**: Responsive UI with background processing
- **Memory Management**: Proper resource cleanup on application exit
- **Error Recovery**: Graceful fallback to demo mode when model is unavailable

## Gesture Recognition Algorithm

The application uses a sophisticated geometric analysis approach:

### Landmark Analysis
- **21 Hand Landmarks**: Full hand skeleton including wrist, finger joints, and fingertips
- **Geometric Calculations**: Distance, angle, and spatial relationship analysis
- **Finger Extension Detection**: Determines if fingers are extended or folded
- **Hand Pose Recognition**: Analyzes overall hand configuration

### Recognition Process
1. **Hand Detection**: MediaPipe detects hand landmarks in real-time
2. **Feature Extraction**: Calculate geometric features from landmark positions
3. **Pattern Matching**: Compare features against gesture templates
4. **Confidence Scoring**: Rate confidence of each possible gesture
5. **Result Selection**: Choose highest confidence gesture above threshold

### Gesture Classification
- **Rule-based Recognition**: Each gesture has specific geometric rules
- **Confidence Weighting**: Different features contribute different weights
- **Threshold Filtering**: Gestures below 30% confidence are marked as unknown
- **Multi-hand Processing**: Independent recognition for each detected hand

## Error Handling

The application handles various error scenarios gracefully:

### Model File Issues
- **Missing model file**: Shows a warning and runs in demo mode
- **Corrupted/invalid model**: Detects invalid files and runs in demo mode
- **Placeholder file**: Recognizes text placeholders and provides download instructions

### Runtime Issues
- **Webcam access failures**: Clear error messages and troubleshooting steps
- **Processing errors**: Continues running with status indicators
- **Memory management**: Proper cleanup of OpenCV and MediaPipe resources
- **Thread synchronization**: Prevents disposed object errors during form closing

### Performance Optimization
- **Frame rate limiting**: Maintains consistent 30 FPS processing
- **Memory efficiency**: Disposes of temporary objects properly
- **UI responsiveness**: Background processing with thread-safe UI updates
- **Resource monitoring**: Detects and handles memory pressure

## Troubleshooting

### Common Issues and Solutions

**"Model validation failed" Error**
- **Cause**: The model file is a placeholder, corrupted, or not binary data
- **Solution**: Download the actual model file from the MediaPipe website
- **Verification**: Ensure the file is at least 5MB in size and contains binary data

**"Failed to open webcam"**
- **Cause**: Webcam is disconnected or used by another application
- **Solution**: Connect a webcam and close other applications using it

**"Failed to initialize HandLandmarker"**
- **Cause**: Model file is missing, corrupted, or incompatible
- **Solution**: Follow the model download and setup instructions above

**Performance Issues**
- **Solution**: Try reducing webcam resolution or closing other applications
- **Optimization**: Ensure adequate lighting conditions for hand detection

**"No gestures detected"**
- **Cause**: Poor lighting, hands not in camera view, or improper hand positioning
- **Solution**: Improve lighting, position hands clearly in front of camera
- **Calibration**: Try different hand distances and orientations

**"Cannot access a disposed object" Error**
- **Cause**: Form is being closed while video processing is still running
- **Solution**: This issue is fixed with proper thread synchronization

**Application Shows Demo Mode Unexpectedly**
- **Cause**: Model file is not detected as valid or is missing
- **Solution**: Verify the model file size and integrity, re-download if necessary

## Code Structure

- `MainForm.cs`: Main application logic, webcam processing, and error handling
- `MainForm.Designer.cs`: Windows Forms designer-generated code
- `GestureRecognitionVisualizer.cs`: Utility class for drawing hand landmarks and gesture results
- `GestureRecognizer.cs`: Core gesture recognition algorithms and logic
- `Program.cs`: Application entry point
- `Models/hand_landmarker.task`: Model file (replace placeholder with actual model)

## Development Notes

### Adding New Gestures
When extending the application with new gestures:

1. **Analyze Gesture**: Understand the geometric pattern of the new gesture
2. **Implement Recognition**: Add a new `CheckGestureName()` method in `GestureRecognizer.cs`
3. **Add Color Mapping**: Update `GestureColors` dictionary in `GestureRecognitionVisualizer.cs`
4. **Update Enum**: Add new gesture type to `GestureType` enum
5. **Test Thoroughly**: Verify recognition works with various hand positions

### Gesture Recognition Best Practices
1. **Geometric Features**: Use distances, angles, and relative positions
2. **Threshold Tuning**: Adjust confidence thresholds for reliable recognition
3. **Error Handling**: Handle edge cases and invalid inputs gracefully
4. **Performance**: Keep recognition algorithms efficient for real-time processing

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

## Performance Characteristics

### System Requirements
- **CPU**: Intel i5 or equivalent recommended
- **Memory**: 4GB RAM minimum, 8GB recommended
- **Camera**: USB webcam or integrated camera
- **OS**: Windows 10/11 with .NET 9.0 runtime

### Expected Performance
- **Frame Rate**: 25-30 FPS with 640x480 resolution
- **Latency**: <100ms gesture recognition latency
- **Accuracy**: >90% recognition accuracy for clear gestures
- **Resource Usage**: <500MB memory usage during operation

### Optimization Tips
- **Lighting**: Good, even lighting improves recognition accuracy
- **Distance**: Hands should be 30-80cm from camera for best results
- **Background**: Plain, contrasting backgrounds improve detection
- **Movement**: Smooth, deliberate gestures work better than rapid motions

## License

This project uses MediaPipe and OpenCV libraries. Please refer to their respective licenses for usage terms.

- **MediaPipe**: Apache License 2.0
- **OpenCV**: Apache License 2.0
- **OpenCVSharp4**: BSD 3-Clause License
