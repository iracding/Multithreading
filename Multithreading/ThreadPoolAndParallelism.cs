using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace 线程池与并行度
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
        public void UseThreads(int numberOfOperations)
        {
            using(var countDown=new CountdownEvent(numberOfOperations))
            {
                WriteLine("Scheduling work by creating threads");
                for(int i=0;i<numberOfOperations;i++)
                {
                    var thread = new Thread(() =>
                    {
                        WriteLine($"{Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countDown.Signal();
                    }
                    );
                    thread.Start();
                }
                countDown.Wait();

            }
        }
        public void UseThreadPool(int numberOfOperations)
        {
            using (var countDown = new CountdownEvent(numberOfOperations))
            {
                WriteLine("Scheduling work by creating threads");
                for (int i = 0; i < numberOfOperations; i++)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        WriteLine($"{Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        countDown.Signal();
                    });
                }
                countDown.Wait();
            }
        }
        [Fact]
        public void MainTest()
        {
            const int numberOfOperations = 500;
            var sw = new Stopwatch();
            sw.Start();
            UseThreads(numberOfOperations);
            sw.Stop();
            WriteLine($"Execution time using threads:{sw.ElapsedMilliseconds}");
            sw.Reset();
            sw.Start();
            UseThreadPool(numberOfOperations);
            sw.Stop();
            WriteLine($"Execution time using threads:{sw.ElapsedMilliseconds}");
            Console.WriteLine();
        }
    }
}
