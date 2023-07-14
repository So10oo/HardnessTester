using System;
using System.Windows.Media.Animation;

namespace TestsSystems_HardnessTester
{
    internal static class Animations
    {
        public static StringAnimationUsingKeyFrames CreatStrindAnimation(string message, double secondAnimation = 1, double secondEnd = 2)
        {
            StringAnimationUsingKeyFrames animation = new StringAnimationUsingKeyFrames();
            animation.AutoReverse = false;
            animation.FillBehavior = FillBehavior.Stop;
            double dTS = secondAnimation / message.Length;
            string tempText = string.Empty;
            double tempTime = 0;
            for (int i = 0; i < message.Length; i++)
            {
                tempTime += dTS;
                tempText += message[i];
                var frame = new DiscreteStringKeyFrame(tempText, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(tempTime)));
                animation.KeyFrames.Add(frame);
            }
            var frameEnd = new DiscreteStringKeyFrame(tempText, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(tempTime + secondEnd)));
            animation.KeyFrames.Add(frameEnd);
            return animation;
        }
    }
}
