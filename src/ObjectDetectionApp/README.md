# Object Detection App

A real-time object detection application built with MediaPipe and OpenCV that detects multiple object categories from
webcam feed.

## Features

- Real-time object detection using MediaPipe's ObjectDetector
- Supports common object categories (person, car, truck, bicycle, motorcycle, bus, dog, cat, chair, bottle, etc.)
- Color-coded bounding boxes for different object categories
- Confidence scores and labels displayed for each detection
- Summary of detected objects with category counts
- Demo mode support when model file is unavailable
- Robust error handling and logging

## Requirements

- .NET 9.0 or later
- Windows OS
- Webcam
- MediaPipe model file (see setup instructions)

## Setup

1. **Download the Model File**
    - Download the EfficientDet Lite0 model from MediaPipe's model zoo
    - Model file: `efficientdet_lite0.tflite` (approximately 5-10MB)
    - Place it in the `Models/` directory

2. **Build the Application**
   ```bash
   # Using the build script
   build.bat
   
   # Or manually using dotnet
   dotnet build --configuration Release
   ```

3. **Run the Application**
   ```bash
   # From the build output directory
   ObjectDetectionApp.exe
   ```

## Model Setup

The application requires the `efficientdet_lite0.tflite` model file to be placed in the `Models/` directory:

```
ObjectDetectionApp/
├── Models/
│   └── efficientdet_lite0.tflite  ← Required model file
├── ObjectDetectionApp.exe
└── ... (other files)
```

### Where to Get the Model

You can download the EfficientDet Lite0 model from:

- [MediaPipe Model Zoo](https://developers.google.com/mediapipe/solutions/vision/object_detector#models)
- [TensorFlow Hub](https://tfhub.dev/tensorflow/lite-model/efficientdet/lite0/detection/metadata/1)

## Supported Object Categories

The app can detect 80+ COCO object categories, including:

- **People**: person
- **Vehicles**: car, truck, bus, motorcycle, bicycle
- **Animals**: dog, cat, horse, cow, sheep, bird
- **Household items**: chair, couch, bed, dining table
- **Food**: banana, apple, sandwich, orange
- **Electronics**: laptop, cell phone, TV, microwave
- **And many more...**

## Usage

1. **Launch the Application**
    - Run `ObjectDetectionApp.exe`
    - Grant camera permissions when prompted

2. **Object Detection**
    - The app will automatically start detecting objects from your webcam feed
    - Colored bounding boxes appear around detected objects
    - Each box shows the object category and confidence score
    - Bottom left shows a summary of all detected objects

3. **Demo Mode**
    - If the model file is missing or invalid, the app runs in demo mode
    - Demo mode shows the webcam feed without object detection
    - Follow the setup instructions to enable full functionality

## Visual Indicators

- **Bounding Box Colors**: Different colors for different object categories
    - Yellow: Person
    - Blue: Car
    - Red: Truck
    - Cyan: Bicycle
    - Magenta: Motorcycle
    - Orange: Bus
    - Green: Chair
    - And more...

- **Labels**: Show object category name and confidence score
- **Summary**: Bottom left shows total count and breakdown by category

## Performance

- **Resolution**: Automatic webcam resolution (typically 640x480)
- **Frame Rate**: ~30 FPS (depending on hardware)
- **Latency**: Real-time processing with minimal delay
- **Memory**: Optimized for continuous operation

## Troubleshooting

### Model File Issues

**"Model File Missing" Error**

```
Solution:
1. Download efficientdet_lite0.tflite
2. Place in Models/ directory
3. Restart application
```

**"Invalid Model File" Error**

```
Solution:
1. Ensure the file is a valid TensorFlow Lite model (not a placeholder)
2. Check file size (should be 5-10MB)
3. Re-download the model if corrupted
```

### Webcam Issues

**"Failed to open webcam" Error**

```
Solution:
1. Check if webcam is connected
2. Verify webcam permissions
3. Try closing other applications using the webcam
4. Restart the application
```

### Performance Issues

**Low Frame Rate**

```
Solution:
1. Close other CPU-intensive applications
2. Ensure sufficient RAM is available
3. Try lowering webcam resolution if possible
```

**High Memory Usage**

```
Solution:
1. Restart application periodically for long-running sessions
2. Ensure model file is not corrupted
3. Check for memory leaks in other applications
```

## Technical Details

### Architecture

- **MediaPipe Integration**: Uses MediaPipe's ObjectDetector for inference
- **OpenCV**: Handles video capture and frame processing
- **Windows Forms**: Provides the user interface
- **Multi-threading**: Separate threads for video processing and UI updates

### Processing Pipeline

1. **Capture**: Webcam frame capture using OpenCV
2. **Preprocessing**: Convert BGR to RGB format for MediaPipe
3. **Inference**: Object detection using EfficientDet Lite0 model
4. **Postprocessing**: Draw bounding boxes and labels
5. **Display**: Show processed frame in UI

### Model Configuration

- **Model**: EfficientDet Lite0 (TensorFlow Lite)
- **Input Size**: Variable (model-dependent)
- **Confidence Threshold**: 0.5 (configurable)
- **Max Results**: 40 (configurable)
- **Delegate**: CPU (can be changed to GPU if supported)

## Build Configuration

### Dependencies

- **Mediapipe**: Core MediaPipe functionality
- **OpenCvSharp4**: Computer vision and video capture
- **Google.Protobuf**: Protocol buffer support
- **Windows Forms**: UI framework

### Build Requirements

- Visual Studio 2022 or later
- .NET 9.0 SDK
- Windows SDK

### Build Script

The included `build.bat` script:

- Builds in Release configuration
- Copies model files to output directory
- Creates a self-contained executable if configured

## License

This application is part of the MediaPipe-Sharp project and follows the same licensing terms.

## Support

For issues and support:

1. Check the troubleshooting section above
2. Verify model file setup
3. Test with different webcam configurations
4. Report issues to the MediaPipe-Sharp repository

## Contributing

Contributions are welcome! Please:

1. Follow the existing code style
2. Add appropriate error handling
3. Update documentation for new features
4. Test thoroughly before submitting

---

**Note**: This application requires a valid `efficientdet_lite0.tflite` model file to function properly. Without it, the
application will run in demo mode showing only the webcam feed.
