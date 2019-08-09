using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Async2
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
        public async Task AsynchronousProcessing()
        {
            Func<string, Task<string>> asyncLambda = async name =>
               {
                   await Task.Delay(TimeSpan.FromSeconds(2));
                   return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}";
               };
            string result = await asyncLambda("async lambda");
            WriteLine(result);
        }
        [Fact]
        public void MainTest()
        {
            Task t = AsynchronousProcessing();
            t.Wait();
        }
    }
}
