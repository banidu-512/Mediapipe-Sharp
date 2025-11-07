using Emgu.CV;
using Emgu.CV.CvEnum;
using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.FaceDetector;

namespace Mediapipe.Test;

public class FaceDetectorTest
{
    public void Init()
    {
        FaceDetectorOptions options =
            new(new CoreBaseOptions(CoreBaseOptions.Delegate.CPU, "Models/blaze_face_short_range.tflite"));
        using FaceDetector faceDetector = FaceDetector.CreateFromOptions(options);
        using VideoCapture videoCapture = new();
        while (videoCapture.IsOpened)
        {
            using Mat frame = new();
            videoCapture.Read(frame);

            using Mat rgb = new();
            CvInvoke.CvtColor(frame, rgb, ColorConversion.Bgr2Rgb);

            using Image image = new(ImageFormat.Types.Format.Srgb, rgb.Width, rgb.Height,
                rgb.Width * rgb.NumberOfChannels, rgb.GetRawData());
            DetectionResult result = faceDetector.Detect(image);
            Console.WriteLine(result.Detections?.Count);
        }
    }
}