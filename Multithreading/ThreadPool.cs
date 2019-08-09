using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ThreadPool类
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
        public  void AsyncOperation(object state)
        {
            WriteLine($"Operation state:{state??"null"}");
            WriteLine($"Worker thread id:{Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
        [Fact]
        public void TestMain()
        {
            const int x = 1;
            const int y = 2;
            const string lambdaState = "lambda state 2";
            ThreadPool.QueueUserWorkItem(AsyncOperation);
            Thread.Sleep(TimeSpan.FromSeconds(1));
            ThreadPool.QueueUserWorkItem(AsyncOperation, "async state");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            ThreadPool.QueueUserWorkItem(state =>
            {
                WriteLine($"Operation state:{Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            },"lambda state");
            ThreadPool.QueueUserWorkItem(_ =>
            {
                WriteLine($"Operation state:{x + y},{lambdaState}");
                WriteLine($"Wroker thread id :{Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            },"lambda state");
            Console.WriteLine();
        }
    }
}
