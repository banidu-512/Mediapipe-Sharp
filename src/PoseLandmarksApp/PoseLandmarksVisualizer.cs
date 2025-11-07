using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace PoseLandmarksApp;

/// <summary>
///     Utility class for visualizing pose landmarks on OpenCV Mat
/// </summary>
public static class PoseLandmarksVisualizer
{
    // MediaPipe Pose landmark connections (33 landmarks total)
    // Based on BlazePose/Ghost model landmark structure
    private static readonly int[][] PoseConnections = new[]
    {
        // Face
        new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 7 }, // Right face
        new[] { 0, 4 }, new[] { 4, 5 }, new[] { 5, 6 }, new[] { 6, 8 }, // Left face

        // Torso
        new[] { 11, 12 }, // Shoulders
        new[] { 11, 23 }, new[] { 12, 24 }, // Shoulders to hips
        new[] { 23, 24 }, // Hips

        // Right arm
        new[] { 11, 13 }, new[] { 13, 15 }, // Shoulder to elbow to wrist

        // Left arm  
        new[] { 12, 14 }, new[] { 14, 16 }, // Shoulder to elbow to wrist

        // Right leg
        new[] { 23, 25 }, new[] { 25, 27 }, new[] { 27, 29 }, new[] { 29, 31 }, // Hip to knee to ankle to foot

        // Left leg
        new[] { 24, 26 }, new[] { 26, 28 }, new[] { 28, 30 }, new[] { 30, 32 }, // Hip to knee to ankle to foot

