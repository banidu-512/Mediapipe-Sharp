using Mediapipe.Tasks.Vision.FaceLandmarker;
using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using Mediapipe.Tasks.Components.Containers;
using Point = OpenCvSharp.Point;

namespace FaceMeshApp
{
    /// <summary>
    /// Utility class for visualizing face mesh on OpenCV Mat
    /// </summary>
    public static class FaceMeshVisualizer
    {
        // Face mesh connections for drawing mesh structure
        private static readonly int[][] FaceMeshConnections = new int[][]
        {
            // Lips
            new[] { 61, 146 }, new[] { 146, 91 }, new[] { 91, 181 }, new[] { 181, 84 }, new[] { 84, 17 },
            new[] { 17, 314 }, new[] { 314, 405 }, new[] { 405, 320 }, new[] { 320, 307 }, new[] { 307, 375 },
            new[] { 375, 321 }, new[] { 321, 308 }, new[] { 308, 324 }, new[] { 324, 318 }, new[] { 318, 402 },
            new[] { 402, 317 }, new[] { 317, 14 }, new[] { 14, 87 }, new[] { 87, 178 }, new[] { 178, 88 }, new[] { 88, 95 },
            new[] { 95, 78 }, new[] { 78, 191 }, new[] { 191, 80 }, new[] { 80, 81 }, new[] { 81, 82 }, new[] { 82, 13 }, new[] { 13, 312 },
            new[] { 312, 311 }, new[] { 311, 310 }, new[] { 310, 415 }, new[] { 415, 308 },
            
            // Left eye
            new[] { 33, 7 }, new[] { 7, 163 }, new[] { 163, 144 }, new[] { 144, 145 }, new[] { 145, 153 }, new[] { 153, 154 },
            new[] { 154, 155 }, new[] { 155, 133 }, new[] { 133, 173 }, new[] { 173, 157 }, new[] { 157, 158 }, new[] { 158, 159 },
            new[] { 159, 160 }, new[] { 160, 161 }, new[] { 161, 246 }, new[] { 246, 33 },
            
            // Right eye
            new[] { 362, 398 }, new[] { 398, 384 }, new[] { 384, 385 }, new[] { 385, 386 }, new[] { 386, 387 }, new[] { 387, 388 },
            new[] { 388, 466 }, new[] { 466, 263 }, new[] { 263, 249 }, new[] { 249, 390 }, new[] { 390, 373 }, new[] { 373, 374 },
            new[] { 374, 380 }, new[] { 380, 381 }, new[] { 381, 382 }, new[] { 382, 362 },
            
            // Left eyebrow
            new[] { 46, 53 }, new[] { 53, 52 }, new[] { 52, 65 }, new[] { 65, 55 }, new[] { 55, 70 }, new[] { 70, 63 }, new[] { 63, 105 },
            new[] { 105, 66 }, new[] { 66, 107 }, new[] { 107, 55 }, new[] { 55, 65 }, new[] { 65, 52 }, new[] { 52, 53 },
            
            // Right eyebrow
            new[] { 276, 283 }, new[] { 283, 282 }, new[] { 282, 295 }, new[] { 295, 285 }, new[] { 285, 300 }, new[] { 300, 293 }, new[] { 293, 334 },
            new[] { 334, 293 }, new[] { 293, 300 }, new[] { 300, 285 }, new[] { 285, 295 }, new[] { 295, 282 }, new[] { 282, 283 },
            
            // Face oval
            new[] { 10, 338 }, new[] { 338, 297 }, new[] { 297, 332 }, new[] { 332, 284 }, new[] { 284, 251 }, new[] { 251, 389 },
            new[] { 389, 356 }, new[] { 356, 454 }, new[] { 454, 323 }, new[] { 323, 361 }, new[] { 361, 340 }, new[] { 340, 346 },
            new[] { 346, 347 }, new[] { 347, 348 }, new[] { 348, 349 }, new[] { 349, 350 }, new[] { 350, 451 }, new[] { 451, 452 }, new[] { 452, 453 },
            new[] { 453, 464 }, new[] { 464, 435 }, new[] { 435, 410 }, new[] { 410, 287 }, new[] { 287, 273 }, new[] { 273, 335 }, new[] { 335, 406 },
            new[] { 406, 313 }, new[] { 313, 18 }, new[] { 18, 83 }, new[] { 83, 182 }, new[] { 182, 106 }, new[] { 106, 43 }, new[] { 43, 57 }, new[] { 57, 186 },
            new[] { 186, 92 }, new[] { 92, 165 }, new[] { 165, 167 }, new[] { 167, 164 }, new[] { 164, 393 }, new[] { 393, 391 }, new[] { 391, 322 }, new[] { 322, 410 },
            new[] { 410, 287 }, new[] { 287, 273 }, new[] { 273, 335 }, new[] { 335, 321 }, new[] { 321, 308 }, new[] { 308, 324 }, new[] { 324, 318 }, new[] { 318, 402 },
            new[] { 402, 317 }, new[] { 317, 14 }, new[] { 14, 87 }, new[] { 87, 178 }, new[] { 178, 88 }, new[] { 88, 95 }, new[] { 95, 78 }, new[] { 78, 191 },
            new[] { 191, 80 }, new[] { 80, 81 }, new[] { 81, 82 }, new[] { 82, 13 }, new[] { 13, 312 }, new[] { 312, 311 }, new[] { 311, 310 }, new[] { 310, 415 }, new[] { 415, 308 }
        };

