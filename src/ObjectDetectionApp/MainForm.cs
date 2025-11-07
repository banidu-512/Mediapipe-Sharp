using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.ObjectDetector;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ObjectDetectionApp
{
    public partial class MainForm : Form
    {
        private VideoCapture? _capture;
        private ObjectDetector? _objectDetector;
        private Thread? _processingThread;
        private volatile bool _isRunning = false;
        private long _frameTimestamp = 0;
        private bool _objectDetectorAvailable = false;
        private bool _demoMode = false;
        private volatile bool _isDisposing = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeObjectDetector();
        }

        private void InitializeObjectDetector()
        {
            try
            {
                // Log initialization start
                Console.WriteLine("[ObjectDetector] Starting initialization...");
                
                // Check if model file exists
                string modelPath = "Models/efficientdet_lite0.tflite";
                if (!File.Exists(modelPath))
                {
                    string errorMsg = $"Model file not found at: {Path.GetFullPath(modelPath)}\n\n" +
                                    "The application will run in demo mode without object detection.\n\n" +
                                    "To enable object detection:\n" +
                                    "1. Copy efficientdet_lite0.tflite model to Models directory\n" +
                                    "2. Restart the application";
                    
                    Console.WriteLine($"[ObjectDetector] WARNING: {errorMsg}");
                    MessageBox.Show(errorMsg, "Model File Missing - Running in Demo Mode", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    _demoMode = true;
                    _objectDetectorAvailable = false;
                    return;
                }
                
                Console.WriteLine($"[ObjectDetector] Model file found at: {Path.GetFullPath(modelPath)}");
                
                // Check if the model file is a valid MediaPipe model (not just a placeholder)
                if (!IsValidModelFile(modelPath))
                {
                    string errorMsg = $"The model file at {Path.GetFullPath(modelPath)} appears to be a placeholder or corrupted file.\n\n" +
                                    "The application will run in demo mode without object detection.\n\n" +
                                    "To enable object detection:\n" +
                                    "1. Copy efficientdet_lite0.tflite model to Models directory\n" +
                                    "2. The file should be approximately 5-10MB in size\n" +
                                    "3. Replace the placeholder file in the Models directory\n" +
                                    "4. Restart the application";
                    
                    Console.WriteLine($"[ObjectDetector] WARNING: {errorMsg}");
                    MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    _demoMode = true;
                    _objectDetectorAvailable = false;
                    return;
                }
                
                // Initialize ObjectDetector with video mode for real-time processing
                Console.WriteLine("[ObjectDetector] Creating CoreBaseOptions...");
                var baseOptions = new CoreBaseOptions(
                    modelAssetPath: modelPath,
                    delegateCase: CoreBaseOptions.Delegate.CPU
                );
                
                Console.WriteLine("[ObjectDetector] Creating ObjectDetectorOptions...");
                var options = new ObjectDetectorOptions(
                    baseOptions: baseOptions,
                    runningMode: VisionRunningMode.VIDEO,
                    maxResults: 40,
                    scoreThreshold: 0.5f
                );

                Console.WriteLine("[ObjectDetector] Creating ObjectDetector from options...");
                _objectDetector = ObjectDetector.CreateFromOptions(options);
                
                if (_objectDetector != null)
                {
                    Console.WriteLine("[ObjectDetector] Initialization successful!");
                    _objectDetectorAvailable = true;
                    _demoMode = false;
                }
                else
                {
                    Console.WriteLine("[ObjectDetector] ERROR: ObjectDetector is null after creation");
                    MessageBox.Show("ObjectDetector is null after creation\n\nRunning in demo mode without object detection.", 
                                  "Initialization Error - Running in Demo Mode", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    _demoMode = true;
                    _objectDetectorAvailable = false;
                }
            }
            catch (ArgumentException ex)
            {
                string errorMsg = $"Argument error during ObjectDetector initialization: {ex.Message}\n\n" +
                                "This usually indicates the model file is corrupted or not a valid MediaPipe model.\n\n" +
                                "The application will run in demo mode without object detection.\n\n" +
                                "To fix this issue:\n" +
                                "1. Copy a valid efficientdet_lite0.tflite model to the Models directory\n" +
                                "2. Restart the application";
                
                Console.WriteLine($"[ObjectDetector] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _objectDetectorAvailable = false;
            }
            catch (InvalidOperationException ex)
            {
                string errorMsg = $"Invalid operation during ObjectDetector initialization: {ex.Message}\n\n" +
                                "The application will run in demo mode without object detection.\n\n" +
                                "Stack Trace:\n{ex.StackTrace}";
                
                Console.WriteLine($"[ObjectDetector] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Operation Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _objectDetectorAvailable = false;
            }
            catch (Mediapipe.Core.MediaPipeException ex)
            {
                string errorMsg = $"MediaPipe error during ObjectDetector initialization: {ex.Message}\n\n" +
                                "This typically indicates the model file is not a valid MediaPipe model or is corrupted.\n\n" +
                                "The application will run in demo mode without object detection.\n\n" +
                                "To fix this issue:\n" +
                                "1. Copy the correct efficientdet_lite0.tflite model to the Models directory\n" +
                                "2. Ensure the file is complete and not corrupted (should be 5-10MB)\n" +
                                "3. Restart the application";
                
                Console.WriteLine($"[ObjectDetector] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "MediaPipe Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _objectDetectorAvailable = false;
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected error during ObjectDetector initialization: {ex.Message}\n\n" +
                                "Type: {ex.GetType().Name}\n\n" +
                                "The application will run in demo mode without object detection.\n\n" +
                                "Stack Trace:\n{ex.StackTrace}";
                
                Console.WriteLine($"[ObjectDetector] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Unexpected Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _objectDetectorAvailable = false;
            }
        }

        private bool IsValidModelFile(string modelPath)
        {
            try
            {
                var fileInfo = new FileInfo(modelPath);
                
                // Check file size - a valid MediaPipe efficientdet_lite0.tflite model should be at least 5MB
                if (fileInfo.Length < 5 * 1024 * 1024) // 5MB
                {
                    Console.WriteLine($"[IsValidModelFile] File too small: {fileInfo.Length} bytes (expected at least 5MB)");
                    return false;
                }
                
                // Check if it's a text file (placeholder)
                using (var reader = new StreamReader(modelPath))
                {
                    char[] buffer = new char[1024];
                    int bytesRead = reader.Read(buffer, 0, buffer.Length);
                    string firstChunk = new string(buffer, 0, bytesRead);
                    
                    // If it starts with '#' or contains "placeholder", it's likely a text placeholder
                    if (firstChunk.StartsWith("#") || firstChunk.ToLower().Contains("placeholder"))
                    {
                        Console.WriteLine("[IsValidModelFile] File appears to be a text placeholder");
                        return false;
                    }
                }
                
                // Check if file is binary (not text) by examining first bytes
                using (var fileStream = File.OpenRead(modelPath))
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
                    {
                        if (header[i] == 0)
                            nullBytes++;
                        else if (header[i] < 32 && header[i] != 9 && header[i] != 10 && header[i] != 13)
                            controlChars++;
                    }
                    
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
                this.Text = _demoMode ? "Object Detection - Demo Mode (No Object Detection)" : "Object Detection - Mediapipe";
                this.ClientSize = new System.Drawing.Size(640, 480);

                // Start processing thread
                _isRunning = true;
                _processingThread = new Thread(ProcessVideoFrames);
                _processingThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize webcam: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessVideoFrames()
        {
            Mat frame = new Mat();
            
            while (_isRunning && _capture != null && _capture.IsOpened() && !_isDisposing)
            {
                try
                {
                    _capture.Read(frame);
                    
                    if (frame.Empty())
                        continue;

                    // Process frame with ObjectDetector or in demo mode
                    ProcessFrame(frame);
                }
                catch (Exception ex)
                {
                    // Log error but continue processing
                    Console.WriteLine($"Error processing frame: {ex.Message}");
                }
            }
            
            frame.Dispose();
        }

        private void ProcessFrame(Mat frame)
        {
            try
            {
                if (_demoMode || !_objectDetectorAvailable || _objectDetector == null || _isDisposing)
                {
                    // Demo mode: just show the webcam feed without object detection
                    DisplayFrame(frame, "Demo Mode: Object detection unavailable");
                    return;
                }

                // Normal mode: process with ObjectDetector
                using var image = ConvertMatToImage(frame);
                
                // Get current timestamp for video processing
                _frameTimestamp += 33; // Approximate 30 FPS (33ms per frame)
                
                // Detect objects
                Console.WriteLine($"[ProcessFrame] Detecting objects for timestamp {_frameTimestamp}...");
                var result = _objectDetector.DetectForVideo(image, _frameTimestamp);
                
                if (result.Detections == null || result.Detections.Count == 0)
                {
                    Console.WriteLine("[ProcessFrame] WARNING: No objects detected");
                    DisplayFrame(frame, "No objects detected");
                    return;
                }
                
                Console.WriteLine($"[ProcessFrame] Detected {result.Detections.Count} object(s)");
                
                // Add detailed detection diagnostics
                for (int objectIndex = 0; objectIndex < result.Detections.Count; objectIndex++)
                {
                    var detection = result.Detections[objectIndex];
                    Console.WriteLine($"[ProcessFrame] Object {objectIndex}: {detection.Keypoints?.Count ?? 0} keypoints");
                    
                    // Check confidence score and category
                    if (detection.Categories != null && detection.Categories.Count > 0)
                    {
                        var confidence = detection.Categories[0].Score;
                        var categoryName = detection.Categories[0].CategoryName ?? "Unknown";
                        Console.WriteLine($"[ProcessFrame]   Category: {categoryName}, Confidence: {confidence:F3}");
                    }
                }
                
                // Draw object detection results on frame
                Console.WriteLine("[ProcessFrame] Calling DrawObjectDetection...");
                ObjectDetectionVisualizer.DrawObjectDetection(frame, result);
                Console.WriteLine("[ProcessFrame] DrawObjectDetection completed");
                
                // Display the frame
                DisplayFrame(frame, $"Objects detected: {result.Detections.Count}");
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
            catch (Mediapipe.Core.MediaPipeException ex)
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
                if (_isDisposing || this.IsDisposed)
                {
                    return;
                }

                // Add status text to the frame
                if (!string.IsNullOrEmpty(status))
                {
                    Cv2.PutText(frame, status, new OpenCvSharp.Point(10, 30), 
                               HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 0), 2);
                }

                // Convert to Bitmap and display
                var bitmap = BitmapConverter.ToBitmap(frame);
                
                // Check if we need to invoke on UI thread
                if (InvokeRequired)
                {
                    try
                    {
                        Invoke(new Action(() => {
                            // Double-check that form is still not disposing/disposed
                            if (!_isDisposing && !this.IsDisposed && pictureBoxVideo != null)
                            {
                                // Dispose previous image to prevent memory leaks
                                if (pictureBoxVideo.Image != null)
                                {
                                    pictureBoxVideo.Image.Dispose();
                                }
                                pictureBoxVideo.Image = bitmap;
                            }
                            else
                            {
                                // If form is disposing, dispose bitmap
                                bitmap?.Dispose();
                            }
                        }));
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
                        if (pictureBoxVideo.Image != null)
                        {
                            pictureBoxVideo.Image.Dispose();
                        }
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

        private Mediapipe.Framework.Formats.Image ConvertMatToImage(Mat mat)
        {
            try
            {
                Console.WriteLine($"[ConvertMatToImage] Converting Mat of size {mat.Width}x{mat.Height}...");
                Console.WriteLine($"[ConvertMatToImage] Mat type: {mat.Type()}, channels: {mat.Channels()}, depth: {mat.Depth()}");
                
                if (mat.Empty())
                {
                    Console.WriteLine("[ConvertMatToImage] ERROR: Input Mat is empty");
                    throw new ArgumentException("Input Mat cannot be empty");
                }
                
                // Ensure Mat is in the correct format (8-bit, 3 channels)
                Mat processedMat = new Mat();
                
                try
                {
                    // Convert to 8-bit 3-channel BGR if needed
                    if (mat.Type() != MatType.CV_8UC3)
                    {
                        Console.WriteLine($"[ConvertMatToImage] Converting Mat from {mat.Type()} to CV_8UC3");
                        
                        if (mat.Channels() == 1)
                        {
                            // Convert grayscale to BGR
                            Cv2.CvtColor(mat, processedMat, ColorConversionCodes.GRAY2BGR);
                        }
                        else if (mat.Channels() == 4)
                        {
                            // Convert BGRA to BGR
                            Cv2.CvtColor(mat, processedMat, ColorConversionCodes.BGRA2BGR);
                        }
                        else if (mat.Depth() != MatType.CV_8U)
                        {
                            // Convert depth to 8-bit
                            mat.ConvertTo(processedMat, MatType.CV_8UC3);
                        }
                        else
                        {
                            // Just copy if channels are already correct but type is different
                            mat.CopyTo(processedMat);
                        }
                    }
                    else
                    {
                        // Mat is already in correct format, just copy it
                        mat.CopyTo(processedMat);
                    }
                    
                    Console.WriteLine($"[ConvertMatToImage] Processed Mat type: {processedMat.Type()}, channels: {processedMat.Channels()}");
                    
                    // Convert BGR to RGB for MediaPipe
                    var rgbMat = new Mat();
                    Cv2.CvtColor(processedMat, rgbMat, ColorConversionCodes.BGR2RGB);
                    
                    Console.WriteLine($"[ConvertMatToImage] Converted to RGB, size: {rgbMat.Width}x{rgbMat.Height}");
                    
                    // Extract raw byte data from Mat
                    // Use Marshal.Copy to get the raw byte data
                    var imageData = new byte[rgbMat.Width * rgbMat.Height * 3];
                    unsafe
                    {
                        IntPtr ptr = rgbMat.Data;
                        System.Runtime.InteropServices.Marshal.Copy(ptr, imageData, 0, imageData.Length);
                    }
                    
                    Console.WriteLine($"[ConvertMatToImage] Extracted image data array of size {imageData.Length}");
                    
                    // Create MediaPipe Image with correct format
                    var image = new Mediapipe.Framework.Formats.Image(
                        Mediapipe.ImageFormat.Types.Format.Srgb,
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
            {
                if (!_processingThread.Join(2000)) // Wait up to 2 seconds
                {
                    // If thread doesn't finish, we can't force it from here
                    // but we set _isDisposing flag so it should exit gracefully
                    Console.WriteLine("[MainForm_FormClosing] Warning: Processing thread did not finish within timeout");
                }
            }
            
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
            
            // ObjectDetector doesn't have Dispose method in current implementation
            // _objectDetector?.Dispose();
            _objectDetector = null;
        }
    }
}