        // Additional connections for better visualization
        new[] { 0, 15 }, new[] { 0, 16 }, // Nose to wrists
        new[] { 15, 17 }, new[] { 15, 19 }, new[] { 15, 21 }, new[] { 17, 19 }, // Right hand
        new[] { 16, 18 }, new[] { 16, 20 }, new[] { 16, 22 }, new[] { 18, 20 }, // Left hand
        new[] { 27, 31 }, new[] { 28, 32 } // Feet connections
    };

    // Pose landmark names for debugging and labeling
    private static readonly string[] LandmarkNames = new[]
    {
        "nose", "left_eye_inner", "left_eye", "left_eye_outer",
        "right_eye_inner", "right_eye", "right_eye_outer", "left_ear",
        "right_ear", "mouth_left", "mouth_right", "left_shoulder",
        "right_shoulder", "left_elbow", "right_elbow", "left_wrist",
        "right_wrist", "left_pinky", "right_pinky", "left_index",
        "right_index", "left_thumb", "right_thumb", "left_hip",
        "right_hip", "left_knee", "right_knee", "left_ankle",
        "right_ankle", "left_heel", "right_heel", "left_foot_index",
        "right_foot_index"
    };

    // Colors for different body parts
    private static readonly Scalar[] BodyPartColors = new[]
    {
        new Scalar(255, 0, 0), // Red - Face
        new Scalar(0, 255, 0), // Green - Torso
        new Scalar(0, 0, 255), // Blue - Right arm
        new Scalar(255, 255, 0), // Cyan - Left arm
        new Scalar(255, 0, 255), // Magenta - Right leg
        new Scalar(0, 255, 255) // Yellow - Left leg
    };

    /// <summary>
    ///     Draws pose landmarks on provided Mat frame
    /// </summary>
    /// <param name="frame">The frame to draw on</param>
    /// <param name="result">Pose landmark detection result</param>
    /// <param name="landmarkColor">Color for landmark points</param>
    /// <param name="connectionColor">Color for connection lines</param>
    /// <param name="landmarkRadius">Radius of landmark points</param>
    /// <param name="connectionThickness">Thickness of connection lines</param>
    public static void DrawPoseLandmarks(
        Mat frame,
        PoseLandmarkerResult result,
        Scalar? landmarkColor = null,
        Scalar? connectionColor = null,
        int landmarkRadius = 3,
        int connectionThickness = 2)
    {
        Console.WriteLine($"[DrawPoseLandmarks] Starting drawing. Frame size: {frame.Width}x{frame.Height}");

        if (result.PoseLandmarks == null)
        {
            Console.WriteLine("[DrawPoseLandmarks] ERROR: result.PoseLandmarks is null");
            return;
        }

        if (result.PoseLandmarks.Count == 0)
        {
            Console.WriteLine("[DrawPoseLandmarks] WARNING: result.PoseLandmarks.Count is 0");
            return;
        }

        Scalar lmColor = landmarkColor ?? new Scalar(0, 255, 255); // Yellow for landmarks
        Scalar connColor = connectionColor ?? new Scalar(0, 255, 0); // Green for connections

        Console.WriteLine($"[DrawPoseLandmarks] Processing {result.PoseLandmarks.Count} pose(s)");

        for (int poseIndex = 0; poseIndex < result.PoseLandmarks.Count; poseIndex++)
        {
            NormalizedLandmarks poseLandmarks = result.PoseLandmarks[poseIndex];
            Console.WriteLine(
                $"[DrawPoseLandmarks] Processing pose {poseIndex} with {poseLandmarks.landmarks.Count} landmarks");

            // Draw connections first (so they appear behind landmarks)
            DrawConnections(frame, poseLandmarks, connColor, connectionThickness);

            // Draw landmarks on top
            DrawLandmarks(frame, poseLandmarks, lmColor, landmarkRadius, poseIndex);

            // Draw pose confidence if available
            DrawPoseConfidence(frame, poseLandmarks, poseIndex);
        }

        // Draw summary
        DrawPoseSummary(frame, result);

        Console.WriteLine("[DrawPoseLandmarks] Drawing completed successfully");
    }

    /// <summary>
    ///     Draws landmark points for a pose
    /// </summary>
    private static void DrawLandmarks(Mat frame, NormalizedLandmarks poseLandmarks, Scalar color, int radius,
        int poseIndex)
    {
        Console.WriteLine($"[DrawLandmarks] Drawing {poseLandmarks.landmarks.Count} landmarks for pose {poseIndex}");
        int drawnCount = 0;
        int skippedCount = 0;

        for (int i = 0; i < poseLandmarks.landmarks.Count; i++)
        {
            NormalizedLandmark landmark = poseLandmarks.landmarks[i];

            // Only draw if landmark is visible enough
            bool shouldDraw = true;
            if (landmark.Visibility.HasValue) shouldDraw = landmark.Visibility.Value > 0.3f;

            if (landmark.Presence.HasValue && landmark.Presence.Value < 0.3f) shouldDraw = false;

            if (shouldDraw)
            {
                int x = (int)(landmark.X * frame.Width);
                int y = (int)(landmark.Y * frame.Height);

                // Ensure coordinates are within frame bounds
                x = Math.Max(0, Math.Min(x, frame.Width - 1));
                y = Math.Max(0, Math.Min(y, frame.Height - 1));

                Point point = new(x, y);

                // Use different colors for different body parts
                Scalar landmarkColor = GetLandmarkColor(i);

                Cv2.Circle(frame, point, radius, landmarkColor, -1);

                // Draw landmark number for debugging (optional)
                if (i < LandmarkNames.Length)
                    Cv2.PutText(frame, i.ToString(), new Point(x + radius + 2, y - radius),
                        HersheyFonts.HersheySimplex, 0.3, new Scalar(255, 255, 255));

                drawnCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        Console.WriteLine($"[DrawLandmarks] Drew {drawnCount} landmarks, skipped {skippedCount} due to low visibility");
    }

    /// <summary>
    ///     Draws connections between landmarks
    /// </summary>
    private static void DrawConnections(Mat frame, NormalizedLandmarks poseLandmarks, Scalar color, int thickness)
    {
        Console.WriteLine($"[DrawConnections] Drawing {PoseConnections.Length} connections");
        int drawnConnections = 0;
        int skippedConnections = 0;

        foreach (int[] connection in PoseConnections)
            if (connection.Length == 2 &&
                connection[0] < poseLandmarks.landmarks.Count &&
                connection[1] < poseLandmarks.landmarks.Count)
            {
                NormalizedLandmark startLandmark = poseLandmarks.landmarks[connection[0]];
                NormalizedLandmark endLandmark = poseLandmarks.landmarks[connection[1]];

                // Only draw if both landmarks are visible enough
                bool startVisible = true;
                bool endVisible = true;

                if (startLandmark.Visibility.HasValue) startVisible = startLandmark.Visibility.Value > 0.3f;
                if (startLandmark.Presence.HasValue) startVisible = startVisible && startLandmark.Presence.Value > 0.3f;

                if (endLandmark.Visibility.HasValue) endVisible = endLandmark.Visibility.Value > 0.3f;
                if (endLandmark.Presence.HasValue) endVisible = endVisible && endLandmark.Presence.Value > 0.3f;

                if (startVisible && endVisible)
                {
                    Point startPoint = new(
                        (int)(startLandmark.X * frame.Width),
                        (int)(startLandmark.Y * frame.Height)
                    );

                    Point endPoint = new(
                        (int)(endLandmark.X * frame.Width),
                        (int)(endLandmark.Y * frame.Height)
                    );

                    // Use different colors for different body parts
                    Scalar connectionColor = GetConnectionColor(connection[0]);

                    Cv2.Line(frame, startPoint, endPoint, connectionColor, thickness);
                    drawnConnections++;
                }
                else
                {
                    skippedConnections++;
                }
            }

        Console.WriteLine($"[DrawConnections] Drew {drawnConnections} connections, skipped {skippedConnections}");
    }

    /// <summary>
    ///     Gets color for a specific landmark based on body part
    /// </summary>
    private static Scalar GetLandmarkColor(int landmarkIndex)
    {
        if (landmarkIndex >= 0 && landmarkIndex <= 10) // Face
            return BodyPartColors[0];
        if (landmarkIndex >= 11 && landmarkIndex <= 22) // Arms and hands
            return landmarkIndex % 2 == 1 ? BodyPartColors[2] : BodyPartColors[3]; // Right vs Left
        if (landmarkIndex >= 23 && landmarkIndex <= 32) // Legs and feet
            return landmarkIndex % 2 == 1 ? BodyPartColors[4] : BodyPartColors[5]; // Right vs Left
        return BodyPartColors[1]; // Torso default
    }

    /// <summary>
    ///     Gets color for a connection based on body part
    /// </summary>
    private static Scalar GetConnectionColor(int landmarkIndex)
    {
        return GetLandmarkColor(landmarkIndex);
    }

    /// <summary>
    ///     Draws confidence score for a pose
    /// </summary>
    private static void DrawPoseConfidence(Mat frame, NormalizedLandmarks poseLandmarks, int poseIndex)
    {
        try
        {
            // Calculate average visibility as confidence score
            float totalVisibility = 0;
            int visibleCount = 0;

            foreach (NormalizedLandmark landmark in poseLandmarks.landmarks)
                if (landmark.Visibility.HasValue)
                {
                    totalVisibility += landmark.Visibility.Value;
                    visibleCount++;
                }

            if (visibleCount > 0)
            {
                float avgVisibility = totalVisibility / visibleCount;

                // Use nose position for text placement
                if (poseLandmarks.landmarks.Count > 0)
                {
                    NormalizedLandmark nose = poseLandmarks.landmarks[0];
                    int x = (int)(nose.X * frame.Width);
                    int y = (int)(nose.Y * frame.Height) - 30; // Above the head

                    // Ensure coordinates are within frame bounds
                    x = Math.Max(10, Math.Min(x, frame.Width - 100));
                    y = Math.Max(10, Math.Min(y, frame.Height - 1));

                    string confidenceText = $"Pose {poseIndex + 1}: {avgVisibility:F2}";
                    Cv2.PutText(frame, confidenceText, new Point(x, y),
                        HersheyFonts.HersheySimplex, 0.6, new Scalar(255, 255, 255), 2);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawPoseConfidence] Error drawing confidence: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws a pose detection summary on the frame
    /// </summary>
    public static void DrawPoseSummary(Mat frame, PoseLandmarkerResult result, Scalar? textColor = null)
    {
        try
        {
            Scalar color = textColor ?? new Scalar(0, 255, 0);
            string summary = $"Poses detected: {result.PoseLandmarks?.Count ?? 0}";

            Cv2.PutText(frame, summary, new Point(10, frame.Height - 20),
                HersheyFonts.HersheySimplex, 0.7, color, 2);

            Console.WriteLine($"[DrawPoseSummary] Drew summary: {summary}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawPoseSummary] Error drawing summary: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws bounding box around detected pose
    /// </summary>
    public static void DrawPoseBoundingBox(Mat frame, NormalizedLandmarks poseLandmarks, Scalar color,
        int thickness = 2)
    {
        if (poseLandmarks.landmarks.Count < 5)
            return;

        // Calculate bounding box from landmarks
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (NormalizedLandmark landmark in poseLandmarks.landmarks)
        {
            bool isVisible = true;
            if (landmark.Visibility.HasValue) isVisible = landmark.Visibility.Value > 0.3f;

            if (isVisible)
            {
                minX = Math.Min(minX, landmark.X);
                minY = Math.Min(minY, landmark.Y);
                maxX = Math.Max(maxX, landmark.X);
                maxY = Math.Max(maxY, landmark.Y);
            }
        }

        if (minX != float.MaxValue)
        {
            Point topLeft = new((int)(minX * frame.Width), (int)(minY * frame.Height));
            Point bottomRight = new((int)(maxX * frame.Width), (int)(maxY * frame.Height));

            Cv2.Rectangle(frame, topLeft, bottomRight, color, thickness);
        }
    }
}