using System.Runtime.InteropServices;
using Mediapipe.Tasks.Vision.ImageSegmenter;
using OpenCvSharp;
using Image = Mediapipe.Framework.Formats.Image;
using ImageFormat = Mediapipe.ImageFormat.Types.Format;
using Point = OpenCvSharp.Point;

namespace ImageSegmentationApp;

/// <summary>
///     Utility class for visualizing image segmentation results on OpenCV Mat
/// </summary>
public static class ImageSegmentationVisualizer
{
    // Simple colors for segmentation masks
    private static readonly Scalar[] MaskColors = new[]
    {
        new Scalar(0, 0, 0), // Background - black
        new Scalar(0, 255, 0), // Person - green
        new Scalar(255, 0, 0), // Other - red
        new Scalar(0, 0, 255) // Another category - blue
    };

    /// <summary>
    ///     Draws segmentation mask on provided Mat frame
    /// </summary>
    /// <param name="frame">The frame to draw on</param>
    /// <param name="result">Image segmentation result</param>
    /// <param name="overlayAlpha">Alpha blending factor (0.0 to 1.0)</param>
    public static void DrawSegmentation(
        Mat frame,
        ImageSegmenterResult result,
        double overlayAlpha = 0.6)
    {
        Console.WriteLine($"[DrawSegmentation] Starting drawing. Frame size: {frame.Width}x{frame.Height}");

        if (result.Equals(null))
        {
            Console.WriteLine("[DrawSegmentation] ERROR: result is null");
            return;
        }

        try
        {
            // Draw category mask if available
            if (result.CategoryMask != null)
                DrawCategoryMask(frame, result.CategoryMask, overlayAlpha);
            // Otherwise try confidence masks
            else if (result.ConfidenceMasks != null && result.ConfidenceMasks.Count > 0)
                DrawConfidenceMask(frame, result.ConfidenceMasks[0], overlayAlpha);

            // Draw summary
            DrawSegmentationSummary(frame, result);

            Console.WriteLine("[DrawSegmentation] Drawing completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawSegmentation] Error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws category segmentation mask
    /// </summary>
    private static void DrawCategoryMask(Mat frame, Image categoryMask, double alpha)
    {
        try
        {
            Console.WriteLine("[DrawCategoryMask] Drawing category mask...");

            using (Mat? maskMat = ConvertImageToMat(categoryMask))
            {
                if (maskMat == null)
                {
                    Console.WriteLine("[DrawCategoryMask] ERROR: Failed to convert mask to Mat");
                    return;
                }

                using (Mat resizedMask = new())
                {
                    Cv2.Resize(maskMat, resizedMask, frame.Size(), 0, 0, InterpolationFlags.Nearest);

                    // Create colored overlay
                    using (Mat coloredMask = new(frame.Size(), MatType.CV_8UC3, new Scalar(0, 255, 0)))
                    {
                        // Apply mask to colored overlay
                        coloredMask.SetTo(new Scalar(0, 255, 0), resizedMask);

                        // Blend with original frame
                        Cv2.AddWeighted(frame, 1.0 - alpha, coloredMask, alpha, 0, frame);

                        Console.WriteLine("[DrawCategoryMask] Category mask applied successfully");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawCategoryMask] Error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws confidence segmentation mask
    /// </summary>
    private static void DrawConfidenceMask(Mat frame, Image confidenceMask, double alpha)
    {
        try
        {
            Console.WriteLine("[DrawConfidenceMask] Drawing confidence mask...");

            using (Mat? maskMat = ConvertImageToMat(confidenceMask))
            {
                if (maskMat == null)
                {
                    Console.WriteLine("[DrawConfidenceMask] ERROR: Failed to convert mask to Mat");
                    return;
                }

                using (Mat resizedMask = new())
                {
                    Cv2.Resize(maskMat, resizedMask, frame.Size());

                    // Create colored overlay - green for segmented regions
                    using (Mat coloredMask = new(frame.Size(), MatType.CV_8UC3, new Scalar(0, 255, 0)))
                    {
                        // Apply mask with threshold
                        using (Mat binaryMask = new())
                        {
                            Cv2.Threshold(resizedMask, binaryMask, 0.5, 255, ThresholdTypes.Binary);
                            coloredMask.SetTo(new Scalar(0, 255, 0), binaryMask);

                            // Blend with original frame
                            Cv2.AddWeighted(frame, 1.0 - alpha, coloredMask, alpha, 0, frame);

                            Console.WriteLine("[DrawConfidenceMask] Confidence mask applied successfully");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawConfidenceMask] Error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Draws segmentation summary on the frame
    /// </summary>
    private static void DrawSegmentationSummary(Mat frame, ImageSegmenterResult result, Scalar? textColor = null)
    {
        try
        {
            Scalar color = textColor ?? new Scalar(0, 255, 0);
            string summary = "Segmentation: Active";

            Cv2.PutText(frame, summary, new Point(10, frame.Height - 20),
                HersheyFonts.HersheySimplex, 0.7, color, 2);

            Console.WriteLine($"[DrawSegmentationSummary] Drew summary: {summary}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DrawSegmentationSummary] Error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Converts MediaPipe Image to OpenCV Mat
    /// </summary>
    private static Mat ConvertImageToMat(Image image)
    {
        try
        {
            Func<ImageFormat> format = image.ImageFormat;
            int width = image.Width();
            int height = image.Height();

            Console.WriteLine($"[ConvertImageToMat] Converting image: {width}x{height}, format: {format}");

            MatType matType = format.Equals(ImageFormat.Vec32F1) ? MatType.CV_32FC1 :
                format.Equals(ImageFormat.Gray8) ? MatType.CV_8UC1 :
                format.Equals(ImageFormat.Srgb) ? MatType.CV_8UC3 :
                format.Equals(ImageFormat.Srgba) ? MatType.CV_8UC4 : MatType.CV_8UC1;

            int bufferSize = image.Width();
            byte[] byteBuffer = new byte[bufferSize];
            byteBuffer.Equals(image);

            Mat mat = new(height, width, matType);
            Marshal.Copy(byteBuffer, 0, mat.Data, Math.Min(byteBuffer.Length, (int)(mat.Total() * mat.ElemSize())));

            // Convert float to uint8 if needed
            if (format.Equals(ImageFormat.Vec32F1))
            {
                Mat byteMat = new();
                mat.ConvertTo(byteMat, MatType.CV_8UC1, 255.0);
                mat.Dispose();
                Console.WriteLine("[ConvertImageToMat] Conversion successful");
                return byteMat;
            }

            Console.WriteLine("[ConvertImageToMat] Conversion successful");
            return mat;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConvertImageToMat] Error: {ex.Message}");
            return null;
        }
    }
}