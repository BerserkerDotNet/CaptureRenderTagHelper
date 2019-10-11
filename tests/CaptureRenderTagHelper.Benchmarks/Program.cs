using BenchmarkDotNet.Running;
using CaptureRenderTagHelper.Benchmarks.Benchmarks;

namespace CaptureRenderTagHelper.Benchmarks
{
    public class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<CaptureTagBenchmark>();
            BenchmarkRunner.Run<RenderTagBenchmark>();
        }
    }
}
