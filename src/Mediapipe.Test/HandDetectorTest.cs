using Emgu.CV;
using Emgu.CV.Structure;
using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.Test;

public class HandDetectorTest
{
    public void Init()
    {
        string win1 = "win";
        CvInvoke.NamedWindow(win1);
        var options = new HandLandmarkerOptions(new CoreBaseOptions(CoreBaseOptions.Delegate.CPU, modelAssetPath: "Models/hand_landmarker.task"));
        using var faceDetector = HandLandmarker.CreateFromOptions(options);
        using var videoCapture = new VideoCapture();
        while (CvInvoke.WaitKey(1) == -1)
        {
            using var frame = new Mat();
            videoCapture.Read(frame);

            using var rgb = new Mat();
            CvInvoke.CvtColor(frame, rgb, Emgu.CV.CvEnum.ColorConversion.Bgr2Rgb);

            using var image = new Image(ImageFormat.Types.Format.Srgb, rgb.Width, rgb.Height, rgb.Width * rgb.NumberOfChannels, rgb.GetRawData());
            var result = faceDetector.Detect(image);
            if (result.HandLandmarks != null)
            {
                foreach (var landmarks in result.HandLandmarks)
                {
                    if (landmarks.landmarks != null)
                    {
                        var landmark = landmarks.landmarks[8];
                        CvInvoke.Circle(rgb, new System.Drawing.Point((int)(landmark.X * rgb.Width), (int)(landmark.Y * rgb.Height)), 10, new MCvScalar(100, 100, 100));
                    }
                }
            }

            CvInvoke.Imshow(win1, rgb);
            Console.WriteLine(result.HandLandmarks?.Count);
        }
        CvInvoke.DestroyAllWindows();
    }
}