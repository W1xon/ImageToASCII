using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public static class ConversionProcessorFactory
{
    public static IConversionProcessor Create(JobType type)
    {
        return type switch
        {
            JobType.ImageToAscii => new ImageConversionProcessor(),
            _ => throw new NotSupportedException()
        };
    }
}