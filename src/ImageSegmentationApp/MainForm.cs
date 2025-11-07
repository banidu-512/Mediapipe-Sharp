using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.ImageSegmenter;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Google.Protobuf.WellKnownTypes;

namespace ImageSegmentationApp
{
    public partial class MainForm : Form
    {
        private VideoCapture? _capture;
        private ImageSegmenter? _imageSegmenter;
        private Thread? _processingThread;
        private volatile bool _isRunning = false;
        private long _frameTimestamp = 0;
        private bool _imageSegmenterAvailable = false;
        private bool _demoMode = false;
        private volatile bool _isDisposing = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeImageSegmenter();
        }

        private void InitializeImageSegmenter()
        {
            try
            {
                // Log initialization start
                Console.WriteLine("[ImageSegmenter] Starting initialization...");
                
                // Check if model file exists
                string modelPath = "Models/selfie_segmenter.tflite";
                if (!File.Exists(modelPath))
                {
                    string errorMsg = $"Model file not found at: {Path.GetFullPath(modelPath)}\n\n" +
                                    "The application will run in demo mode without image segmentation.\n\n" +
                                    "To enable image segmentation:\n" +
                                    "1. Copy selfie_segmenter.tflite model to Models directory\n" +
                                    "2. Restart the application";
                    
                    Console.WriteLine($"[ImageSegmenter] WARNING: {errorMsg}");
                    MessageBox.Show(errorMsg, "Model File Missing - Running in Demo Mode", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    _demoMode = true;
                    _imageSegmenterAvailable = false;
                    return;
                }
                
                Console.WriteLine($"[ImageSegmenter] Model file found at: {Path.GetFullPath(modelPath)}");
                
                // Check if the model file is a valid MediaPipe model (not just a placeholder)
                if (!IsValidModelFile(modelPath))
                {
                    string errorMsg = $"The model file at {Path.GetFullPath(modelPath)} appears to be a placeholder or corrupted file.\n\n" +
                                    "The application will run in demo mode without image segmentation.\n\n" +
                                    "To enable image segmentation:\n" +
                                    "1. Copy selfie_segmenter.tflite model to Models directory\n" +
                                    "2. The file should be approximately 2-5MB in size\n" +
                                    "3. Replace the placeholder file in the Models directory\n" +
                                    "4. Restart the application";
                    
                    Console.WriteLine($"[ImageSegmenter] WARNING: {errorMsg}");
                    MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    _demoMode = true;
                    _imageSegmenterAvailable = false;
                    return;
                }
                
                // Initialize ImageSegmenter with video mode for real-time processing
                Console.WriteLine("[ImageSegmenter] Creating CoreBaseOptions...");
                var baseOptions = new CoreBaseOptions(
                    modelAssetPath: modelPath,
                    delegateCase: CoreBaseOptions.Delegate.CPU
                );
                
                Console.WriteLine("[ImageSegmenter] Creating ImageSegmenterOptions...");
                var options = new ImageSegmenterOptions(
                    baseOptions: baseOptions,
                    runningMode: VisionRunningMode.VIDEO,
                    outputCategoryMask: true,
                    outputConfidenceMasks: true
                );

                Console.WriteLine("[ImageSegmenter] Creating ImageSegmenter from options...");
                _imageSegmenter = ImageSegmenter.CreateFromOptions(options);
                
                if (_imageSegmenter != null)
                {
                    Console.WriteLine("[ImageSegmenter] Initialization successful!");
                    _imageSegmenterAvailable = true;
                    _demoMode = false;
                }
                else
                {
                    Console.WriteLine("[ImageSegmenter] ERROR: ImageSegmenter is null after creation");
                    MessageBox.Show("ImageSegmenter is null after creation\n\nRunning in demo mode without image segmentation.", 
                                  "Initialization Error - Running in Demo Mode", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    
                    _demoMode = true;
                    _imageSegmenterAvailable = false;
                }
            }
            catch (ArgumentException ex)
            {
                string errorMsg = $"Argument error during ImageSegmenter initialization: {ex.Message}\n\n" +
                                "This usually indicates the model file is corrupted or not a valid MediaPipe model.\n\n" +
                                "The application will run in demo mode without image segmentation.\n\n" +
                                "To fix this issue:\n" +
                                "1. Copy a valid selfie_segmenter.tflite model to the Models directory\n" +
                                "2. Restart the application";
                
                Console.WriteLine($"[ImageSegmenter] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Model File - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _imageSegmenterAvailable = false;
            }
            catch (InvalidOperationException ex)
            {
                string errorMsg = $"Invalid operation during ImageSegmenter initialization: {ex.Message}\n\n" +
                                "The application will run in demo mode without image segmentation.\n\n" +
                                "Stack Trace:\n{ex.StackTrace}";
                
                Console.WriteLine($"[ImageSegmenter] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Invalid Operation Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _imageSegmenterAvailable = false;
            }
            catch (Mediapipe.Core.MediaPipeException ex)
            {
                string errorMsg = $"MediaPipe error during ImageSegmenter initialization: {ex.Message}\n\n" +
                                "This typically indicates the model file is not a valid MediaPipe model or is corrupted.\n\n" +
                                "The application will run in demo mode without image segmentation.\n\n" +
                                "To fix this issue:\n" +
                                "1. Copy the correct selfie_segmenter.tflite model to the Models directory\n" +
                                "2. Ensure the file is complete and not corrupted (should be 2-5MB)\n" +
                                "3. Restart the application";
                
                Console.WriteLine($"[ImageSegmenter] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "MediaPipe Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _imageSegmenterAvailable = false;
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected error during ImageSegmenter initialization: {ex.Message}\n\n" +
                                "Type: {ex.GetType().Name}\n\n" +
                                "The application will run in demo mode without image segmentation.\n\n" +
                                "Stack Trace:\n{ex.StackTrace}";
                
                Console.WriteLine($"[ImageSegmenter] ERROR: {errorMsg}");
                MessageBox.Show(errorMsg, "Unexpected Error - Running in Demo Mode", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                _demoMode = true;
                _imageSegmenterAvailable = false;
            }
        }

        private bool IsValidModelFile(string modelPath)
        {
            try
            {
                var fileInfo = new FileInfo(modelPath);
                
                // Check file size - a valid MediaPipe selfie_segmenter.tflite model should be at least 2MB
                if (fileInfo.Length < 1024) // 2MB
                {
                    Console.WriteLine($"[IsValidModelFile] File too small: {fileInfo.Length} bytes (expected at least 2MB)");
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
                this.Text = _demoMode ? "Image Segmentation - Demo Mode (No Segmentation)" : "Image Segmentation - Mediapipe";
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

                    // Process frame with ImageSegmenter or in demo mode
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
                if (_demoMode || !_imageSegmenterAvailable || _imageSegmenter == null || _isDisposing)
                {
                    // Demo mode: just show the webcam feed without segmentation
                    DisplayFrame(frame, "Demo Mode: Image segmentation unavailable");
                    return;
                }

                // Normal mode: process with ImageSegmenter
                using var image = ConvertMatToImage(frame);
                
                if (image == null)
                {
                    Console.WriteLine("[ProcessFrame] ERROR: Failed to convert Mat to Image");
                    DisplayFrame(frame, "Error: Frame conversion failed");
                    return;
                }
                
                // Get current timestamp for video processing
                _frameTimestamp += 33; // Approximate 30 FPS (33ms per frame)
                
                // Perform segmentation
                Console.WriteLine($"[ProcessFrame] Segmenting frame for timestamp {_frameTimestamp}...");
                var result = _imageSegmenter.SegmentForVideo(image, _frameTimestamp);
                
                if (result.Equals(null))
                {
                    Console.WriteLine("[ProcessFrame] WARNING: Segmentation result is null");
                    DisplayFrame(frame, "Error: Segmentation failed");
                    return;
                }
                
                Console.WriteLine("[ProcessFrame] Segmentation successful");
                
                // Draw segmentation results on frame
                Console.WriteLine("[ProcessFrame] Calling DrawSegmentation...");
                ImageSegmentationVisualizer.DrawSegmentation(frame, result);
                Console.WriteLine("[ProcessFrame] DrawSegmentation completed");
                
                // Display the frame
                DisplayFrame(frame, "Segmentation: Active");
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
            
            // ImageSegmenter doesn't have Dispose method in current implementation
            // _imageSegmenter?.Dispose();
            _imageSegmenter = null;
        }
    }
}
