# Image Segmentation App

A minimalistic real-time image segmentation application using MediaPipe and OpenCV. This app performs live image segmentation on webcam feed, displaying segmentation masks overlaid on the original video.

## Features

- **Real-time Image Segmentation**: Process webcam frames to segment person from background
- **Simple Visual Overlay**: Display segmentation mask with green color blending
- **Demo Mode**: Fallback functionality when model is missing (shows webcam only)
- **Minimalistic Design**: Clean, simple interface like other MediaPipe apps
- **Performance Optimized**: Video mode processing for real-time performance

## Requirements

- .NET 9.0 or later
- Windows 10 or later
- Webcam
- MediaPipe model file (see setup instructions)

## Setup

1. **Download Model**:
   - Download selfie segmentation model from [MediaPipe Image Segmentation](https://developers.google.com/mediapipe/solutions/vision/image_segmenter)
   - Recommended: `selfie_segmenter.tflite` (approximately 2-5MB)

2. **Place Model File**:
   - Copy the downloaded `.tflite` model file to: `ImageSegmentationApp/Models/selfie_segmenter.tflite`

3. **Build and Run**:
   ```bash
   # Use the provided build script
   cd ImageSegmentationApp
   build.bat

   # Or build manually
   dotnet build --configuration Release

   # Run the application
   dotnet run --configuration Release
   ```

## Usage

1. **Launch Application**: The app will automatically start using your default webcam
2. **Segmentation Display**: The app will display:
   - Original video feed with green segmentation overlay
   - Real-time segmentation results
   - Status indicator showing segmentation mode
3. **Demo Mode**: If model is missing, shows webcam feed with "Demo Mode" status

## Visualization Features

### Segmentation Overlay
- **Green Mask**: Person/foreground segmentation overlay
- **Blended Display**: Semi-transparent mask over original video
- **Real-time Processing**: Live segmentation at ~30 FPS

### Status Display
- **"Segmentation: Active"**: When segmentation is working
- **"Demo Mode: Image segmentation unavailable"**: When model is missing

## Troubleshooting

### "Model File Missing - Running in Demo Mode"
- Download the correct model file as described in Setup
- Ensure the file is named `selfie_segmenter.tflite`
- Place it in the `ImageSegmentationApp/Models/` directory

### "Invalid Model File - Running in Demo Mode"
- Verify that the downloaded file is not corrupted
- Check file size (should be 2-5MB for valid models)
- Ensure the file is binary data, not a text placeholder
- Re-download if necessary

### Webcam Issues
- Ensure webcam is connected and not used by other applications
- Check Windows privacy settings for camera access
- Try restarting the application

## Building from Source

### Prerequisites
- Visual Studio 2022 or later
- .NET 9.0 SDK
- Git (to clone the repository)

### Build Steps
```bash
# Clone the repository
git clone https://github.com/banidu-512/Mediapipe-Sharp.git
cd Mediapipe-Sharp/src

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the ImageSegmentationApp
dotnet run --project ImageSegmentationApp/ImageSegmentationApp.csproj
```

## Model Information

### Selfie Segmentation Model
- **Purpose**: Segments person from background
- **Input**: RGB image (any size, resized internally)
- **Output**: Person mask and background mask

## License

This application follows the same license as the parent MediaPipe-Sharp project. MediaPipe models have their own licensing terms.

## Contributing

Contributions are welcome! Please follow the existing code patterns and ensure compatibility with the overall project structure.

## Support

For issues specific to this app:
1. Check the troubleshooting section above
2. Verify the model file is correctly placed
3. Ensure the webcam is working
4. Review the console output for error messages

For MediaPipe-related issues:
- Refer to the [MediaPipe documentation](https://developers.google.com/mediapipe)
- Check model compatibility and requirements
