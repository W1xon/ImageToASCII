using ImageToASCII.Core;
using ImageToASCII.Web.Models;

namespace ImageToASCII.Web;

public interface IConversionProcessor
{
    public Task Process(ConversionJob job, IReporter reporter);
}