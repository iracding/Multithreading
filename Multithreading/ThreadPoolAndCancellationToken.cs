using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace 线程池与取消
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
        public void AsyncOperation1(CancellationToken token)
        {
            WriteLine("Starting the first task");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    WriteLine("The first task has been canceled.");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            WriteLine("The first task has completed sucesfully");
        }
        public void AsyncOperation2(CancellationToken token)
        {
            try
            {
                WriteLine("Starting the second task");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                WriteLine("The first task has completed sucesfully");
            }
            catch (OperationCanceledException)
            {
                WriteLine("The second task has been canceled.");
            }
        }
        public void AsyncOperation3(CancellationToken token)
        {
            bool concellationFlag = false;
            token.Register(() => concellationFlag = true);
            WriteLine("Starting the third task");
            for (int i = 0; i<5;i++)
            {
                if(concellationFlag)
                {
                    WriteLine("The third task has been canceled.");
                    return;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            WriteLine("The third task has completed succesfully");
        }
        [Fact]
        public void MainTest()
        {
            using(var cts=new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation1(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation2(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }
            using (var cts = new CancellationTokenSource())
            {
                CancellationToken token = cts.Token;
                ThreadPool.QueueUserWorkItem(_ => AsyncOperation3(token));
                Thread.Sleep(TimeSpan.FromSeconds(2));
                cts.Cancel();
            }
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();
        }
    }
}
