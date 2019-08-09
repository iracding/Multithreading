using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Task4
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
        public int TaskMethod(string name, int seconds,CancellationToken token)
        {
            WriteLine($"Task{name} is ruuning on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread :{Thread.CurrentThread.IsThreadPoolThread} ");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            for(int i=0;i<seconds;i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if(token.IsCancellationRequested)
                {
                    return -1;
                }
            }
            return 42 * seconds;
        }
        [Fact]
        public void MainTest()
        {
            var cts = new CancellationTokenSource();
            var longTask = new Task<int>(() => TaskMethod("Task1", 10, cts.Token));
            longTask.Start();
            WriteLine(longTask.Result.ToString());
            cts.Cancel();
            WriteLine(longTask.Result.ToString());
            WriteLine("First task has been cancelled before execution");
            cts = new CancellationTokenSource();
            longTask = new Task<int>(() => TaskMethod("Task2", 10, cts.Token));
            longTask.Start();
            for (int i = 0; i<5;i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                WriteLine(longTask.Status.ToString());
            }
            WriteLine($"A task has been completed with result {longTask.Result}");
        }
    }
}
