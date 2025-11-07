using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using Point = OpenCvSharp.Point;

namespace GestureRecognitionApp
{
    /// <summary>
    /// Utility class for visualizing hand landmarks and gesture recognition results on OpenCV Mat
    /// </summary>
    public static class GestureRecognitionVisualizer
    {
        // Hand mesh connections for drawing hand structure
        private static readonly int[][] HandConnections = new int[][]
        {
            // Thumb
            new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 4 },
            // Index finger
            new[] { 0, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 8 },
            // Middle finger
            new[] { 0, 9 }, new[] { 9, 10 }, new[] { 10, 11 }, new[] { 11, 12 },
            // Ring finger
            new[] { 0, 13 }, new[] { 13, 14 }, new[] { 14, 15 }, new[] { 15, 16 },
            // Pinky
            new[] { 0, 17 }, new[] { 17, 18 }, new[] { 18, 19 }, new[] { 19, 20 },
            // Palm connections
            new[] { 5, 9 }, new[] { 9, 13 }, new[] { 13, 17 }
        };

        // Color palette for different gesture types
        private static readonly Dictionary<GestureRecognizer.GestureType, Scalar> GestureColors = new()
        {
            { GestureRecognizer.GestureType.Unknown, new Scalar(128, 128, 128) },      // Gray
            { GestureRecognizer.GestureType.ThumbsUp, new Scalar(0, 255, 0) },        // Green
            { GestureRecognizer.GestureType.ThumbsDown, new Scalar(0, 0, 255) },      // Red
            { GestureRecognizer.GestureType.OK, new Scalar(255, 255, 0) },            // Cyan
            { GestureRecognizer.GestureType.Point, new Scalar(255, 165, 0) },          // Orange
            { GestureRecognizer.GestureType.Fist, new Scalar(255, 0, 255) },          // Magenta
            { GestureRecognizer.GestureType.OpenPalm, new Scalar(0, 255, 255) },       // Yellow
            { GestureRecognizer.GestureType.Victory, new Scalar(255, 0, 127) },        // Pink
            { GestureRecognizer.GestureType.One, new Scalar(255, 215, 0) },            // Gold
            { GestureRecognizer.GestureType.Two, new Scalar(0, 255, 127) },            // Spring Green
            { GestureRecognizer.GestureType.Three, new Scalar(255, 105, 180) },       // Light Pink
            { GestureRecognizer.GestureType.Four, new Scalar(138, 43, 226) },         // Blue Violet
            { GestureRecognizer.GestureType.Five, new Scalar(255, 215, 255) },         // Light Yellow
            { GestureRecognizer.GestureType.Rock, new Scalar(139, 69, 19) },           // Saddle Brown
            { GestureRecognizer.GestureType.Paper, new Scalar(255, 255, 255) },         // White
            { GestureRecognizer.GestureType.Scissors, new Scalar(0, 191, 255) },       // Deep Sky Blue
            { GestureRecognizer.GestureType.CallMe, new Scalar(255, 20, 147) },         // Deep Pink
            { GestureRecognizer.GestureType.Shaka, new Scalar(0, 255, 127) },           // Spring Green
            { GestureRecognizer.GestureType.Heart, new Scalar(255, 105, 180) },          // Light Pink
            { GestureRecognizer.GestureType.ComeHere, new Scalar(255, 165, 0) },       // Orange
            { GestureRecognizer.GestureType.GoAway, new Scalar(255, 69, 0) },         // Red Orange
            { GestureRecognizer.GestureType.StopHand, new Scalar(255, 0, 0) }           // Red
        };

