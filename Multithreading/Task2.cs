using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Task2
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
            var firstTask = new Task<int>(() => TaskMethod("First Task", 3));
            var secondTask = new Task<int>(() => TaskMethod("Second Task", 2));
            firstTask.ContinueWith(
                t => WriteLine($"The first answer is{t.Result}. Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}")
            , TaskContinuationOptions.OnlyOnRanToCompletion
                );
            firstTask.Start();
            secondTask.Start();
            Thread.Sleep(TimeSpan.FromSeconds(4));
            Task continuation = secondTask.ContinueWith(
                t => WriteLine($"The second answer is{t.Result}. Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}")
            , TaskContinuationOptions.ExecuteSynchronously
                );
            continuation.GetAwaiter().OnCompleted(
                () => WriteLine($"Thread id {Thread.CurrentThread.ManagedThreadId}, is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}")
                );
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Console.WriteLine();
            
            firstTask = new Task<int>(() =>
              {
                  var innerTask = Task.Factory.StartNew(() => TaskMethod("Second Task", 5), TaskCreationOptions.AttachedToParent);
                  innerTask.ContinueWith(t => TaskMethod("Third task", 2), TaskContinuationOptions.AttachedToParent);
                  return TaskMethod("First Task", 2);
              });
            firstTask.Start();
            while(!firstTask.IsCompleted)
            {
                WriteLine(firstTask.Status.ToString());
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            WriteLine(firstTask.Status.ToString());
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
    }
}
