using BenchmarkDotNet.Running;
using CaptureRenderTagHelper.Benchmarks.Benchmarks;

namespace CaptureRenderTagHelper.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CaptureRenderTagBenchmark>();
            BenchmarkRunner.Run<ScriptRenderTagBenchmark>();
        }
    }
}
