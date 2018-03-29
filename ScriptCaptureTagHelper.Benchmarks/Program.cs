using BenchmarkDotNet.Running;
using ScriptCaptureTagHelper.Benchmarks.Benchmarks;

namespace ScriptCaptureTagHelper.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ScriptTagBenchmark>();
        }
    }
}
