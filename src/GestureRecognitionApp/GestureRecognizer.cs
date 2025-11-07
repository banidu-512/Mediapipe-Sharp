using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestureRecognitionApp
{
    /// <summary>
    /// Recognizes various hand gestures from hand landmarks using geometric analysis
    /// </summary>
    public class GestureRecognizer
    {
        // Hand landmark indices (MediaPipe hand landmark model)
        private const int WRIST = 0;
        private const int THUMB_CMC = 1;
        private const int THUMB_IP = 2;
        private const int THUMB_MCP = 3;
        private const int THUMB_IP2 = 4;
        private const int THUMB_TIP = 4;
        private const int INDEX_FINGER_MCP = 5;
        private const int INDEX_FINGER_PIP = 6;
        private const int INDEX_FINGER_DIP = 7;
        private const int INDEX_FINGER_TIP = 8;
        private const int MIDDLE_FINGER_MCP = 9;
        private const int MIDDLE_FINGER_PIP = 10;
        private const int MIDDLE_FINGER_DIP = 11;
        private const int MIDDLE_FINGER_TIP = 12;
        private const int RING_FINGER_MCP = 13;
        private const int RING_FINGER_PIP = 14;
        private const int RING_FINGER_DIP = 15;
        private const int RING_FINGER_TIP = 16;
        private const int PINKY_MCP = 17;
        private const int PINKY_PIP = 18;
        private const int PINKY_DIP = 19;
        private const int PINKY_TIP = 20;

        /// <summary>
        /// Recognized gesture types
        /// </summary>
        public enum GestureType
        {
            Unknown,
            ThumbsUp,
            ThumbsDown,
            OK,
            Point,
            Fist,
            OpenPalm,
            Victory,
            One,
            Two,
            Three,
            Four,
            Five,
            Rock,
            Paper,
            Scissors,
            CallMe,
            Shaka,
            Heart,
            ComeHere,
            GoAway,
            StopHand
        }

        /// <summary>
        /// Gesture recognition result
        /// </summary>
        public class GestureResult
        {
            public GestureType Gesture { get; set; }
            public float Confidence { get; set; }
            public string Description { get; set; } = string.Empty;
            public bool IsLeftHand { get; set; }
        }

        /// <summary>
        /// Recognize gesture from hand landmarks
        /// </summary>
        /// <param name="handLandmarks">Hand landmarks from MediaPipe</param>
        /// <param name="handedness">Handedness information</param>
        /// <returns>Gesture recognition result</returns>
        public static GestureResult RecognizeGesture(NormalizedLandmarks handLandmarks, List<Classifications>? handedness)
        {
            if (handLandmarks.landmarks.Equals(null) || handLandmarks.landmarks.Count < 21)
            {
                return new GestureResult { Gesture = GestureType.Unknown, Confidence = 0f, Description = "No hand detected" };
            }

            var landmarks = handLandmarks.landmarks.ToArray();
            bool isLeftHand = DetermineHandedness(handedness);

            // Check various gestures in order of specificity
            var results = new List<GestureResult>();

            // Advanced gestures
            results.Add(CheckOK(landmarks, isLeftHand));
            results.Add(CheckHeartSign(landmarks, isLeftHand));
            results.Add(CheckCallMe(landmarks, isLeftHand));
            results.Add(CheckShaka(landmarks, isLeftHand));

            // Number gestures
            results.Add(CheckVictory(landmarks, isLeftHand));
            results.Add(CheckThree(landmarks, isLeftHand));
            results.Add(CheckFour(landmarks, isLeftHand));
            results.Add(CheckTwo(landmarks, isLeftHand));
            results.Add(CheckOne(landmarks, isLeftHand));

            // Basic gestures
            results.Add(CheckThumbsUp(landmarks, isLeftHand));
            results.Add(CheckThumbsDown(landmarks, isLeftHand));
            results.Add(CheckPoint(landmarks, isLeftHand));
            results.Add(CheckScissors(landmarks, isLeftHand));
            results.Add(CheckRock(landmarks, isLeftHand));
            results.Add(CheckPaper(landmarks, isLeftHand));
            results.Add(CheckOpenPalm(landmarks, isLeftHand));
            results.Add(CheckFist(landmarks, isLeftHand));
            results.Add(CheckStopHand(landmarks, isLeftHand));

            // Interactive gestures
            results.Add(CheckComeHere(landmarks, isLeftHand));
            results.Add(CheckGoAway(landmarks, isLeftHand));

            // Return the result with highest confidence
            var bestResult = results.OrderByDescending(r => r.Confidence).First();
            if (bestResult.Confidence < 0.3f)
            {
                return new GestureResult { Gesture = GestureType.Unknown, Confidence = bestResult.Confidence, Description = "Unknown gesture", IsLeftHand = isLeftHand };
            }

            bestResult.IsLeftHand = isLeftHand;
            return bestResult;
        }

        private static bool DetermineHandedness(List<Classifications>? handedness)
        {
            if (handedness?.Any() == true)
            {
                var classification = handedness.First();
                return classification.Categories?.Any(c => c.Index == 0) == true; // index 0 = Left
            }
            return false; // Default to right hand
        }

        private static float CalculateDistance(NormalizedLandmark p1, NormalizedLandmark p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private static float CalculateAngle(NormalizedLandmark p1, NormalizedLandmark p2, NormalizedLandmark p3)
        {
            // Calculate angle at p2 between p1-p2-p3
            float a = CalculateDistance(p2, p3);
            float b = CalculateDistance(p1, p2);
            float c = CalculateDistance(p1, p3);
            
            if (a * b == 0) return 0;
            
            float cosAngle = (a * a + b * b - c * c) / (2 * a * b);
            cosAngle = Math.Max(-1, Math.Min(1, cosAngle)); // Clamp to valid range
            
            return (float)(Math.Acos(cosAngle) * 180 / Math.PI);
        }

        private static bool IsFingerExtended(NormalizedLandmark[] landmarks, int tipIndex, int pipIndex, int mcpIndex)
        {
            var tip = landmarks[tipIndex];
            var pip = landmarks[pipIndex];
            var mcp = landmarks[mcpIndex];
            var wrist = landmarks[WRIST];

            // Finger is extended if tip is further from wrist than pip is
            float tipDistance = CalculateDistance(wrist, tip);
            float pipDistance = CalculateDistance(wrist, pip);
            
            return tipDistance > pipDistance * 1.1f;
        }

        private static GestureResult CheckThumbsUp(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool thumbExtended = IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);
            bool indexFolded = !IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (thumbExtended) confidence += 0.3f;
            if (indexFolded) confidence += 0.2f;
            if (middleFolded) confidence += 0.2f;
            if (ringFolded) confidence += 0.15f;
            if (pinkyFolded) confidence += 0.15f;

            return new GestureResult 
            { 
                Gesture = GestureType.ThumbsUp, 
                Confidence = confidence,
                Description = "Thumbs Up"
            };
        }

        private static GestureResult CheckThumbsDown(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            var thumbTip = landmarks[THUMB_TIP];
            var wrist = landmarks[WRIST];
            
            // Thumb is down if tip is below wrist
            bool thumbDown = thumbTip.Y > wrist.Y + 0.1f;
            bool otherFingersFolded = !IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP) &&
                                   !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP) &&
                                   !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP) &&
                                   !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = thumbDown ? 0.5f : 0f;
            if (otherFingersFolded) confidence += 0.5f;

            return new GestureResult 
            { 
                Gesture = GestureType.ThumbsDown, 
                Confidence = confidence,
                Description = "Thumbs Down"
            };
        }

        private static GestureResult CheckOK(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            var thumbTip = landmarks[THUMB_TIP];
            var indexTip = landmarks[INDEX_FINGER_TIP];
            
            // Check if thumb and index tips are close (forming circle)
            float thumbIndexDistance = CalculateDistance(thumbTip, indexTip);
            bool tipsClose = thumbIndexDistance < 0.05f;
            
            // Check if other fingers are extended
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringExtended = IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyExtended = IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (tipsClose) confidence += 0.4f;
            if (middleExtended) confidence += 0.2f;
            if (ringExtended) confidence += 0.2f;
            if (pinkyExtended) confidence += 0.2f;

            return new GestureResult 
            { 
                Gesture = GestureType.OK, 
                Confidence = confidence,
                Description = "OK Sign"
            };
        }

        private static GestureResult CheckPoint(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            bool thumbFolded = !IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);

            float confidence = 0f;
            if (indexExtended) confidence += 0.4f;
            if (middleFolded) confidence += 0.15f;
            if (ringFolded) confidence += 0.15f;
            if (pinkyFolded) confidence += 0.15f;
            if (thumbFolded) confidence += 0.15f;

            return new GestureResult 
            { 
                Gesture = GestureType.Point, 
                Confidence = confidence,
                Description = "Pointing"
            };
        }

        private static GestureResult CheckFist(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexFolded = !IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            bool thumbFolded = !IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);

            float confidence = 0f;
            if (indexFolded) confidence += 0.2f;
            if (middleFolded) confidence += 0.2f;
            if (ringFolded) confidence += 0.2f;
            if (pinkyFolded) confidence += 0.2f;
            if (thumbFolded) confidence += 0.2f;

            return new GestureResult 
            { 
                Gesture = GestureType.Fist, 
                Confidence = confidence,
                Description = "Fist"
            };
        }

        private static GestureResult CheckOpenPalm(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringExtended = IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyExtended = IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (indexExtended) confidence += 0.25f;
            if (middleExtended) confidence += 0.25f;
            if (ringExtended) confidence += 0.25f;
            if (pinkyExtended) confidence += 0.25f;

            return new GestureResult 
            { 
                Gesture = GestureType.OpenPalm, 
                Confidence = confidence,
                Description = "Open Palm"
            };
        }

        private static GestureResult CheckVictory(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            bool thumbFolded = !IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);

            float confidence = 0f;
            if (indexExtended && middleExtended) confidence += 0.4f;
            if (ringFolded) confidence += 0.2f;
            if (pinkyFolded) confidence += 0.2f;
            if (thumbFolded) confidence += 0.2f;

            return new GestureResult 
            { 
                Gesture = GestureType.Victory, 
                Confidence = confidence,
                Description = "Victory (Peace)"
            };
        }

        private static GestureResult CheckTwo(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool otherFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP) &&
                              !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (indexExtended && middleExtended) confidence += 0.5f;
            if (otherFolded) confidence += 0.5f;

            return new GestureResult 
            { 
                Gesture = GestureType.Two, 
                Confidence = confidence,
                Description = "Two"
            };
        }

        private static GestureResult CheckOne(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (indexExtended) confidence += 0.4f;
            if (middleFolded) confidence += 0.2f;
            if (ringFolded) confidence += 0.2f;
            if (pinkyFolded) confidence += 0.2f;

            return new GestureResult 
            { 
                Gesture = GestureType.One, 
                Confidence = confidence,
                Description = "One"
            };
        }

        private static GestureResult CheckThree(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringExtended = IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (indexExtended && middleExtended && ringExtended) confidence += 0.6f;
            if (pinkyFolded) confidence += 0.4f;

            return new GestureResult 
            { 
                Gesture = GestureType.Three, 
                Confidence = confidence,
                Description = "Three"
            };
        }

        private static GestureResult CheckFour(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringExtended = IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyExtended = IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            bool thumbFolded = !IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);

            float confidence = 0f;
            if (indexExtended && middleExtended && ringExtended && pinkyExtended) confidence += 0.7f;
            if (thumbFolded) confidence += 0.3f;

            return new GestureResult 
            { 
                Gesture = GestureType.Four, 
                Confidence = confidence,
                Description = "Four"
            };
        }

        private static GestureResult CheckRock(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexFolded = !IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyExtended = IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            bool thumbExtended = IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);

            float confidence = 0f;
            if (indexFolded && middleFolded && ringFolded) confidence += 0.4f;
            if (pinkyExtended) confidence += 0.2f;
            if (thumbExtended) confidence += 0.4f;

            return new GestureResult 
            { 
                Gesture = GestureType.Rock, 
                Confidence = confidence,
                Description = "Rock"
            };
        }

        private static GestureResult CheckPaper(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool allFingersExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP) &&
                                   IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP) &&
                                   IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP) &&
                                   IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = allFingersExtended ? 0.8f : 0f;

            return new GestureResult 
            { 
                Gesture = GestureType.Paper, 
                Confidence = confidence,
                Description = "Paper"
            };
        }

        private static GestureResult CheckScissors(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            bool thumbFolded = !IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);

            float confidence = 0f;
            if (indexExtended && middleExtended) confidence += 0.5f;
            if (ringFolded && pinkyFolded) confidence += 0.3f;
            if (thumbFolded) confidence += 0.2f;

            return new GestureResult 
            { 
                Gesture = GestureType.Scissors, 
                Confidence = confidence,
                Description = "Scissors"
            };
        }

        private static GestureResult CheckCallMe(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            var thumbTip = landmarks[THUMB_TIP];
            var pinkyTip = landmarks[PINKY_TIP];
            var pinkyMcp = landmarks[PINKY_MCP];
            
            // Check if thumb and pinky are extended and close
            float thumbPinkyDistance = CalculateDistance(thumbTip, pinkyTip);
            bool thumbPinkyClose = thumbPinkyDistance < 0.15f;
            
            bool thumbExtended = IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);
            bool pinkyExtended = IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            
            // Other fingers should be folded
            bool indexFolded = !IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);

            float confidence = 0f;
            if (thumbPinkyClose) confidence += 0.4f;
            if (thumbExtended && pinkyExtended) confidence += 0.3f;
            if (indexFolded && middleFolded && ringFolded) confidence += 0.3f;

            return new GestureResult 
            { 
                Gesture = GestureType.CallMe, 
                Confidence = confidence,
                Description = "Call Me"
            };
        }

        private static GestureResult CheckShaka(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            var thumbTip = landmarks[THUMB_TIP];
            var pinkyTip = landmarks[PINKY_TIP];
            
            bool thumbExtended = IsFingerExtended(landmarks, THUMB_TIP, THUMB_IP2, THUMB_MCP);
            bool pinkyExtended = IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            
            // Middle and ring fingers should be folded
            bool middleFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            
            // Index finger can be either extended or folded

            float confidence = 0f;
            if (thumbExtended && pinkyExtended) confidence += 0.4f;
            if (middleFolded && ringFolded) confidence += 0.4f;
            if (thumbTip.Y < 0.5f && pinkyTip.Y < 0.5f) confidence += 0.2f; // Both pointing up

            return new GestureResult 
            { 
                Gesture = GestureType.Shaka, 
                Confidence = confidence,
                Description = "Shaka"
            };
        }

        private static GestureResult CheckHeartSign(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            var thumbTip = landmarks[THUMB_TIP];
            var indexTip = landmarks[INDEX_FINGER_TIP];
            var middleTip = landmarks[MIDDLE_FINGER_TIP];
            
            // Heart sign: thumb and index forming heart shape, middle often pointing up
            bool thumbIndexClose = CalculateDistance(thumbTip, indexTip) < 0.08f;
            bool middleExtended = IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP);
            
            // Ring and pinky usually folded
            bool ringFolded = !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP);
            bool pinkyFolded = !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);

            float confidence = 0f;
            if (thumbIndexClose) confidence += 0.4f;
            if (middleExtended) confidence += 0.3f;
            if (ringFolded && pinkyFolded) confidence += 0.3f;

            return new GestureResult 
            { 
                Gesture = GestureType.Heart, 
                Confidence = confidence,
                Description = "Heart Sign"
            };
        }

        private static GestureResult CheckComeHere(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool indexExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP);
            bool otherFolded = !IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP) &&
                              !IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP) &&
                              !IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            
            // Check if index finger is moving in beckoning motion (simplified: check if pointing downward)
            var indexTip = landmarks[INDEX_FINGER_TIP];
            var wrist = landmarks[WRIST];
            bool pointingDown = indexTip.Y > wrist.Y + 0.05f;

            float confidence = 0f;
            if (indexExtended && otherFolded) confidence += 0.5f;
            if (pointingDown) confidence += 0.5f;

            return new GestureResult 
            { 
                Gesture = GestureType.ComeHere, 
                Confidence = confidence,
                Description = "Come Here"
            };
        }

        private static GestureResult CheckGoAway(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool allExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP) &&
                             IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP) &&
                             IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP) &&
                             IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            
            // Check if palm is facing forward/push gesture (simplified: check if fingers are pointing forward)
            var wrist = landmarks[WRIST];
            var middleTip = landmarks[MIDDLE_FINGER_TIP];
            bool palmForward = middleTip.X > wrist.X + 0.1f; // For right hand pointing right

            float confidence = 0f;
            if (allExtended) confidence += 0.5f;
            if (palmForward) confidence += 0.5f;

            return new GestureResult 
            { 
                Gesture = GestureType.GoAway, 
                Confidence = confidence,
                Description = "Go Away"
            };
        }

        private static GestureResult CheckStopHand(NormalizedLandmark[] landmarks, bool isLeftHand)
        {
            bool allExtended = IsFingerExtended(landmarks, INDEX_FINGER_TIP, INDEX_FINGER_PIP, INDEX_FINGER_MCP) &&
                             IsFingerExtended(landmarks, MIDDLE_FINGER_TIP, MIDDLE_FINGER_PIP, MIDDLE_FINGER_MCP) &&
                             IsFingerExtended(landmarks, RING_FINGER_TIP, RING_FINGER_PIP, RING_FINGER_MCP) &&
                             IsFingerExtended(landmarks, PINKY_TIP, PINKY_PIP, PINKY_MCP);
            
            // Check if palm is facing forward (stop gesture)
            var wrist = landmarks[WRIST];
            var middleTip = landmarks[MIDDLE_FINGER_TIP];
            bool palmFacingForward = Math.Abs(middleTip.X - wrist.X) < 0.1f && middleTip.Y < wrist.Y;

            float confidence = 0f;
            if (allExtended) confidence += 0.5f;
            if (palmFacingForward) confidence += 0.5f;

            return new GestureResult 
            { 
                Gesture = GestureType.StopHand, 
                Confidence = confidence,
                Description = "Stop Hand"
            };
        }
    }
}
