using Mediapipe.Tasks.Components.Containers;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using Rect = Mediapipe.Tasks.Components.Containers.Rect;

namespace FaceDetectionApp;

/// <summary>
///     Utility class for visualizing face detection results on OpenCV Mat
/// </summary>
public static class FaceDetectionVisualizer
{
    // Face keypoint indices for blaze_face_short_range model
    // These correspond to the 6 keypoints: left eye, right eye, nose tip, mouth, left eye tragion, right eye tragion
    private static readonly string[] KeypointNames = new[]
    {
        "Right Eye", // 0
        "Left Eye", // 1
        "Nose Tip", // 2
        "Mouth Center", // 3
        "Right Ear", // 4
        "Left Ear" // 5
    };

    /// <summary>
    ///     Draws face detection results on provided Mat frame
    /// </summary>
    /// <param name="frame">The frame to draw on</param>
    /// <param name="result">Face detection result</param>
    /// <param name="boxColor">Color for bounding boxes</param>
    /// <param name="keypointColor">Color for keypoints</param>
    /// <param name="boxThickness">Thickness of bounding box lines</param>
    /// <param name="keypointRadius">Radius of keypoint points</param>
    /// <param name="showLabels">Whether to show keypoint labels</param>
    public static void DrawFaceDetection(
        Mat frame,
        DetectionResult result,
        Scalar? boxColor = null,
        Scalar? keypointColor = null,
        int boxThickness = 2,
        int keypointRadius = 4,
        bool showLabels = true)
    {
        Console.WriteLine($"[DrawFaceDetection] Starting drawing. Frame size: {frame.Width}x{frame.Height}");

        if (result.Detections == null)
        {
            Console.WriteLine("[DrawFaceDetection] ERROR: result.Detections is null");
            return;
        }

        if (result.Detections.Count == 0)
        {
            Console.WriteLine("[DrawFaceDetection] WARNING: result.Detections.Count is 0");
            return;
        }

        Scalar bboxColor = boxColor ?? new Scalar(0, 255, 0); // Green for bounding boxes
        Scalar kpColor = keypointColor ?? new Scalar(0, 0, 255); // Red for keypoints

        Console.WriteLine($"[DrawFaceDetection] Processing {result.Detections.Count} face detections");

        foreach (DetectionResultItem detection in result.Detections)
        {
            Console.WriteLine(
                $"[DrawFaceDetection] Processing face detection with {detection.Keypoints?.Count ?? 0} keypoints");

            // Draw bounding box
            DrawBoundingBox(frame, detection, bboxColor, boxThickness);

            // Draw keypoints
            if (detection.Keypoints != null && detection.Keypoints.Count > 0)
                DrawKeypoints(frame, detection, kpColor, keypointRadius, showLabels);

            // Draw confidence score if available
            DrawConfidenceScore(frame, detection);
        }

        Console.WriteLine("[DrawFaceDetection] Drawing completed successfully");
    }

