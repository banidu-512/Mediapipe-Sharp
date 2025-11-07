using System.Runtime.InteropServices;
using Mediapipe;
using Mediapipe.Core;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Image = Mediapipe.Framework.Formats.Image;
using NormalizedLandmark = Mediapipe.Tasks.Components.Containers.NormalizedLandmark;
using Point = OpenCvSharp.Point;
using Size = System.Drawing.Size;

namespace HandMeshApp;

public partial class MainForm : Form
{
    private VideoCapture? _capture;
    private bool _demoMode;
    private long _frameTimestamp;
    private HandLandmarker? _handLandmarker;
    private bool _handLandmarkerAvailable;
    private volatile bool _isDisposing;
    private volatile bool _isRunning;
    private Thread? _processingThread;

    public MainForm()
    {
        InitializeComponent();
        InitializeHandLandmarker();
    }

    private void InitializeHandLandmarker()
    {
        try
        {
            // Log initialization start
            Console.WriteLine("[HandLandmarker] Starting initialization...");

            // Check if model file exists
            string modelPath = "Models/hand_landmarker.task";
            if (!File.Exists(modelPath))
            {
                string errorMsg = $"Model file not found at: {Path.GetFullPath(modelPath)}\n\n" +
                                  "The application will run in demo mode without hand detection.\n\n" +
                                  "To enable hand detection:\n" +
                                  "1. Download the hand_landmarker.task model from:\n" +
                                  "   https://developers.google.com/mediapipe/solutions/vision/hand_landmarker\n" +
                                  "2. Place it in the Models directory\n" +
                                  "3. Restart the application";

                Console.WriteLine($"[HandLandmarker] WARNING: {errorMsg}");
                MessageBox.Show(errorMsg, "Model File Missing - Running in Demo Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _demoMode = true;
                _handLandmarkerAvailable = false;
                return;
            }

            Console.WriteLine($"[HandLandmarker] Model file found at: {Path.GetFullPath(modelPath)}");

            // Check if the model file is a valid MediaPipe model (not just a placeholder)
            if (!IsValidModelFile(modelPath))
            {
                string errorMsg =
                    $"The model file at {Path.GetFullPath(modelPath)} appears to be a placeholder or corrupted file.\n\n" +
                    "The application will run in demo mode without hand detection.\n\n" +
                    "To enable hand detection:\n" +
                    "1. Download the hand_landmarker.task model from:\n" +
                    "   https://developers.google.com/mediapipe/solutions/vision/hand_landmarker\n" +
                    "2. The file should be approximately 3-10MB in size\n" +
                    "3. Replace the placeholder file in the Models directory\n" +
                    "4. Restart the application";

                Console.WriteLine($"[HandLandmarker] WARNING: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _demoMode = true;
                _handLandmarkerAvailable = false;
                return;
            }

            // Initialize HandLandmarker with video mode for real-time processing
            Console.WriteLine("[HandLandmarker] Creating CoreBaseOptions...");
            CoreBaseOptions baseOptions = new(
                modelAssetPath: modelPath,
                delegateCase: CoreBaseOptions.Delegate.CPU
            );

            Console.WriteLine("[HandLandmarker] Creating HandLandmarkerOptions...");
            HandLandmarkerOptions options = new(
                baseOptions,
                VisionRunningMode.VIDEO,
                2
            );

            Console.WriteLine("[HandLandmarker] Creating HandLandmarker from options...");
            _handLandmarker = HandLandmarker.CreateFromOptions(options);

            if (_handLandmarker != null)
            {
                Console.WriteLine("[HandLandmarker] Initialization successful!");
                _handLandmarkerAvailable = true;
                _demoMode = false;
            }
            else
            {
                Console.WriteLine("[HandLandmarker] ERROR: HandLandmarker is null after creation");
                MessageBox.Show("HandLandmarker is null after creation\n\nRunning in demo mode without hand detection.",
                    "Initialization Error - Running in Demo Mode",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                _demoMode = true;
                _handLandmarkerAvailable = false;
            }
        }
        catch (ArgumentException ex)
        {
            string errorMsg = $"Argument error during HandLandmarker initialization: {ex.Message}\n\n" +
                              "This usually indicates the model file is corrupted or not a valid MediaPipe model.\n\n" +
                              "The application will run in demo mode without hand detection.\n\n" +
                              "To fix this issue:\n" +
                              "1. Download a fresh copy of the hand_landmarker.task model from:\n" +
                              "   https://developers.google.com/mediapipe/solutions/vision/hand_landmarker\n" +
                              "2. Replace the file in the Models directory\n" +
                              "3. Restart the application";

            Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _handLandmarkerAvailable = false;
        }
        catch (InvalidOperationException ex)
        {
            string errorMsg = $"Invalid operation during HandLandmarker initialization: {ex.Message}\n\n" +
                              "The application will run in demo mode without hand detection.\n\n" +
                              "Stack Trace:\n{ex.StackTrace}";

            Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Invalid Operation Error - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _handLandmarkerAvailable = false;
        }
        catch (MediaPipeException ex)
        {
            string errorMsg = $"MediaPipe error during HandLandmarker initialization: {ex.Message}\n\n" +
                              "This typically indicates the model file is not a valid MediaPipe model or is corrupted.\n\n" +
                              "The application will run in demo mode without hand detection.\n\n" +
                              "To fix this issue:\n" +
                              "1. Download the correct hand_landmarker.task model from:\n" +
                              "   https://developers.google.com/mediapipe/solutions/vision/hand_landmarker\n" +
                              "2. Ensure the file is complete and not corrupted (should be 3-10MB)\n" +
                              "3. Replace the file in the Models directory\n" +
                              "4. Restart the application";

            Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "MediaPipe Error - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _handLandmarkerAvailable = false;
        }
        catch (Exception ex)
        {
            string errorMsg = $"Unexpected error during HandLandmarker initialization: {ex.Message}\n\n" +
                              $"Type: {ex.GetType().Name}\n\n" +
                              "The application will run in demo mode without hand detection.\n\n" +
                              $"Stack Trace:\n{ex.StackTrace}";

            Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
            MessageBox.Show(errorMsg, "Unexpected Error - Running in Demo Mode",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _demoMode = true;
            _handLandmarkerAvailable = false;
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
                ? "Hand Mesh Detection - Demo Mode (No Hand Detection)"
                : "Hand Mesh Detection - Mediapipe";
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
            if (_demoMode || !_handLandmarkerAvailable || _handLandmarker == null || _isDisposing)
            {
                // Demo mode: just show the webcam feed without hand detection
                DisplayFrame(frame, "Demo Mode: Hand detection unavailable");
                return;
            }

            // Normal mode: process with HandLandmarker
            using Image image = ConvertMatToImage(frame);

            // Get current timestamp for video processing
            _frameTimestamp += 33; // Approximate 30 FPS (33ms per frame)

            // Detect hand landmarks
            Console.WriteLine($"[ProcessFrame] Detecting hand landmarks for timestamp {_frameTimestamp}...");
            HandLandmarkerResult result = _handLandmarker.DetectForVideo(image, _frameTimestamp);

            if (result.HandLandmarks == null || result.HandLandmarks.Count == 0)
            {
                Console.WriteLine("[ProcessFrame] WARNING: No hand landmarks detected");
                DisplayFrame(frame, "No hand detected");
                return;
            }

            Console.WriteLine($"[ProcessFrame] Detected {result.HandLandmarks?.Count ?? 0} hand(s)");

            // Add detailed landmark diagnostics
            if (result.HandLandmarks != null && result.HandLandmarks.Count > 0)
            {
                Console.WriteLine($"[ProcessFrame] Processing {result.HandLandmarks.Count} hand landmark sets");
                for (int handIndex = 0; handIndex < result.HandLandmarks.Count; handIndex++)
                {
                    NormalizedLandmarks handLandmarks = result.HandLandmarks[handIndex];
                    Console.WriteLine($"[ProcessFrame] Hand {handIndex}: {handLandmarks.landmarks.Count} landmarks");

                    // Check first few landmarks for visibility and coordinate values
                    int sampleCount = Math.Min(5, handLandmarks.landmarks.Count);
                    for (int i = 0; i < sampleCount; i++)
                    {
                        NormalizedLandmark landmark = handLandmarks.landmarks[i];
                        Console.WriteLine(
                            $"[ProcessFrame]   Landmark {i}: X={landmark.X:F3}, Y={landmark.Y:F3}, Z={landmark.Z:F3}, Visibility={landmark.Visibility}, Presence={landmark.Presence}");
                    }

                    // Count visible landmarks
                    int visibleCount = 0;
                    foreach (NormalizedLandmark landmark in handLandmarks.landmarks)
                        if (landmark.Visibility.HasValue && landmark.Visibility.Value > 0.3f)
                            visibleCount++;
                    Console.WriteLine(
                        $"[ProcessFrame]   Visible landmarks (vis > 0.3): {visibleCount}/{handLandmarks.landmarks.Count}");
                }
            }
            else
            {
                Console.WriteLine("[ProcessFrame] WARNING: HandLandmarks is null or empty");
            }

            // Draw hand mesh on frame
            Console.WriteLine("[ProcessFrame] Calling DrawHandMesh...");
            HandMeshVisualizer.DrawHandMesh(frame, result);
            Console.WriteLine("[ProcessFrame] DrawHandMesh completed");

            // Display the frame
            DisplayFrame(frame, $"Hand detected: {result.HandLandmarks?.Count ?? 0} hand(s)");
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

        // HandLandmarker doesn't have Dispose method in current implementation
        // _handLandmarker?.Dispose();
        _handLandmarker = null;
    }
}