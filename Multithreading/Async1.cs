using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;


namespace Async1
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
        public async Task<string> GetInfoAsync(string name)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return $"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}. Is thread pool thread:{Thread.CurrentThread.IsThreadPoolThread}";
        }
        public Task AsyncronyWithTPL()
        {
            Task<string> t = GetInfoAsync("Task 1");
            Task t2 = t.ContinueWith(task => WriteLine(t.Result),TaskContinuationOptions.OnlyOnFaulted);
            Task t3= t.ContinueWith(task => WriteLine(t.Exception.InnerException.ToString()), TaskContinuationOptions.OnlyOnFaulted);
            return Task.WhenAny(t2,t3);
        }
        public async Task AsynchronyWithAwait()
        {
            try
            {
                string result = await GetInfoAsync("Task 2");
                WriteLine(result);
            }
            catch(Exception ex)
            {
                WriteLine(ex.ToString());
            }
        }
        [Fact]
        public void MainTest()
        {
            Task t = AsyncronyWithTPL();
            t.Wait();
            t = AsynchronyWithAwait();
            t.Wait();
        }
    }
}
