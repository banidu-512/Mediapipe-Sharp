using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestureRecognitionApp
{
    public partial class MainForm : Form
    {
        private VideoCapture? _capture;
        private HandLandmarker? _handLandmarker;
        private Thread? _processingThread;
        private volatile bool _isRunning = false;
        private long _frameTimestamp = 0;
        private bool _handLandmarkerAvailable = false;
        private bool _demoMode = false;
        private volatile bool _isDisposing = false;
        private readonly List<GestureRecognizer.GestureResult> _gestureHistory = new();
        private readonly object _gestureHistoryLock = new object();

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
                                    "The application will run in demo mode without gesture recognition.\n\n" +
                                    "To enable gesture recognition:\n" +
                                    "1. Download hand_landmarker.task model from MediaPipe website\n" +
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
                
                // Check if the model file is valid
                if (!IsValidModelFile(modelPath))
                {
                    string errorMsg = $"The model file at {Path.GetFullPath(modelPath)} appears to be a placeholder or corrupted file.\n\n" +
                                    "The application will run in demo mode without gesture recognition.\n\n" +
                                    "To enable gesture recognition:\n" +
                                    "1. Download hand_landmarker.task model from MediaPipe website\n" +
                                    "2. The file should be approximately 10-20MB in size\n" +
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
                var baseOptions = new CoreBaseOptions(
                    modelAssetPath: modelPath,
                    delegateCase: CoreBaseOptions.Delegate.CPU
                );
                
                Console.WriteLine("[HandLandmarker] Creating HandLandmarkerOptions...");
                var options = new HandLandmarkerOptions(
                    baseOptions: baseOptions,
                    runningMode: VisionRunningMode.VIDEO,
                    numHands: 2
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
                    MessageBox.Show("HandLandmarker is null after creation\n\nRunning in demo mode without gesture recognition.", 
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
                                "The application will run in demo mode without gesture recognition.\n\n" +
                                "To fix this issue:\n" +
                                "1. Download a valid hand_landmarker.task model from the MediaPipe website\n" +
                                "2. Restart the application";
                
                Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _handLandmarkerAvailable = false;
            }
            catch (InvalidOperationException ex)
            {
                string errorMsg = $"Invalid operation during HandLandmarker initialization: {ex.Message}\n\n" +
                                "The application will run in demo mode without gesture recognition.\n\n" +
                                "Stack Trace:\n{ex.StackTrace}";
                
                Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Operation Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _handLandmarkerAvailable = false;
            }
            catch (Mediapipe.Core.MediaPipeException ex)
            {
                string errorMsg = $"MediaPipe error during HandLandmarker initialization: {ex.Message}\n\n" +
                                "This typically indicates the model file is not a valid MediaPipe model or is corrupted.\n\n" +
                                "The application will run in demo mode without gesture recognition.\n\n" +
                                "To fix this issue:\n" +
                                "1. Download the correct hand_landmarker.task model from the MediaPipe website\n" +
                                "2. Ensure the file is complete and not corrupted (should be 10-20MB)\n" +
                                "3. Restart the application";
                
                Console.WriteLine($"[HandLandmarker] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "MediaPipe Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _handLandmarkerAvailable = false;
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected error during HandLandmarker initialization: {ex.Message}\n\n" +
                                "Type: {ex.GetType().Name}\n\n" +
                                "The application will run in demo mode without gesture recognition.\n\n" +
                                "Stack Trace:\n{ex.StackTrace}";
                
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
                var fileInfo = new FileInfo(modelPath);
                
                // Check file size - a valid MediaPipe hand_landmarker.task model should be at least 5MB
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
                this.Text = _demoMode ? "Gesture Recognition - Demo Mode (No Gesture Recognition)" : "Gesture Recognition - Mediapipe";
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

                    // Process frame with HandLandmarker or in demo mode
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
                if (_demoMode || !_handLandmarkerAvailable || _handLandmarker == null || _isDisposing)
                {
                    // Demo mode: just show the webcam feed without gesture recognition
                    DisplayFrame(frame, "Demo Mode: Gesture recognition unavailable");
                    return;
                }

                // Normal mode: process with HandLandmarker
                using var image = ConvertMatToImage(frame);
                
                // Get current timestamp for video processing
                _frameTimestamp += 33; // Approximate 30 FPS (33ms per frame)
                
                // Detect hand landmarks
                Console.WriteLine($"[ProcessFrame] Detecting hand landmarks for timestamp {_frameTimestamp}...");
                var result = _handLandmarker.DetectForVideo(image, _frameTimestamp);
                
                if (result.HandLandmarks == null || result.HandLandmarks.Count == 0)
                {
                    Console.WriteLine("[ProcessFrame] WARNING: No hands detected");
                    DisplayFrame(frame, "No hands detected");
                    return;
                }
                
                Console.WriteLine($"[ProcessFrame] Detected {result.HandLandmarks.Count} hand(s)");
                
                // Recognize gestures for each hand
                var gestureResults = new List<GestureRecognizer.GestureResult>();
                for (int handIndex = 0; handIndex < result.HandLandmarks.Count; handIndex++)
                {
                    var handLandmarks = result.HandLandmarks[handIndex];
                    var handedness = (result.Handedness != null && result.Handedness.Count > handIndex) ? new List<Mediapipe.Tasks.Components.Containers.Classifications> { result.Handedness[handIndex] } : null;
                    
                    var gestureResult = GestureRecognizer.RecognizeGesture(handLandmarks, handedness);
                    gestureResults.Add(gestureResult);
                    
                    Console.WriteLine($"[ProcessFrame] Hand {handIndex}: {gestureResult.Description} (Confidence: {gestureResult.Confidence:F2})");
                }
                
                // Update gesture history
                UpdateGestureHistory(gestureResults);
                
                // Draw hand landmarks and gesture results on frame
                Console.WriteLine("[ProcessFrame] Calling DrawHandGestures...");
                GestureRecognitionVisualizer.DrawHandGestures(frame, result, gestureResults);
                Console.WriteLine("[ProcessFrame] DrawHandGestures completed");
                
                // Draw summary
                GestureRecognitionVisualizer.DrawGestureSummary(frame, gestureResults);
                
                // Display the frame
                string status = GetStatusMessage(gestureResults);
                DisplayFrame(frame, status);
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

        private void UpdateGestureHistory(List<GestureRecognizer.GestureResult> currentGestures)
        {
            lock (_gestureHistoryLock)
            {
                // Add current gestures to history
                foreach (var gesture in currentGestures)
                {
                    _gestureHistory.Add(gesture);
                }
                
                // Keep only last 100 gesture results
                if (_gestureHistory.Count > 100)
                {
                    _gestureHistory.RemoveRange(0, _gestureHistory.Count - 100);
                }
            }
        }

        private string GetStatusMessage(List<GestureRecognizer.GestureResult> gestureResults)
        {
            if (gestureResults.Count == 0)
                return "No gestures detected";
            
            var recognizedGestures = gestureResults.Where(g => g.Gesture != GestureRecognizer.GestureType.Unknown).ToList();
            if (recognizedGestures.Count == 0)
                return "Hands detected but no recognized gestures";
            
            // Count gesture types
            var gestureCounts = new Dictionary<GestureRecognizer.GestureType, int>();
            foreach (var gesture in recognizedGestures)
            {
                gestureCounts[gesture.Gesture] = gestureCounts.GetValueOrDefault(gesture.Gesture, 0) + 1;
            }
            
            // Build status message
            var gestureNames = new List<string>();
            foreach (var kvp in gestureCounts.OrderByDescending(x => x.Value))
            {
                var name = kvp.Key.ToString();
                if (kvp.Value > 1)
                    name += $" (x{kvp.Value})";
                gestureNames.Add(name);
            }
            
            return $"Detected: {string.Join(", ", gestureNames)}";
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

                // Draw status information
                GestureRecognitionVisualizer.DrawStatusInfo(frame, status, _demoMode);

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
            
            // HandLandmarker doesn't have Dispose method in current implementation
            _handLandmarker = null;
        }
    }
}
