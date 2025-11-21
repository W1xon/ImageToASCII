namespace ImageToASCII.Extensions;

public static class ConsoleExtensions
{
    public static void FitConsoleToImage(int imgWidth, int imgHeight)
    {
        int windowWidth  = Math.Min(imgWidth + 2, Console.LargestWindowWidth - 2);
        int windowHeight = Math.Min(imgHeight + 2, Console.LargestWindowHeight - 2);

        int bufferWidth  = windowWidth;
        int bufferHeight = imgHeight + 5; 

        if (Console.WindowWidth > windowWidth)
            Console.WindowWidth = windowWidth;
        if (Console.WindowHeight > windowHeight)
            Console.WindowHeight = windowHeight;

        if (Console.BufferWidth < bufferWidth)
            Console.BufferWidth = bufferWidth;
        if (Console.BufferHeight < bufferHeight)
            Console.BufferHeight = bufferHeight;

        Console.WindowWidth  = windowWidth;
        Console.WindowHeight = windowHeight;
    }

}