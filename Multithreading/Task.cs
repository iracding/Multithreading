using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Task类
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
        public void TaskMethod(string name)
        {
            WriteLine($"Task {name} is running on a thread id{Thread.CurrentThread.ManagedThreadId}. Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}");
        }
        [Fact]
        public void MainTest()
        {
            var t1 = new Task(() => TaskMethod("Task 1"));
            var t2 = new Task(() => TaskMethod("Task 2"));
            t1.Start();
            t2.Start();
            Task.Run(() => TaskMethod("Task3"));
            Task.Factory.StartNew(() => TaskMethod("Task 4"));
            Task.Factory.StartNew(() => TaskMethod("Task 5"),TaskCreationOptions.LongRunning);
            Thread.Sleep(TimeSpan.FromSeconds(1));

        }
    }
}
