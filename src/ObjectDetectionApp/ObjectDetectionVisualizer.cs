using Mediapipe.Tasks.Components.Containers;
using OpenCvSharp;
using System.Drawing;
using System.Linq;
using Point = OpenCvSharp.Point;

namespace ObjectDetectionApp
{
    /// <summary>
    /// Utility class for visualizing object detection results on OpenCV Mat
    /// </summary>
    public static class ObjectDetectionVisualizer
    {
        // Common object categories with their corresponding colors
        private static readonly Dictionary<string, Scalar> CategoryColors = new()
        {
            { "person", new Scalar(0, 255, 255) },      // Yellow
            { "car", new Scalar(255, 0, 0) },           // Blue
            { "truck", new Scalar(0, 0, 255) },         // Red
            { "bicycle", new Scalar(255, 255, 0) },     // Cyan
            { "motorcycle", new Scalar(255, 0, 255) },  // Magenta
            { "bus", new Scalar(0, 128, 255) },         // Orange
            { "dog", new Scalar(128, 0, 128) },         // Purple
            { "cat", new Scalar(255, 165, 0) },         // Dark Orange
            { "chair", new Scalar(0, 255, 0) },         // Green
            { "bottle", new Scalar(255, 192, 203) },    // Pink
            { "tree", new Scalar(0, 255, 128) },        // Light Green
            { "bird", new Scalar(255, 255, 128) },      // Light Yellow
            { "sheep", new Scalar(255, 128, 255) },    // Light Magenta
            { "cow", new Scalar(128, 255, 128) },       // Light Cyan
            { "horse", new Scalar(255, 128, 128) },     // Light Purple
            { "elephant", new Scalar(128, 128, 255) }, // Light Blue
            { "bear", new Scalar(165, 42, 42) },        // Brown
            { "zebra", new Scalar(255, 255, 255) },     // White
            { "giraffe", new Scalar(255, 200, 100) },  // Light Brown
            { "backpack", new Scalar(100, 100, 255) },  // Lavender
            { "umbrella", new Scalar(255, 0, 128) },    // Rose
            { "handbag", new Scalar(255, 192, 128) },   // Peach
            { "tie", new Scalar(128, 64, 0) },          // Dark Brown
            { "suitcase", new Scalar(192, 192, 192) },   // Silver
            { "frisbee", new Scalar(255, 100, 255) },   // Hot Pink
            { "skis", new Scalar(200, 200, 255) },     // Light Lavender
            { "snowboard", new Scalar(150, 150, 255) }, // Lighter Lavender
            { "sports ball", new Scalar(255, 150, 0) }, // Dark Orange
            { "kite", new Scalar(255, 255, 150) },     // Light Yellow
            { "baseball bat", new Scalar(139, 69, 19) }, // Saddle Brown
            { "baseball glove", new Scalar(255, 218, 185) }, // Peach Puff
            { "skateboard", new Scalar(255, 140, 0) },  // Dark Orange
            { "surfboard", new Scalar(135, 206, 235) }, // Sky Blue
            { "tennis racket", new Scalar(255, 182, 193) }, // Light Pink
            { "wine glass", new Scalar(220, 20, 60) },   // Crimson
            { "cup", new Scalar(255, 215, 0) },         // Gold
            { "fork", new Scalar(192, 192, 192) },      // Silver
            { "knife", new Scalar(192, 192, 192) },     // Silver
            { "spoon", new Scalar(192, 192, 192) },     // Silver
            { "bowl", new Scalar(255, 228, 181) },      // Moccasin
            { "banana", new Scalar(255, 255, 0) },       // Yellow
            { "apple", new Scalar(255, 0, 0) },          // Red
            { "sandwich", new Scalar(210, 180, 140) },   // Tan
            { "orange", new Scalar(255, 165, 0) },       // Dark Orange
            { "broccoli", new Scalar(0, 128, 0) },       // Dark Green
            { "carrot", new Scalar(255, 140, 0) },       // Dark Orange
            { "hot dog", new Scalar(255, 69, 0) },       // Red Orange
            { "pizza", new Scalar(255, 99, 71) },       // Tomato
            { "donut", new Scalar(255, 192, 203) },      // Pink
            { "cake", new Scalar(255, 182, 193) },       // Light Pink
            { "clock", new Scalar(192, 192, 192) },     // Silver
            { "vase", new Scalar(238, 130, 238) },     // Orchid
            { "scissors", new Scalar(192, 192, 192) },   // Silver
            { "teddy bear", new Scalar(139, 69, 19) },   // Saddle Brown
            { "hair drier", new Scalar(255, 215, 0) },   // Gold
            { "toothbrush", new Scalar(255, 255, 255) }, // White
            { "remote", new Scalar(255, 100, 100) },    // Indian Red
        };

        // Default color for unknown categories
        private static readonly Scalar DefaultColor = new Scalar(255, 255, 255); // White

