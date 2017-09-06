using System;
using System.Diagnostics;
using System.Text;

namespace CloneExtensions.UnitTests.Helpers
{
    public static class TimingHelper
    {
        public static TimingResult<T> TimeIt<T>(Func<T> func)
        {
            DateTime start = DateTime.Now;

            T result = func();

            return new TimingResult<T>()
            {
                Result = result,
                Elapsed = DateTime.Now.Subtract(start).TotalMilliseconds
            };
        }

        public static ComparisonResult ComparePerformance(
            int iterationsFirst,
            int iterationsSecond,
            Action first,
            Action second)
        {
            first();
            DateTime start = DateTime.Now;
            for (int i = 0; i < iterationsFirst; i++)
            {
                first();
            }
            var firstElapsed = DateTime.Now.Subtract(start).TotalMilliseconds;

            second();
            start = DateTime.Now;
            for (int i = 0; i < iterationsSecond; i++)
            {
                second();
            }
            var secondElapsed = DateTime.Now.Subtract(start).TotalMilliseconds;

            var firstOpsPerSec = ((double)iterationsFirst) / (firstElapsed / ((double)1000));
            var secondOpsPerSec = ((double)iterationsSecond) / (secondElapsed / ((double)1000));

            return new ComparisonResult()
            {
                IterationsFirst = iterationsFirst,
                IterationsSecond = iterationsSecond,
                FirstTotalTime = firstElapsed,
                SecondTotalTime = secondElapsed,
                FirstOpsPerSec = firstOpsPerSec,
                SecondOpsPerSec = secondOpsPerSec,
                PeformanceDiff = Math.Ceiling((((firstElapsed / secondElapsed) - 1) * 100))
            };
        }

        public static PerformanceResult GetPerformance(
            int iterations,
            Action act)
        {
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                act();
            }

            sw.Stop();
            var total = sw.ElapsedMilliseconds;

            var opsPerSec = ((double)iterations) / (total / ((double)1000));

            return new PerformanceResult()
            {
                Ave = total / ((double)iterations),
                Count = iterations,
                Total = total,
                OpsPerSec = opsPerSec
            };
        }
    }

    [DebuggerDisplay("{Elapsed} - {Result}")]
    public class TimingResult<T>
    {
        public double Elapsed { get; set; }
        public T Result { get; set; }
    }

    public class ComparisonResult
    {
        public int IterationsFirst { get; set; }
        public int IterationsSecond { get; set; }
        public double FirstTotalTime { get; set; }
        public double SecondTotalTime { get; set; }
        public double FirstOpsPerSec { get; set; }
        public double SecondOpsPerSec { get; set; }
        public double PeformanceDiff { get; set; }

        public string GetReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("IterationsFirst : " + this.IterationsFirst.ToString("N0"));
            sb.AppendLine("IterationsSecond : " + this.IterationsSecond.ToString("N0"));
            sb.AppendLine("Total Time - First (MS): " + this.FirstTotalTime);
            sb.AppendLine("Total Time - Second (MS): " + this.SecondTotalTime);
            sb.AppendLine("Ops per Sec - First: " + this.FirstOpsPerSec.ToString("N3"));
            sb.AppendLine("Ops per Sec - Second: " + this.SecondOpsPerSec.ToString("N3"));

            if (this.PeformanceDiff > 0)
            {
                sb.AppendLine("Performance Increase: " + this.PeformanceDiff + "%");
            }
            else
            {
                sb.AppendLine("Performance Decrease: " + Math.Abs(this.PeformanceDiff) + "%");
            }

            return sb.ToString();
        }
    }

    public class PerformanceResult
    {
        public int Count { get; set; }
        public double Ave { get; set; }
        public double Total { get; set; }
        public double OpsPerSec { get; set; }

        public string GetReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Count : " + this.Count.ToString("N0"));
            sb.AppendLine("Ave : " + this.Ave.ToString("N10"));
            sb.AppendLine("Total : " + this.Total);
            sb.AppendLine("Ops per Sec: " + this.OpsPerSec.ToString("N3"));
            return sb.ToString().Trim();
        }
    }
}