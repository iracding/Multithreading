using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Parallel类
{
    public class Test
    {
        protected readonly ITestOutputHelper Output;
        object lockObject = new object();
        public void WriteLine(string message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message);
        }
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        public string EmulateProcessing(string taskName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(new Random(DateTime.Now.Millisecond).Next(250, 350)));
            WriteLine($"{taskName} task was processed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            return taskName;
        }
        [Fact]
        public void MainTest()
        {
            Parallel.Invoke(()=>EmulateProcessing("Task1"), () => EmulateProcessing("Task2"), () => EmulateProcessing("Task3"));
            var cts = new CancellationTokenSource();
            var result = Parallel.ForEach(Enumerable.Range(1, 30), new ParallelOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                TaskScheduler = TaskScheduler.Default
            }, (i, state) =>
             {
                 WriteLine(i.ToString());
                 if (i == 20)
                 {
                     state.Break();
                     WriteLine($"Loop is stopped:{state.IsStopped}");
                 }
             });
            WriteLine("............");
            WriteLine($"IsCompleted：{result.IsCompleted} ");
            WriteLine($"Lowest break iteration:{result.LowestBreakIteration}");
        }
    }
}