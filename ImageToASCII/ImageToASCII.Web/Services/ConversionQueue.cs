using System.Threading.Channels;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public class ConversionQueue
{
    public readonly Channel<ConversionJob> ConversionChannel = Channel.CreateUnbounded<ConversionJob>();
    public async Task EnqueueAsync(ConversionJob job) => await ConversionChannel.Writer.WriteAsync(job);
}