        /// <summary>
        /// Draws object detection results on provided Mat frame with rectangles and labels
        /// </summary>
        /// <param name="frame">The frame to draw on</param>
        /// <param name="result">Object detection result</param>
        /// <param name="showConfidence">Whether to show confidence scores</param>
        /// <param name="showLabels">Whether to show object labels</param>
        /// <param name="boxThickness">Thickness of bounding box lines</param>
        public static void DrawObjectDetection(
            Mat frame, 
            DetectionResult result, 
            bool showConfidence = true,
            bool showLabels = true,
            int boxThickness = 2)
        {
            Console.WriteLine($"[DrawObjectDetection] Starting drawing. Frame size: {frame.Width}x{frame.Height}");
            
            if (result.Detections == null)
            {
                Console.WriteLine("[DrawObjectDetection] ERROR: result.Detections is null");
                return;
            }
            
            if (result.Detections.Count == 0)
            {
                Console.WriteLine("[DrawObjectDetection] WARNING: result.Detections.Count is 0");
                return;
            }

            Console.WriteLine($"[DrawObjectDetection] Processing {result.Detections.Count} detections");

            foreach (var detection in result.Detections)
            {
                Console.WriteLine($"[DrawObjectDetection] Processing detection with {detection.Keypoints?.Count ?? 0} keypoints");
                
                // Get category information
                string categoryName = "Unknown";
                float confidence = 0.0f;
                Scalar boxColor = DefaultColor;
                
                if (detection.Categories != null && detection.Categories.Count > 0)
                {
                    var category = detection.Categories[0];
                    categoryName = category.CategoryName ?? "Unknown";
                    confidence = category.Score;
                    
                    // Get color for this category
                    boxColor = CategoryColors.GetValueOrDefault(categoryName.ToLowerInvariant(), DefaultColor);
                }
                
                // Draw bounding box
                DrawBoundingBox(frame, detection, boxColor, boxThickness);
                
                // Draw label and confidence
                if (showLabels || showConfidence)
                {
                    DrawLabelAndConfidence(frame, detection, categoryName, confidence, showLabels, showConfidence, boxColor);
                }
            }
            
            Console.WriteLine("[DrawObjectDetection] Drawing completed successfully");
        }

        /// <summary>
        /// Draws bounding box for an object detection
        /// </summary>
        private static void DrawBoundingBox(Mat frame, DetectionResultItem detection, Scalar color, int thickness)
        {
            try
            {
                var bbox = detection.BoundingBox;
                
                // Debug: Log the raw bounding box values
                Console.WriteLine($"[DrawBoundingBox] Raw BoundingBox: Left={bbox.Left}, Top={bbox.Top}, Right={bbox.Right}, Bottom={bbox.Bottom}");
                
                // Convert normalized coordinates to pixel coordinates
                int x = (int)(bbox.Left * frame.Width);
                int y = (int)(bbox.Top * frame.Height);
                int width = (int)((bbox.Right - bbox.Left) * frame.Width);
                int height = (int)((bbox.Bottom - bbox.Top) * frame.Height);
                
                // Debug: Log the calculated pixel coordinates
                Console.WriteLine($"[DrawBoundingBox] Pixel coords: x={x}, y={y}, width={width}, height={height}");
                
                // Ensure coordinates are within frame bounds
                x = Math.Max(0, Math.Min(x, frame.Width - 1));
                y = Math.Max(0, Math.Min(y, frame.Height - 1));
                width = Math.Max(1, Math.Min(width, frame.Width - x));
                height = Math.Max(1, Math.Min(height, frame.Height - y));
                
                var topLeft = new Point(x, y);
                var bottomRight = new Point(x + width, y + height);
                
                Cv2.Rectangle(frame, topLeft, bottomRight, color, thickness);
                Console.WriteLine($"[DrawBoundingBox] Drew rectangle: {topLeft} to {bottomRight}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawBoundingBox] Error drawing bounding box: {ex.Message}");
            }
        }

        /// <summary>
        /// Draws label and confidence score for an object detection
        /// </summary>
        private static void DrawLabelAndConfidence(Mat frame, DetectionResultItem detection, string categoryName, float confidence, bool showLabel, bool showConfidence, Scalar color)
        {
            try
            {
                var bbox = detection.BoundingBox;
                int x = (int)(bbox.Left * frame.Width);
                int y = (int)(bbox.Top * frame.Height);
                
                // Ensure coordinates are within frame bounds
                x = Math.Max(0, Math.Min(x, frame.Width - 1));
                y = Math.Max(20, Math.Min(y - 5, frame.Height - 20)); // Position above the box, with margin
                
                // Build label text
                string labelText = "";
                if (showLabel)
                {
                    labelText += categoryName;
                }
                if (showConfidence)
                {
                    if (!string.IsNullOrEmpty(labelText))
                        labelText += " ";
                    labelText += $"{confidence:F2}";
                }
                
                if (!string.IsNullOrEmpty(labelText))
                {
                    // Calculate text size for background rectangle
                    var textSize = Cv2.GetTextSize(labelText, HersheyFonts.HersheySimplex, 0.6, 1, out int baseline);
                    
                    // Draw background rectangle for better visibility
                    var labelTopLeft = new Point(x, y - textSize.Height - 5);
                    var labelBottomRight = new Point(x + textSize.Width, y + 5);
                    Cv2.Rectangle(frame, labelTopLeft, labelBottomRight, new Scalar(0, 0, 0), -1); // Black background
                    
                    // Draw text
                    Cv2.PutText(frame, labelText, new Point(x, y), 
                               HersheyFonts.HersheySimplex, 0.6, color, 1);
                    
                    Console.WriteLine($"[DrawLabelAndConfidence] Drew label: {labelText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawLabelAndConfidence] Error drawing label: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a color for a specific category name
        /// </summary>
        public static Scalar GetCategoryColor(string categoryName)
        {
            return CategoryColors.GetValueOrDefault(categoryName.ToLowerInvariant(), DefaultColor);
        }
    }
}
