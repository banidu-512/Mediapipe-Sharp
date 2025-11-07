using Mediapipe.Tasks.Vision.HandLandmarker;
using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using Mediapipe.Tasks.Components.Containers;
using Point = OpenCvSharp.Point;

namespace HandMeshApp
{
    /// <summary>
    /// Utility class for visualizing hand mesh on OpenCV Mat
    /// </summary>
    public static class HandMeshVisualizer
    {
        // Hand mesh connections for drawing mesh structure
        private static readonly int[][] HandMeshConnections = new int[][]
        {
            // Wrist
            new[] { 0, 1 }, new[] { 1, 2 }, new[] { 2, 3 }, new[] { 3, 4 }, // Thumb
            new[] { 0, 5 }, new[] { 5, 6 }, new[] { 6, 7 }, new[] { 7, 8 }, // Index finger
            new[] { 5, 9 }, new[] { 9, 10 }, new[] { 10, 11 }, new[] { 11, 12 }, // Middle finger
            new[] { 9, 13 }, new[] { 13, 14 }, new[] { 14, 15 }, new[] { 15, 16 }, // Ring finger
            new[] { 13, 17 }, new[] { 17, 18 }, new[] { 18, 19 }, new[] { 19, 20 }, // Pinky finger
            new[] { 0, 17 } // Palm connection
        };

        /// <summary>
        /// Draws hand mesh on provided Mat frame
        /// </summary>
        /// <param name="frame">The frame to draw on</param>
        /// <param name="result">Hand landmark detection result</param>
        /// <param name="landmarkColor">Color for landmark points</param>
        /// <param name="connectionColor">Color for connection lines</param>
        /// <param name="landmarkRadius">Radius of landmark points</param>
        /// <param name="connectionThickness">Thickness of connection lines</param>
        public static void DrawHandMesh(
            Mat frame, 
            HandLandmarkerResult result, 
            Scalar? landmarkColor = null, 
            Scalar? connectionColor = null, 
            int landmarkRadius = 2, 
            int connectionThickness = 1)
        {
            Console.WriteLine($"[DrawHandMesh] Starting drawing. Frame size: {frame.Width}x{frame.Height}");
            
            if (result.HandLandmarks == null)
            {
                Console.WriteLine("[DrawHandMesh] ERROR: result.HandLandmarks is null");
                return;
            }
            
            if (result.HandLandmarks.Count == 0)
            {
                Console.WriteLine("[DrawHandMesh] WARNING: result.HandLandmarks.Count is 0");
                return;
            }

            var lmColor = landmarkColor ?? new Scalar(0, 255, 0); // Green for landmarks
            var connColor = connectionColor ?? new Scalar(0, 0, 255); // Blue for connections
            
            // Use more visible defaults if not specified
            if (landmarkRadius == 2) // Default radius
                landmarkRadius = 3; // Increased from 2 to 3 for better visibility
            if (connectionThickness == 1) // Default thickness
                connectionThickness = 2; // Increased from 1 to 2 for better visibility
            
            Console.WriteLine($"[DrawHandMesh] Colors - Landmarks: {lmColor}, Connections: {connColor}");
            Console.WriteLine($"[DrawHandMesh] Processing {result.HandLandmarks.Count} hand landmark sets");

            foreach (var handLandmarks in result.HandLandmarks)
            {
                Console.WriteLine($"[DrawHandMesh] Processing hand with {handLandmarks.landmarks.Count} landmarks");
                
                // Draw connections first (so they appear behind landmarks)
                Console.WriteLine("[DrawHandMesh] Drawing connections...");
                DrawConnections(frame, handLandmarks, connColor, connectionThickness);
                Console.WriteLine("[DrawHandMesh] Connections drawing completed");
                
                // Draw landmarks on top
                Console.WriteLine("[DrawHandMesh] Drawing landmarks...");
                DrawLandmarks(frame, handLandmarks, lmColor, landmarkRadius);
                Console.WriteLine("[DrawHandMesh] Landmarks drawing completed");
            }
            
            Console.WriteLine("[DrawHandMesh] Drawing completed successfully");
        }

        /// <summary>
        /// Draws landmark points
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
                    shouldDraw = landmark.Visibility.Value > 0.1f; // Lowered threshold from 0.3f to 0.1f
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
            Console.WriteLine($"[DrawConnections] Drawing {HandMeshConnections.Length} connections with thickness {thickness}");
            int drawnConnections = 0;
            int skippedConnections = 0;
            int invalidConnections = 0;
            
            foreach (var connection in HandMeshConnections)
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
                        startVisible = startLandmark.Visibility.Value > 0.1f; // Lowered threshold from 0.3f to 0.1f
                    }
                    // If no visibility data, assume landmark is visible
                    
                    if (endLandmark.Visibility.HasValue)
                    {
                        endVisible = endLandmark.Visibility.Value > 0.1f; // Lowered threshold from 0.3f to 0.1f
                    }
                    // If no visibility data, assume landmark is visible
                    
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
        /// Draws a simple hand bounding box around detected hand
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
                    isVisible = landmark.Visibility.Value > 0.1f; // Lowered threshold from 0.3f to 0.1f
                }
                // If no visibility data, assume landmark is visible
                
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
    }
}
