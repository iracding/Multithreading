using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace RunOnThreadPoolApm
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
        private delegate string RunOnThreadPool(out int threadId);
        public void Callback(IAsyncResult ar)
        {
            WriteLine("Starting a callback....");
            WriteLine($"State passed to a callback:{ar.AsyncState}");
            WriteLine($"Is thread pool thread{Thread.CurrentThread.IsThreadPoolThread}");
            WriteLine($"Is thread pool worker threadId{Thread.CurrentThread.ManagedThreadId}");
        }
        public string Test1(out int threadId)
        {
            WriteLine("Starting....");
            WriteLine($"Is thread pool thread{Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            threadId = Thread.CurrentThread.ManagedThreadId;
            return $"Thread pool worker thread id was:{threadId}";
        }
        [Fact]
        public void TestMain()
        {
            int threadId = 0;
            RunOnThreadPool poolDelegate = Test1;
            var t = new Thread(() => Test1(out threadId));
            t.Start();
            t.Join();
            WriteLine($"Thread id:{threadId}");
            IAsyncResult r = poolDelegate.BeginInvoke(out threadId, Callback, "a delegate asynchronous call");
            r.AsyncWaitHandle.WaitOne();
            string result = poolDelegate.EndInvoke(out threadId,r);
            WriteLine($"Thread pool worker thread id:{threadId}");
            Console.WriteLine(result);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();
        }
    }
}