        /// <summary>
        /// Draws hand landmarks and connections on provided Mat frame
        /// </summary>
        /// <param name="frame">The frame to draw on</param>
        /// <param name="result">Hand landmark detection result</param>
        /// <param name="gestureResults">Gesture recognition results for each hand</param>
        /// <param name="landmarkColor">Color for landmark points</param>
        /// <param name="connectionColor">Color for connection lines</param>
        /// <param name="landmarkRadius">Radius of landmark points</param>
        /// <param name="connectionThickness">Thickness of connection lines</param>
        /// <param name="showLabels">Whether to show gesture labels</param>
        public static void DrawHandGestures(
            Mat frame, 
            HandLandmarkerResult result, 
            List<GestureRecognizer.GestureResult>? gestureResults = null,
            Scalar? landmarkColor = null, 
            Scalar? connectionColor = null, 
            int landmarkRadius = 3, 
            int connectionThickness = 2,
            bool showLabels = true)
        {
            Console.WriteLine($"[DrawHandGestures] Starting drawing. Frame size: {frame.Width}x{frame.Height}");
            
            if (result.HandLandmarks == null)
            {
                Console.WriteLine("[DrawHandGestures] ERROR: result.HandLandmarks is null");
                return;
            }
            
            if (result.HandLandmarks.Count == 0)
            {
                Console.WriteLine("[DrawHandGestures] WARNING: result.HandLandmarks.Count is 0");
                return;
            }

            var lmColor = landmarkColor ?? new Scalar(0, 255, 0); // Green for landmarks
            var connColor = connectionColor ?? new Scalar(0, 0, 255); // Blue for connections
            
            Console.WriteLine($"[DrawHandGestures] Processing {result.HandLandmarks.Count} hand landmark sets");

            // Draw each hand with its gesture result
            for (int handIndex = 0; handIndex < result.HandLandmarks.Count; handIndex++)
            {
                var handLandmarks = result.HandLandmarks[handIndex];
                var gestureResult = (gestureResults != null && gestureResults.Count > handIndex) ? gestureResults[handIndex] : null;
                
                Console.WriteLine($"[DrawHandGestures] Processing hand {handIndex} with {handLandmarks.landmarks.Count} landmarks");
                
                // Get gesture-specific colors
                Scalar gestureLandmarkColor = lmColor;
                Scalar gestureConnectionColor = connColor;
                
                if (gestureResult != null && GestureColors.ContainsKey(gestureResult.Gesture))
                {
                    gestureLandmarkColor = GestureColors[gestureResult.Gesture];
                    gestureConnectionColor = GestureColors[gestureResult.Gesture];
                }
                
                // Draw connections first (so they appear behind landmarks)
                Console.WriteLine($"[DrawHandGestures] Drawing connections for hand {handIndex}...");
                DrawConnections(frame, handLandmarks, gestureConnectionColor, connectionThickness);
                
                // Draw landmarks on top
                Console.WriteLine($"[DrawHandGestures] Drawing landmarks for hand {handIndex}...");
                DrawLandmarks(frame, handLandmarks, gestureLandmarkColor, landmarkRadius);
                
                // Draw gesture label if available
                if (gestureResult != null && showLabels)
                {
                    Console.WriteLine($"[DrawHandGestures] Drawing gesture label: {gestureResult.Description}");
                    DrawGestureLabel(frame, handLandmarks, gestureResult);
                }
            }
            
            Console.WriteLine("[DrawHandGestures] Drawing completed successfully");
        }

        /// <summary>
        /// Draws landmark points for a hand
        /// </summary>
        private static void DrawLandmarks(Mat frame, NormalizedLandmarks handLandmarks, Scalar color, int radius)
        {
            Console.WriteLine($"[DrawLandmarks] Drawing {handLandmarks.landmarks.Count} landmarks with radius {radius}");
            int drawnCount = 0;
            int skippedCount = 0;
            
            foreach (var landmark in handLandmarks.landmarks)
            {
                var x = (int)(landmark.X * frame.Width);
                var y = (int)(landmark.Y * frame.Height);
                
                // Only draw if landmark is visible enough
                bool shouldDraw = true;
                if (landmark.Visibility.HasValue)
                {
                    shouldDraw = landmark.Visibility.Value > 0.1f;
                }
                // If no visibility data, assume landmark is visible
                
                if (shouldDraw)
                {
                    Cv2.Circle(frame, new Point(x, y), radius, color, -1);
                    drawnCount++;
                }
                else
                {
                    skippedCount++;
                    if (skippedCount <= 5) // Log first 5 skipped landmarks for debugging
                    {
                        Console.WriteLine($"[DrawLandmarks] Skipped landmark: X={landmark.X:F3}, Y={landmark.Y:F3}, Visibility={landmark.Visibility}");
                    }
                }
            }
            
            Console.WriteLine($"[DrawLandmarks] Drew {drawnCount} landmarks, skipped {skippedCount} due to low visibility");
        }

