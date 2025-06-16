using Emgu.CV;
using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.FaceDetector;

namespace Mediapipe.Test;

public class FaceDetectorTest
{
    public void Init()
    {
        var options = new FaceDetectorOptions(new CoreBaseOptions(CoreBaseOptions.Delegate.CPU, modelAssetPath: "Models/blaze_face_short_range.tflite"));
        using var faceDetector = FaceDetector.CreateFromOptions(options);
        using var videoCapture = new VideoCapture();
        while (videoCapture.IsOpened)
        {
            using var frame = new Mat();
            videoCapture.Read(frame);

            using var rgb = new Mat();
            CvInvoke.CvtColor(frame, rgb, Emgu.CV.CvEnum.ColorConversion.Bgr2Rgb);

            using var image = new Image(ImageFormat.Types.Format.Srgb, rgb.Width, rgb.Height, rgb.Width * rgb.NumberOfChannels, rgb.GetRawData());
            var result = faceDetector.Detect(image);
            Console.WriteLine(result.Detections?.Count);
        }
    }
}