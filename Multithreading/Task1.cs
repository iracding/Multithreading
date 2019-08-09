using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Task1
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
        public Task<int> CreateTask(string name)
        {
            return new Task<int>(() => TaskMethod(name));
        }
        public int TaskMethod(string name)
        {
            WriteLine($"Task{name} is ruuning on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread :{Thread.CurrentThread.IsThreadPoolThread} ");
            Thread.Sleep(TimeSpan.FromSeconds(2));
            return 42;
        }
        [Fact]
        public void MainTest()
        {
            TaskMethod("Main Thread Task");
            Task<int> task = CreateTask("Task 1");
            task.Start();
            int result = task.Result;
            WriteLine($"Result is :{result}");
            task = CreateTask("Task 2");
            task.RunSynchronously();
            result = task.Result;
            WriteLine($"Result is :{result}");
            task = CreateTask("Task 3");
            task.Start();
            while(!task.IsCompleted)
            {
                WriteLine(task.Status.ToString());
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(task.Status.ToString());
            result = task.Result;
            WriteLine($"Result is :{result}");
        }
    }
}