        /// <summary>
        /// Draws connections between landmarks
        /// </summary>
        private static void DrawConnections(Mat frame, NormalizedLandmarks handLandmarks, Scalar color, int thickness)
        {
            Console.WriteLine($"[DrawConnections] Drawing {HandConnections.Length} connections with thickness {thickness}");
            int drawnConnections = 0;
            int skippedConnections = 0;
            int invalidConnections = 0;
            
            foreach (var connection in HandConnections)
            {
                if (connection.Length == 2 &&
                    connection[0] < handLandmarks.landmarks.Count &&
                    connection[1] < handLandmarks.landmarks.Count)
                {
                    var startLandmark = handLandmarks.landmarks[connection[0]];
                    var endLandmark = handLandmarks.landmarks[connection[1]];
                    
                    // Only draw if both landmarks are visible enough
                    bool startVisible = true;
                    bool endVisible = true;
                    
                    if (startLandmark.Visibility.HasValue)
                    {
                        startVisible = startLandmark.Visibility.Value > 0.1f;
                    }
                    
                    if (endLandmark.Visibility.HasValue)
                    {
                        endVisible = endLandmark.Visibility.Value > 0.1f;
                    }
                    
                    if (startVisible && endVisible)
                    {
                        var startPoint = new Point(
                            (int)(startLandmark.X * frame.Width),
                            (int)(startLandmark.Y * frame.Height)
                        );
                        
                        var endPoint = new Point(
                            (int)(endLandmark.X * frame.Width),
                            (int)(endLandmark.Y * frame.Height)
                        );
                        
                        Cv2.Line(frame, startPoint, endPoint, color, thickness);
                        drawnConnections++;
                    }
                    else
                    {
                        skippedConnections++;
                        if (skippedConnections <= 5) // Log first 5 skipped connections
                        {
                            Console.WriteLine($"[DrawConnections] Skipped connection [{connection[0]}, {connection[1]}]: " +
                                          $"Start vis={startLandmark.Visibility}, End vis={endLandmark.Visibility}");
                        }
                    }
                }
                else
                {
                    invalidConnections++;
                    if (invalidConnections <= 3) // Log first 3 invalid connections
                    {
                        Console.WriteLine($"[DrawConnections] Invalid connection: [{string.Join(", ", connection)}], " +
                                      $"landmarks count: {handLandmarks.landmarks.Count}");
                    }
                }
            }
            
            Console.WriteLine($"[DrawConnections] Drew {drawnConnections} connections, " +
                          $"skipped {skippedConnections} due to low visibility, " +
                          $"{invalidConnections} invalid connections");
        }

        /// <summary>
        /// Draws gesture label and confidence score
        /// </summary>
        private static void DrawGestureLabel(Mat frame, NormalizedLandmarks handLandmarks, GestureRecognizer.GestureResult gestureResult)
        {
            try
            {
                // Calculate label position (top of hand)
                float minY = float.MaxValue;
                float centerX = 0;
                int visibleCount = 0;
                
                foreach (var landmark in handLandmarks.landmarks)
                {
                    bool isVisible = true;
                    if (landmark.Visibility.HasValue)
                    {
                        isVisible = landmark.Visibility.Value > 0.1f;
                    }
                    
                    if (isVisible)
                    {
                        minY = Math.Min(minY, landmark.Y);
                        centerX += landmark.X;
                        visibleCount++;
                    }
                }
                
                if (visibleCount > 0)
                {
                    centerX /= visibleCount;
                    
                    // Convert to pixel coordinates
                    int labelX = (int)(centerX * frame.Width);
                    int labelY = (int)(minY * frame.Height) - 30; // Position above hand
                    
                    // Ensure coordinates are within frame bounds
                    labelX = Math.Max(10, Math.Min(labelX, frame.Width - 200));
                    labelY = Math.Max(30, Math.Min(labelY, frame.Height - 50));
                    
                    // Get gesture color
                    var gestureColor = GestureColors.ContainsKey(gestureResult.Gesture) 
                        ? GestureColors[gestureResult.Gesture] 
                        : new Scalar(255, 255, 255);
                    
                    // Prepare label text
                    string labelText = $"{gestureResult.Description}";
                    if (gestureResult.Confidence > 0)
                    {
                        labelText += $" ({gestureResult.Confidence:F2})";
                    }
                    
                    if (gestureResult.IsLeftHand)
                    {
                        labelText += " (L)";
                    }
                    else
                    {
                        labelText += " (R)";
                    }
                    
                    // Draw background rectangle for better visibility
                    var textSize = Cv2.GetTextSize(labelText, HersheyFonts.HersheySimplex, 0.6, 1, out _);
                    var rect = new OpenCvSharp.Rect(
                        labelX - 5,
                        labelY - textSize.Height - 5,
                        textSize.Width + 10,
                        textSize.Height + 10
                    );
                    
                    Cv2.Rectangle(frame, rect, new Scalar(0, 0, 0), -1, LineTypes.Link8, 0); // Black background
                    Cv2.Rectangle(frame, rect, gestureColor, 2, LineTypes.Link8, 0); // Colored border
                    
                    // Draw text
                    Cv2.PutText(frame, labelText, new Point(labelX, labelY), 
                               HersheyFonts.HersheySimplex, 0.6, gestureColor, 1);
                    
                    Console.WriteLine($"[DrawGestureLabel] Drew label: {labelText} at ({labelX}, {labelY})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawGestureLabel] Error drawing gesture label: {ex.Message}");
            }
        }

        /// <summary>
        /// Draws a bounding box around the detected hand
        /// </summary>
        public static void DrawHandBoundingBox(Mat frame, NormalizedLandmarks handLandmarks, Scalar color, int thickness = 2)
        {
            if (handLandmarks.landmarks.Count < 5)
                return;

            // Calculate bounding box from landmarks
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var landmark in handLandmarks.landmarks)
            {
                bool isVisible = true;
                if (landmark.Visibility.HasValue)
                {
                    isVisible = landmark.Visibility.Value > 0.1f;
                }
                
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
                var topLeft = new Point((int)(minX * frame.Width), (int)(minY * frame.Height));
                var bottomRight = new Point((int)(maxX * frame.Width), (int)(maxY * frame.Height));
                
                Cv2.Rectangle(frame, topLeft, bottomRight, color, thickness);
            }
        }

