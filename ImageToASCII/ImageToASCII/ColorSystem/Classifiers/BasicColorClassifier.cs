using SkiaSharp;

namespace ImageToASCII.ColorSystem;

public sealed class BasicColorClassifier : IColorClassifier
{
    private const byte HighThreshold = 200;
    private const byte MidThreshold  = 150;
    private const byte LowThreshold  = 50;
    private const byte GrayThreshold = 100;

    public uint GetColor(byte red, byte green, byte blue)
    {
        // Белый / Чёрный
        if (IsAbove(red, green, blue, HighThreshold))
            return Pack(SKColors.White);

        if (IsBelow(red, green, blue, LowThreshold))
            return Pack(SKColors.Black);

        // Чистые цвета
        if (red   > HighThreshold && green < LowThreshold  && blue  < LowThreshold)
            return Pack(SKColors.Red);

        if (green > HighThreshold && red   < LowThreshold  && blue  < LowThreshold)
            return Pack(SKColors.Green);

        if (blue  > HighThreshold && red   < LowThreshold  && green < LowThreshold)
            return Pack(SKColors.Blue);

        // Вторичные цвета
        if (red > HighThreshold && green > HighThreshold && blue < LowThreshold)
            return Pack(SKColors.Yellow);

        if (red > HighThreshold && blue  > HighThreshold && green < LowThreshold)
            return Pack(SKColors.Magenta);

        if (green > HighThreshold && blue > HighThreshold && red < LowThreshold)
            return Pack(SKColors.Cyan);

        // Оттенки серого
        if (IsAbove(red, green, blue, MidThreshold))
            return Pack(SKColors.LightGray);

        if (IsAbove(red, green, blue, GrayThreshold))
            return Pack(SKColors.Gray);

        // Доминирующий канал
        if (red > green && red > blue)
            return Pack(SKColors.DarkRed);

        if (green > red && green > blue)
            return Pack(SKColors.DarkGreen);

        if (blue > red && blue > green)
            return Pack(SKColors.DarkBlue);

        // Тёплые оттенки
        if (red > green && red > blue && green > blue)
            return Pack(SKColors.Goldenrod);

        if (red > green && red > blue && blue > green)
            return Pack(SKColors.DeepPink);

        return Pack(SKColors.Gray);
    }

    private static bool IsAbove(byte r, byte g, byte b, byte threshold)
        => r > threshold && g > threshold && b > threshold;

    private static bool IsBelow(byte r, byte g, byte b, byte threshold)
        => r < threshold && g < threshold && b < threshold;

    private static uint Pack(SKColor color)
        => ColorUtils.Pack(color);
}