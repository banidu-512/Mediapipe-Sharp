using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Mediapipe.Framework.Formats;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.Test;

public class HandDetectorTest
{
    public void Init()
    {
        string win1 = "win";
        CvInvoke.NamedWindow(win1);
        HandLandmarkerOptions options =
            new(new CoreBaseOptions(CoreBaseOptions.Delegate.CPU, "Models/hand_landmarker.task"));
        using HandLandmarker faceDetector = HandLandmarker.CreateFromOptions(options);
        using VideoCapture videoCapture = new();
        while (CvInvoke.WaitKey(1) == -1)
        {
            using Mat frame = new();
            videoCapture.Read(frame);

            using Mat rgb = new();
            CvInvoke.CvtColor(frame, rgb, ColorConversion.Bgr2Rgb);

            using Image image = new(ImageFormat.Types.Format.Srgb, rgb.Width, rgb.Height,
                rgb.Width * rgb.NumberOfChannels, rgb.GetRawData());
            HandLandmarkerResult result = faceDetector.Detect(image);
            if (result.HandLandmarks != null)
                foreach (NormalizedLandmarks landmarks in result.HandLandmarks)
                    if (landmarks.landmarks != null)
                    {
                        Tasks.Components.Containers.NormalizedLandmark landmark = landmarks.landmarks[8];
                        CvInvoke.Circle(rgb, new Point((int)(landmark.X * rgb.Width), (int)(landmark.Y * rgb.Height)),
                            10, new MCvScalar(100, 100, 100));
                    }

            CvInvoke.Imshow(win1, rgb);
            Console.WriteLine(result.HandLandmarks?.Count);
        }

        CvInvoke.DestroyAllWindows();
    }
}