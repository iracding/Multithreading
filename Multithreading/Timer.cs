using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Timer类
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
        static Timer _timer;
        public void TimerOperation(DateTime start)
        {
            TimeSpan elapsed = DateTime.Now - start;
            WriteLine($"{elapsed.Seconds} seconds from{start}.Timer thread pool thread id :{Thread.CurrentThread.ManagedThreadId}");
        }
        [Fact]
        public void MainTest()
        {
            WriteLine("Press 'Enter' to stop the timer...");
            DateTime start = DateTime.Now;
            _timer = new Timer(_ => TimerOperation(start),null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            Thread.Sleep(TimeSpan.FromSeconds(6));
            _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4));
            Console.ReadLine();
            _timer.Dispose();
        }
    }
}
