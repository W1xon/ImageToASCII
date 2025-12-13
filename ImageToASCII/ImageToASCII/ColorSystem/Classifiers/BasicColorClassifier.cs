
using SkiaSharp;

namespace ImageToASCII.ColorSystem
{
    public class BasicColorClassifier : IColorClassifier
    {
        private const byte _highThreshold = 200;
        private const byte _midThreshold  = 150;
        private const byte _lowThreshold  = 50;
        private const byte _grayThreshold = 100;

        public uint GetColor(byte red, byte green, byte blue)
        {
            
            if (red > _highThreshold && green > _highThreshold && blue > _highThreshold)
                return ColorUtils.Pack(SKColors.White);
            if (red < _lowThreshold && green < _lowThreshold && blue < _lowThreshold)
                return ColorUtils.Pack(SKColors.Black);
            if (red > _highThreshold && green < _lowThreshold && blue < _lowThreshold)
                return ColorUtils.Pack(SKColors.Red);
            if (red < _lowThreshold && green > _highThreshold && blue < _lowThreshold)
                return ColorUtils.Pack(SKColors.Green);
            if (red < _lowThreshold && green < _lowThreshold && blue > _highThreshold)
                return ColorUtils.Pack(SKColors.Blue);
            if (red > _highThreshold && green > _highThreshold && blue < _lowThreshold)
                return ColorUtils.Pack(SKColors.Yellow);
            if (red > _highThreshold && green < _lowThreshold && blue > _highThreshold)
                return ColorUtils.Pack(SKColors.Magenta);
            if (red < _lowThreshold && green > _highThreshold && blue > _highThreshold)
                return ColorUtils.Pack(SKColors.Cyan);
            if (red > _midThreshold && green > _midThreshold && blue > _midThreshold)
                return ColorUtils.Pack(SKColors.LightGray);
            if (red > _grayThreshold && green > _grayThreshold && blue > _grayThreshold)
                return ColorUtils.Pack(SKColors.Gray);
            if (red > green && red > blue)
                return ColorUtils.Pack(SKColors.DarkRed);
            if (green > red && green > blue)
                return ColorUtils.Pack(SKColors.DarkGreen);
            if (blue > red && blue > green)
                return ColorUtils.Pack(SKColors.DarkBlue);
            if (red > green && red > blue && green > blue)
                return ColorUtils.Pack(SKColors.Goldenrod);
            if (red > green && red > blue && blue > green)
                return ColorUtils.Pack(SKColors.DeepPink);

            return ColorUtils.Pack(SKColors.Gray);
        }
    }
}
