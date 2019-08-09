using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace 线程池等待与超时
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
        public void WorkerOperationWait(CancellationTokenSource cts,bool isTimeOut)
        {
            if(isTimeOut)
            {
                cts.Cancel();
                WriteLine("Worker operation time out and was canceled.");
            }
            else
            {
                WriteLine("Worker operation succeded");
            }
        }
        public void WorkOperation(CancellationToken token,ManualResetEvent evt)
        {
            for(int i=0;i<6;i++)
            {
                if(token.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            evt.Set();
        }
        public void RunOperations(TimeSpan wrokerOperationTimeout)
        {
            using(var evt=new ManualResetEvent(false))
            {
                using (var cts=new CancellationTokenSource())
                {
                    var worker = ThreadPool.RegisterWaitForSingleObject(evt, (state, isTimedOut) => WorkerOperationWait(cts, isTimedOut), null, wrokerOperationTimeout, true);

                    WriteLine("Starting long running operation ..");
                    ThreadPool.QueueUserWorkItem(_ => WorkOperation(cts.Token, evt));
                    Thread.Sleep(wrokerOperationTimeout.Add(TimeSpan.FromSeconds(2)));
                    worker.Unregister(evt);
                }
            }

        }
        [Fact]
        public void MainTest()
        {
            RunOperations(TimeSpan.FromSeconds(5));
            RunOperations(TimeSpan.FromSeconds(7));
            Console.WriteLine();
        }
    }
}
