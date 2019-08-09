using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Task5
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
        public int TaskMethod(string name, int seconds, CancellationToken token=default)
        {
            WriteLine($"Task{name} is ruuning on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread :{Thread.CurrentThread.IsThreadPoolThread} ");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                if (token.IsCancellationRequested)
                {
                    return -1;
                }
            }
            return 42 * seconds;
        }
        [Fact]
        public void MainTest()
        {
            var firstTask = new Task<int>(() => TaskMethod("First task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second task", 3));
            var whenAllTask = Task.WhenAll(firstTask,secondTask);
            whenAllTask.ContinueWith(t =>
            WriteLine($"The first answer is {t.Result[0]},the second is {t.Result[1]}")
            );
            firstTask.Start();
            secondTask.Start();
            Thread.Sleep(TimeSpan.FromSeconds(4));
            var tasks = new List<Task<int>>();
            for(int i=0;i<4;i++)
            {
                int counter = i;
                var task = new Task<int>(() => TaskMethod($"Task {counter}", counter));
                tasks.Add(task);
                task.Start();
            }
            while(tasks.Count>0)
            {
                var completedTask = Task.WhenAny(tasks).Result;
                tasks.Remove(completedTask);
                WriteLine($"a task has been completed with result {completedTask.Result}");
            }
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
