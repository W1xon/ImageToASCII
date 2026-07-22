using BenchmarkDotNet.Running;

namespace ImageToASCII.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<ImageProcessingBenchmarks>();
    }
}