        /// <summary>
        /// Draws summary information on the frame
        /// </summary>
        public static void DrawGestureSummary(Mat frame, List<GestureRecognizer.GestureResult>? gestureResults, Scalar? textColor = null)
        {
            try
            {
                var color = textColor ?? new Scalar(0, 255, 0);
                
                if (gestureResults == null || gestureResults.Count == 0)
                {
                    string summary = "No gestures detected";
                    Cv2.PutText(frame, summary, new Point(10, frame.Height - 20), 
                               HersheyFonts.HersheySimplex, 0.7, color, 2);
                    Console.WriteLine($"[DrawGestureSummary] Drew summary: {summary}");
                    return;
                }
                
                // Count gesture types
                var gestureCounts = new Dictionary<GestureRecognizer.GestureType, int>();
                foreach (var result in gestureResults)
                {
                    if (result.Gesture != GestureRecognizer.GestureType.Unknown)
                    {
                        gestureCounts[result.Gesture] = gestureCounts.GetValueOrDefault(result.Gesture, 0) + 1;
                    }
                }
                
                // Build summary text
                var summaryLines = new List<string>();
                summaryLines.Add($"Hands detected: {gestureResults.Count}");
                
                foreach (var kvp in gestureCounts.OrderByDescending(x => x.Value))
                {
                    var gestureName = kvp.Key.ToString();
                    if (kvp.Value > 1)
                    {
                        gestureName += $" (x{kvp.Value})";
                    }
                    summaryLines.Add(gestureName);
                }
                
                // Draw summary background
                int lineHeight = 25;
                int totalHeight = summaryLines.Count * lineHeight + 10;
                var summaryRect = new OpenCvSharp.Rect(10, frame.Height - totalHeight - 10, 300, totalHeight);
                
                Cv2.Rectangle(frame, summaryRect, new Scalar(0, 0, 0), -1, LineTypes.Link8, 0); // Black background
                Cv2.Rectangle(frame, summaryRect, color, 1, LineTypes.Link8, 0); // Border
                
                // Draw text lines
                for (int i = 0; i < summaryLines.Count; i++)
                {
                    var textPoint = new Point(15, frame.Height - totalHeight + 5 + (i + 1) * lineHeight);
                    Cv2.PutText(frame, summaryLines[i], textPoint, 
                               HersheyFonts.HersheySimplex, 0.5, color, 1);
                }
                
                Console.WriteLine($"[DrawGestureSummary] Drew summary with {summaryLines.Count} lines");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawGestureSummary] Error drawing summary: {ex.Message}");
            }
        }

        /// <summary>
        /// Draws performance and status information
        /// </summary>
        public static void DrawStatusInfo(Mat frame, string status, bool demoMode, Scalar? textColor = null)
        {
            try
            {
                var color = textColor ?? new Scalar(0, 255, 0);
                
                // Status text at top
                string modeText = demoMode ? "Demo Mode: Gesture recognition unavailable" : "Gesture Recognition Active";
                Cv2.PutText(frame, modeText, new Point(10, 30), 
                           HersheyFonts.HersheySimplex, 0.7, 
                           demoMode ? new Scalar(0, 0, 255) : color, 2);
                
                // Additional status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    Cv2.PutText(frame, status, new Point(10, 60), 
                               HersheyFonts.HersheySimplex, 0.6, color, 1);
                }
                
                // FPS indicator (simplified)
                Cv2.PutText(frame, "~30 FPS", new Point(frame.Width - 100, 30), 
                           HersheyFonts.HersheySimplex, 0.5, color, 1);
                
                Console.WriteLine($"[DrawStatusInfo] Drew status: {modeText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawStatusInfo] Error drawing status: {ex.Message}");
            }
        }
    }
}
