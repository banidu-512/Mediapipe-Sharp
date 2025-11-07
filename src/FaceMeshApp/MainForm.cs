using System.Runtime.InteropServices;
using Mediapipe;
using Mediapipe.Core;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Image = Mediapipe.Framework.Formats.Image;
using NormalizedLandmark = Mediapipe.Tasks.Components.Containers.NormalizedLandmark;
using Point = OpenCvSharp.Point;
using Size = System.Drawing.Size;

namespace FaceMeshApp;

public partial class MainForm : Form
{
    private VideoCapture? _capture;
    private bool _demoMode;
    private FaceLandmarker? _faceLandmarker;
    private bool _faceLandmarkerAvailable;
    private long _frameTimestamp;
    private volatile bool _isDisposing;
    private volatile bool _isRunning;
    private Thread? _processingThread;

    public MainForm()
    {
        InitializeComponent();
        InitializeFaceLandmarker();
    }

    private void InitializeFaceLandmarker()
    {
        try
        {
            // Log initialization start
            Console.WriteLine("[FaceLandmarker] Starting initialization...");

            // Check if model file exists
            string modelPath = "Models/face_landmarker.task";
            if (!File.Exists(modelPath))
            {
                string errorMsg = $"Model file not found at: {Path.GetFullPath(modelPath)}\n\n" +
                                  "The application will run in demo mode without face detection.\n\n" +
                                  "To enable face detection:\n" +
                                  "1. Download the face_landmarker.task model from:\n" +
                                  "   https://developers.google.com/mediapipe/solutions/vision/face_landmarker\n" +
                                  "2. Place it in the Models directory\n" +
                                  "3. Restart the application";

                Console.WriteLine($"[FaceLandmarker] WARNING: {errorMsg}");
                MessageBox.Show(errorMsg, "Model File Missing - Running in Demo Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _demoMode = true;
                _faceLandmarkerAvailable = false;
                return;
            }

            Console.WriteLine($"[FaceLandmarker] Model file found at: {Path.GetFullPath(modelPath)}");

            // Check if the model file is a valid MediaPipe model (not just a placeholder)
            if (!IsValidModelFile(modelPath))
            {
                string errorMsg =
                    $"The model file at {Path.GetFullPath(modelPath)} appears to be a placeholder or corrupted file.\n\n" +
                    "The application will run in demo mode without face detection.\n\n" +
                    "To enable face detection:\n" +
                    "1. Download the face_landmarker.task model from:\n" +
                    "   https://developers.google.com/mediapipe/solutions/vision/face_landmarker\n" +
                    "2. The file should be approximately 3-10MB in size\n" +
                    "3. Replace the placeholder file in the Models directory\n" +
                    "4. Restart the application";

                Console.WriteLine($"[FaceLandmarker] WARNING: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _demoMode = true;
                _faceLandmarkerAvailable = false;
                return;
            }

            // Initialize FaceLandmarker with video mode for real-time processing
            Console.WriteLine("[FaceLandmarker] Creating CoreBaseOptions...");
            CoreBaseOptions baseOptions = new(
                modelAssetPath: modelPath,
                delegateCase: CoreBaseOptions.Delegate.CPU
            );

            Console.WriteLine("[FaceLandmarker] Creating FaceLandmarkerOptions...");
            FaceLandmarkerOptions options = new(
                baseOptions,
                VisionRunningMode.VIDEO
            );

            Console.WriteLine("[FaceLandmarker] Creating FaceLandmarker from options...");
            _faceLandmarker = FaceLandmarker.CreateFromOptions(options);

            if (_faceLandmarker != null)
            {
                Console.WriteLine("[FaceLandmarker] Initialization successful!");
                _faceLandmarkerAvailable = true;
                _demoMode = false;
            }
            else
            {
                Console.WriteLine("[FaceLandmarker] ERROR: FaceLandmarker is null after creation");
                MessageBox.Show("FaceLandmarker is null after creation\n\nRunning in demo mode without face detection.",
                    "Initialization Error - Running in Demo Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _demoMode = true;
                _faceLandmarkerAvailable = false;
            }
        }
        catch (ArgumentException ex)
        {
            string errorMsg = $"Argument error during FaceLandmarker initialization: {ex.Message}\n\n" +
                              "This usually indicates the model file is corrupted or not a valid MediaPipe model.\n\n" +
                              "The application will run in demo mode without face detection.\n\n" +
                              "To fix this issue:\n" +
                              "1. Download a fresh copy of the face_landmarker.task model from:\n" +
                              "   https://developers.google.com/mediapipe/solutions/vision/face_landmarker\n" +
                              "2. Replace the file in the Models directory\n" +
                              "3. Restart the application";

            Console.WriteLine($"[FaceLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _faceLandmarkerAvailable = false;
        }
        catch (InvalidOperationException ex)
        {
            string errorMsg = $"Invalid operation during FaceLandmarker initialization: {ex.Message}\n\n" +
                              "The application will run in demo mode without face detection.\n\n" +
                              "Stack Trace:\n{ex.StackTrace}";

            Console.WriteLine($"[FaceLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Invalid Operation Error - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _faceLandmarkerAvailable = false;
        }
        catch (MediaPipeException ex)
        {
            string errorMsg = $"MediaPipe error during FaceLandmarker initialization: {ex.Message}\n\n" +
                              "This typically indicates the model file is not a valid MediaPipe model or is corrupted.\n\n" +
                              "The application will run in demo mode without face detection.\n\n" +
                              "To fix this issue:\n" +
                              "1. Download the correct face_landmarker.task model from:\n" +
                              "   https://developers.google.com/mediapipe/solutions/vision/face_landmarker\n" +
                              "2. Ensure the file is complete and not corrupted (should be 3-10MB)\n" +
                              "3. Replace the file in the Models directory\n" +
                              "4. Restart the application";

            Console.WriteLine($"[FaceLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "MediaPipe Error - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _faceLandmarkerAvailable = false;
        }
        catch (Exception ex)
        {
            string errorMsg = $"Unexpected error during FaceLandmarker initialization: {ex.Message}\n\n" +
                              "Type: {ex.GetType().Name}\n\n" +
                              "The application will run in demo mode without face detection.\n\n" +
                              "Stack Trace:\n{ex.StackTrace}";

            Console.WriteLine($"[FaceLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Unexpected Error - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _faceLandmarkerAvailable = false;
        }
    }

    private bool IsValidModelFile(string modelPath)
    {
        try
        {
            FileInfo fileInfo = new(modelPath);

            // Check file size - a valid MediaPipe face_landmarker.task model should be at least 3MB
            // Updated based on actual model sizes (3.7MB is valid for face_landmarker.task)
            if (fileInfo.Length < 3 * 1024 * 1024) // 3MB
            {
                Console.WriteLine(
                    $"[IsValidModelFile] File too small: {fileInfo.Length} bytes (expected at least 3MB)");
                return false;
            }

            // Check if it's a text file (placeholder)
            using (StreamReader reader = new(modelPath))
            {
                char[] buffer = new char[1024];
                int bytesRead = reader.Read(buffer, 0, buffer.Length);
                string firstChunk = new(buffer, 0, bytesRead);

                // If it starts with '#' or contains "placeholder", it's likely a text placeholder
                if (firstChunk.StartsWith("#") || firstChunk.ToLower().Contains("placeholder"))
                {
                    Console.WriteLine("[IsValidModelFile] File appears to be a text placeholder");
                    return false;
                }
            }

            // Check if file is binary (not text) by examining first bytes
            using (FileStream fileStream = File.OpenRead(modelPath))
            {
                byte[] header = new byte[256];
                int bytesRead = fileStream.Read(header, 0, header.Length);
                if (bytesRead < 256)
                {
                    Console.WriteLine("[IsValidModelFile] File too small to validate binary content");
                    return false;
                }

                // Check if the file contains mostly binary data (not text)
                // Valid MediaPipe models should contain binary data, not readable text
                int nullBytes = 0;
                int controlChars = 0;

                for (int i = 0; i < bytesRead; i++)
                    if (header[i] == 0)
                        nullBytes++;
                    else if (header[i] < 32 && header[i] != 9 && header[i] != 10 && header[i] != 13)
                        controlChars++;

                // If the file has too many null bytes or control characters, it's likely binary
                // If it's mostly readable ASCII, it's probably a text placeholder
                double nullRatio = (double)nullBytes / bytesRead;
                double controlRatio = (double)controlChars / bytesRead;

                if (nullRatio < 0.01 && controlRatio < 0.05)
                {
                    Console.WriteLine("[IsValidModelFile] File appears to be text, not binary model data");
                    return false;
                }
            }

            Console.WriteLine($"[IsValidModelFile] File appears to be valid: {fileInfo.Length} bytes");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IsValidModelFile] Error validating model file: {ex.Message}");
            return false;
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            // Initialize webcam
            _capture = new VideoCapture(0);
            if (!_capture.IsOpened())
            {
                MessageBox.Show("Failed to open webcam.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set up form
            Text = _demoMode
                ? "Face Mesh Detection - Demo Mode (No Face Detection)"
                : "Face Mesh Detection - Mediapipe";
            ClientSize = new Size(640, 480);

            // Start processing thread
            _isRunning = true;
            _processingThread = new Thread(ProcessVideoFrames);
            _processingThread.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize webcam: {ex.Message}", "Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ProcessVideoFrames()
    {
        Mat frame = new();

        while (_isRunning && _capture != null && _capture.IsOpened() && !_isDisposing)
            try
            {
                _capture.Read(frame);

                if (frame.Empty())
                    continue;

                // Process frame with FaceLandmarker or in demo mode
                ProcessFrame(frame);
            }
            catch (Exception ex)
            {
                // Log error but continue processing
                Console.WriteLine($"Error processing frame: {ex.Message}");
            }

        frame.Dispose();
    }

    private void ProcessFrame(Mat frame)
    {
        try
        {
            if (_demoMode || !_faceLandmarkerAvailable || _faceLandmarker == null || _isDisposing)
            {
                // Demo mode: just show the webcam feed without face detection
                DisplayFrame(frame, "Demo Mode: Face detection unavailable");
                return;
            }

            // Normal mode: process with FaceLandmarker
            using Image image = ConvertMatToImage(frame);

            // Get current timestamp for video processing
            _frameTimestamp += 33; // Approximate 30 FPS (33ms per frame)

            // Detect face landmarks
            Console.WriteLine($"[ProcessFrame] Detecting landmarks for timestamp {_frameTimestamp}...");
            FaceLandmarkerResult result = _faceLandmarker.DetectForVideo(image, _frameTimestamp);

            if (result.FaceLandmarks == null || result.FaceLandmarks.Count == 0)
            {
                Console.WriteLine("[ProcessFrame] WARNING: No face landmarks detected");
                DisplayFrame(frame, "No face detected");
                return;
            }

            Console.WriteLine($"[ProcessFrame] Detected {result.FaceLandmarks?.Count ?? 0} face(s)");

            // Add detailed landmark diagnostics
            if (result.FaceLandmarks != null && result.FaceLandmarks.Count > 0)
            {
                Console.WriteLine($"[ProcessFrame] Processing {result.FaceLandmarks.Count} face landmark sets");
                for (int faceIndex = 0; faceIndex < result.FaceLandmarks.Count; faceIndex++)
                {
                    NormalizedLandmarks faceLandmarks = result.FaceLandmarks[faceIndex];
                    Console.WriteLine($"[ProcessFrame] Face {faceIndex}: {faceLandmarks.landmarks.Count} landmarks");

                    // Check first few landmarks for visibility and coordinate values
                    int sampleCount = Math.Min(5, faceLandmarks.landmarks.Count);
                    for (int i = 0; i < sampleCount; i++)
                    {
                        NormalizedLandmark landmark = faceLandmarks.landmarks[i];
                        Console.WriteLine(
                            $"[ProcessFrame]   Landmark {i}: X={landmark.X:F3}, Y={landmark.Y:F3}, Z={landmark.Z:F3}, Visibility={landmark.Visibility}, Presence={landmark.Presence}");
                    }

                    // Count visible landmarks
                    int visibleCount = 0;
                    foreach (NormalizedLandmark landmark in faceLandmarks.landmarks)
                        if (landmark.Visibility.HasValue && landmark.Visibility.Value > 0.3f)
                            visibleCount++;
                    Console.WriteLine(
                        $"[ProcessFrame]   Visible landmarks (vis > 0.3): {visibleCount}/{faceLandmarks.landmarks.Count}");
                }
            }
            else
            {
                Console.WriteLine("[ProcessFrame] WARNING: FaceLandmarks is null or empty");
            }

            // Draw face mesh on frame
            Console.WriteLine("[ProcessFrame] Calling DrawFaceMesh...");
            FaceMeshVisualizer.DrawFaceMesh(frame, result);
            Console.WriteLine("[ProcessFrame] DrawFaceMesh completed");

            // Display the frame
            DisplayFrame(frame, $"Face detected: {result.FaceLandmarks?.Count ?? 0} face(s)");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[ProcessFrame] Argument Error: {ex.Message}");
            DisplayFrame(frame, "Error: Invalid frame data");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[ProcessFrame] Invalid Operation: {ex.Message}");
            DisplayFrame(frame, "Error: Invalid operation");
        }
        catch (MediaPipeException ex)
        {
            Console.WriteLine($"[ProcessFrame] MediaPipe Error: {ex.Message}");
            DisplayFrame(frame, "Error: MediaPipe processing failed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessFrame] Unexpected Error: {ex.Message} | Type: {ex.GetType().Name}");
            DisplayFrame(frame, "Error: Processing failed");
        }
    }

    private void DisplayFrame(Mat frame, string status)
    {
        try
        {
            // Check if form is disposing or disposed before trying to display
            if (_isDisposing || IsDisposed) return;

            // Add status text to the frame
            if (!string.IsNullOrEmpty(status))
                Cv2.PutText(frame, status, new Point(10, 30),
                    HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 0), 2);

            // Convert to Bitmap and display
            Bitmap? bitmap = frame.ToBitmap();

            // Check if we need to invoke on UI thread
            if (InvokeRequired)
            {
                try
                {
                    Invoke(() =>
                    {
                        // Double-check that form is still not disposing/disposed
                        if (!_isDisposing && !IsDisposed && pictureBoxVideo != null)
                        {
                            // Dispose previous image to prevent memory leaks
                            if (pictureBoxVideo.Image != null) pictureBoxVideo.Image.Dispose();
                            pictureBoxVideo.Image = bitmap;
                        }
                        else
                        {
                            // If form is disposing, dispose bitmap
                            bitmap?.Dispose();
                        }
                    });
                }
                catch (ObjectDisposedException)
                {
                    // Form was disposed while we were trying to invoke
                    bitmap?.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // Form is in an invalid state
                    bitmap?.Dispose();
                }
            }
            else
            {
                // We're already on UI thread
                if (pictureBoxVideo != null)
                {
                    // Dispose previous image to prevent memory leaks
                    if (pictureBoxVideo.Image != null) pictureBoxVideo.Image.Dispose();
                    pictureBoxVideo.Image = bitmap;
                }
                else
                {
                    bitmap?.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DisplayFrame] Error displaying frame: {ex.Message}");
        }
    }

    private Image ConvertMatToImage(Mat mat)
    {
        try
        {
            Console.WriteLine($"[ConvertMatToImage] Converting Mat of size {mat.Width}x{mat.Height}...");
            Console.WriteLine(
                $"[ConvertMatToImage] Mat type: {mat.Type()}, channels: {mat.Channels()}, depth: {mat.Depth()}");

            if (mat.Empty())
            {
                Console.WriteLine("[ConvertMatToImage] ERROR: Input Mat is empty");
                throw new ArgumentException("Input Mat cannot be empty");
            }

            // Ensure Mat is in the correct format (8-bit, 3 channels)
            Mat processedMat = new();

            try
            {
                // Convert to 8-bit 3-channel BGR if needed
                if (mat.Type() != MatType.CV_8UC3)
                {
                    Console.WriteLine($"[ConvertMatToImage] Converting Mat from {mat.Type()} to CV_8UC3");

                    if (mat.Channels() == 1)
                        // Convert grayscale to BGR
                        Cv2.CvtColor(mat, processedMat, ColorConversionCodes.GRAY2BGR);
                    else if (mat.Channels() == 4)
                        // Convert BGRA to BGR
                        Cv2.CvtColor(mat, processedMat, ColorConversionCodes.BGRA2BGR);
                    else if (mat.Depth() != MatType.CV_8U)
                        // Convert depth to 8-bit
                        mat.ConvertTo(processedMat, MatType.CV_8UC3);
                    else
                        // Just copy if channels are already correct but type is different
                        mat.CopyTo(processedMat);
                }
                else
                {
                    // Mat is already in correct format, just copy it
                    mat.CopyTo(processedMat);
                }

                Console.WriteLine(
                    $"[ConvertMatToImage] Processed Mat type: {processedMat.Type()}, channels: {processedMat.Channels()}");

                // Convert BGR to RGB for MediaPipe
                Mat rgbMat = new();
                Cv2.CvtColor(processedMat, rgbMat, ColorConversionCodes.BGR2RGB);

                Console.WriteLine($"[ConvertMatToImage] Converted to RGB, size: {rgbMat.Width}x{rgbMat.Height}");

                // Extract raw byte data from Mat
                // Use Marshal.Copy to get the raw byte data
                byte[] imageData = new byte[rgbMat.Width * rgbMat.Height * 3];
                IntPtr ptr = rgbMat.Data;
                Marshal.Copy(ptr, imageData, 0, imageData.Length);

                Console.WriteLine($"[ConvertMatToImage] Extracted image data array of size {imageData.Length}");

                // Create MediaPipe Image with correct format
                Image image = new(
                    ImageFormat.Types.Format.Srgb,
                    rgbMat.Width,
                    rgbMat.Height,
                    rgbMat.Width * 3, // stride = width * 3 channels (RGB)
                    imageData
                );

                rgbMat.Dispose();
                Console.WriteLine("[ConvertMatToImage] Conversion successful");
                return image;
            }
            finally
            {
                processedMat.Dispose();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConvertMatToImage] ERROR: {ex.Message}");
            Console.WriteLine($"[ConvertMatToImage] Stack trace: {ex.StackTrace}");
            throw new Exception($"Failed to convert Mat to Image: {ex.Message}", ex);
        }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Set disposing flag to prevent further UI updates
        _isDisposing = true;

        // Stop processing
        _isRunning = false;

        // Wait for processing thread to finish with timeout
        if (_processingThread != null && _processingThread.IsAlive)
            if (!_processingThread.Join(2000)) // Wait up to 2 seconds
                // If thread doesn't finish, we can't force it from here
                // but we set _isDisposing flag so it should exit gracefully
                Console.WriteLine("[MainForm_FormClosing] Warning: Processing thread did not finish within timeout");

        // Clean up resources
        try
        {
            _capture?.Release();
            _capture = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainForm_FormClosing] Error releasing capture: {ex.Message}");
        }

        // FaceLandmarker doesn't have Dispose method in current implementation
        // _faceLandmarker?.Dispose();
        _faceLandmarker = null;
    }
}