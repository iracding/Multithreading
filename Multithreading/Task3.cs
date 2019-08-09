using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Task3
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
        public int TaskMethod(string name, int seconds)
        {
            WriteLine($"Task{name} is ruuning on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread :{Thread.CurrentThread.IsThreadPoolThread} ");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            return 42 * seconds;
        }
        [Fact]
        public void MainTest()
        {
            var tcs = new TaskCompletionSource<int>();
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, eventArgs) =>
              {
                  eventArgs.Result = TaskMethod("background worker",5);
              };
            worker.RunWorkerCompleted+= (sender, eventArgs) =>
            {
               if(eventArgs.Error!=null)
                {
                    tcs.SetException(eventArgs.Error);
                }
               else if(eventArgs.Cancelled)
                {
                    tcs.SetCanceled();
                }
               else
                {
                    tcs.SetResult((int)eventArgs.Result);
                }
            };
            worker.RunWorkerAsync();
            int result = tcs.Task.Result;
            WriteLine($"Result is :{result}");
        }
    }
}