        /// <summary>
        /// Draws face mesh on provided Mat frame
        /// </summary>
        /// <param name="frame">The frame to draw on</param>
        /// <param name="result">Face landmark detection result</param>
        /// <param name="landmarkColor">Color for landmark points</param>
        /// <param name="connectionColor">Color for connection lines</param>
        /// <param name="landmarkRadius">Radius of landmark points</param>
        /// <param name="connectionThickness">Thickness of connection lines</param>
        public static void DrawFaceMesh(
            Mat frame, 
            FaceLandmarkerResult result, 
            Scalar? landmarkColor = null, 
            Scalar? connectionColor = null, 
            int landmarkRadius = 2, 
            int connectionThickness = 1)
        {
            Console.WriteLine($"[DrawFaceMesh] Starting drawing. Frame size: {frame.Width}x{frame.Height}");
            
            if (result.FaceLandmarks == null)
            {
                Console.WriteLine("[DrawFaceMesh] ERROR: result.FaceLandmarks is null");
                return;
            }
            
            if (result.FaceLandmarks.Count == 0)
            {
                Console.WriteLine("[DrawFaceMesh] WARNING: result.FaceLandmarks.Count is 0");
                return;
            }

            var lmColor = landmarkColor ?? new Scalar(0, 255, 0); // Green for landmarks
            var connColor = connectionColor ?? new Scalar(0, 0, 255); // Changed to Blue for better visibility (was Yellow)
            
            // Use more visible defaults if not specified
            if (landmarkRadius == 2) // Default radius
                landmarkRadius = 3; // Increased from 2 to 3 for better visibility
            if (connectionThickness == 1) // Default thickness
                connectionThickness = 2; // Increased from 1 to 2 for better visibility
            
            Console.WriteLine($"[DrawFaceMesh] Colors - Landmarks: {lmColor}, Connections: {connColor}");
            Console.WriteLine($"[DrawFaceMesh] Processing {result.FaceLandmarks.Count} face landmark sets");

            foreach (var faceLandmarks in result.FaceLandmarks)
            {
                Console.WriteLine($"[DrawFaceMesh] Processing face with {faceLandmarks.landmarks.Count} landmarks");
                
                // Draw connections first (so they appear behind landmarks)
                Console.WriteLine("[DrawFaceMesh] Drawing connections...");
                DrawConnections(frame, faceLandmarks, connColor, connectionThickness);
                Console.WriteLine("[DrawFaceMesh] Connections drawing completed");
                
                // Draw landmarks on top
                Console.WriteLine("[DrawFaceMesh] Drawing landmarks...");
                DrawLandmarks(frame, faceLandmarks, lmColor, landmarkRadius);
                Console.WriteLine("[DrawFaceMesh] Landmarks drawing completed");
            }
            
            Console.WriteLine("[DrawFaceMesh] Drawing completed successfully");
        }

        /// <summary>
        /// Draws landmark points
        /// </summary>
        private static void DrawLandmarks(Mat frame, NormalizedLandmarks faceLandmarks, Scalar color, int radius)
        {
            Console.WriteLine($"[DrawLandmarks] Drawing {faceLandmarks.landmarks.Count} landmarks with radius {radius}");
            int drawnCount = 0;
            int skippedCount = 0;
            
            foreach (var landmark in faceLandmarks.landmarks)
            {
                var x = (int)(landmark.X * frame.Width);
                var y = (int)(landmark.Y * frame.Height);
                
                // Only draw if landmark is visible enough
                // Modified: Lower visibility threshold and handle missing visibility data
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
        private static void DrawConnections(Mat frame, NormalizedLandmarks faceLandmarks, Scalar color, int thickness)
        {
            Console.WriteLine($"[DrawConnections] Drawing {FaceMeshConnections.Length} connections with thickness {thickness}");
            int drawnConnections = 0;
            int skippedConnections = 0;
            int invalidConnections = 0;
            
            foreach (var connection in FaceMeshConnections)
            {
                if (connection.Length == 2 &&
                    connection[0] < faceLandmarks.landmarks.Count &&
                    connection[1] < faceLandmarks.landmarks.Count)
                {
                    var startLandmark = faceLandmarks.landmarks[connection[0]];
                    var endLandmark = faceLandmarks.landmarks[connection[1]];
                    
                    // Only draw if both landmarks are visible enough
                    // Modified: Lower visibility threshold and handle missing visibility data
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
                                      $"landmarks count: {faceLandmarks.landmarks.Count}");
                    }
                }
            }
            
            Console.WriteLine($"[DrawConnections] Drew {drawnConnections} connections, " +
                          $"skipped {skippedConnections} due to low visibility, " +
                          $"{invalidConnections} invalid connections");
        }

        /// <summary>
        /// Draws a simple face oval around detected face
        /// </summary>
        public static void DrawFaceOval(Mat frame, NormalizedLandmarks faceLandmarks, Scalar color, int thickness = 2)
        {
            if (faceLandmarks.landmarks.Count < 10)
                return;

            // Calculate bounding box from landmarks
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var landmark in faceLandmarks.landmarks)
            {
                // Modified: Lower visibility threshold and handle missing visibility data
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