    /// <summary>
    ///     Draws bounding box for a face detection
    /// </summary>
    private static void DrawBoundingBox(Mat frame, DetectionResultItem detection, Scalar color, int thickness)
    {
        try
        {
            Rect bbox = detection.BoundingBox;

            // Debug: Log the raw bounding box values
            Console.WriteLine(
                $"[DrawBoundingBox] Raw BoundingBox: Left={bbox.Left}, Top={bbox.Top}, Right={bbox.Right}, Bottom={bbox.Bottom}");

            // Convert normalized coordinates to pixel coordinates
            int x = bbox.Left * frame.Width;
            int y = bbox.Top * frame.Height;
            int width = (bbox.Right - bbox.Left) * frame.Width;
            int height = (bbox.Bottom - bbox.Top) * frame.Height;

            // Debug: Log the calculated pixel coordinates
            Console.WriteLine($"[DrawBoundingBox] Pixel coords: x={x}, y={y}, width={width}, height={height}");

            // Ensure coordinates are within frame bounds
            x = Math.Max(0, Math.Min(x, frame.Width - 1));
            y = Math.Max(0, Math.Min(y, frame.Height - 1));
            width = Math.Max(1, Math.Min(width, frame.Width - x));
            height = Math.Max(1, Math.Min(height, frame.Height - y));

            Point topLeft = new(x, y);
            Point bottomRight = new(x + width, y + height);

            Cv2.Rectangle(frame, topLeft, bottomRight, color, thickness);
            Console.WriteLine($"[DrawBoundingBox] Drew rectangle: {topLeft} to {bottomRight}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawBoundingBox] Error drawing bounding box: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws keypoints for a face detection
    /// </summary>
    private static void DrawKeypoints(Mat frame, DetectionResultItem detection, Scalar color, int radius,
        bool showLabels)
    {
        Console.WriteLine($"[DrawKeypoints] Drawing {detection.Keypoints?.Count ?? 0} keypoints");

        if (detection.Keypoints == null)
            return;

        for (int i = 0; i < detection.Keypoints.Count && i < KeypointNames.Length; i++)
            try
            {
                NormalizedKeypoint keypoint = detection.Keypoints[i];

                // Convert normalized coordinates to pixel coordinates
                int x = (int)(keypoint.X * frame.Width);
                int y = (int)(keypoint.Y * frame.Height);

                // Ensure coordinates are within frame bounds
                x = Math.Max(0, Math.Min(x, frame.Width - 1));
                y = Math.Max(0, Math.Min(y, frame.Height - 1));

                Point point = new(x, y);

                // Draw keypoint circle
                Cv2.Circle(frame, point, radius, color, -1);

                // Draw label if requested
                if (showLabels && i < KeypointNames.Length)
                {
                    string label = KeypointNames[i];
                    Cv2.PutText(frame, label, new Point(x + radius + 2, y - radius),
                        HersheyFonts.HersheySimplex, 0.5, color);
                }

                Console.WriteLine($"[DrawKeypoints] Drew keypoint {KeypointNames[i]} at ({x}, {y})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawKeypoints] Error drawing keypoint {i}: {ex.Message}");
            }
    }

    /// <summary>
    ///     Draws confidence score for a face detection
    /// </summary>
    private static void DrawConfidenceScore(Mat frame, DetectionResultItem detection)
    {
        try
        {
            if (detection.Categories != null && detection.Categories.Count > 0)
            {
                Category category = detection.Categories[0];
                float confidence = category.Score;

                Rect bbox = detection.BoundingBox;
                int x = bbox.Left * frame.Width;
                int y = bbox.Top * frame.Height;

                // Ensure coordinates are within frame bounds
                x = Math.Max(0, Math.Min(x, frame.Width - 1));
                y = Math.Max(10, Math.Min(y - 10, frame.Height - 1)); // Position above the box, with margin

                string confidenceText = $"Face: {confidence:F2}";
                Cv2.PutText(frame, confidenceText, new Point(x, y),
                    HersheyFonts.HersheySimplex, 0.6, new Scalar(255, 255, 255), 2);

                Console.WriteLine($"[DrawConfidenceScore] Drew confidence: {confidenceText}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawConfidenceScore] Error drawing confidence score: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws a simple face detection summary on the frame
    /// </summary>
    public static void DrawDetectionSummary(Mat frame, DetectionResult result, Scalar? textColor = null)
    {
        try
        {
            Scalar color = textColor ?? new Scalar(0, 255, 0);
            string summary = $"Faces detected: {result.Detections?.Count ?? 0}";

            Cv2.PutText(frame, summary, new Point(10, frame.Height - 20),
                HersheyFonts.HersheySimplex, 0.7, color, 2);

            Console.WriteLine($"[DrawDetectionSummary] Drew summary: {summary}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawDetectionSummary] Error drawing summary: {ex.Message}");
        }
    }
}