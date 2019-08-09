using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Concurrentstack类
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
        public async Task TaskProducer(ConcurrentStack<CustomTask> concurrentStack)
        {
            for (int i = 1; i <= 20; i++)
            {
                await Task.Delay(50);
                var workItem = new CustomTask { Id = i };
                concurrentStack.Push(workItem);
                WriteLine($"Task{workItem.Id} has been posted");
            }
        }
        public async Task TaskProcessor(ConcurrentStack<CustomTask> concurrentStack, string name, CancellationToken token)
        {
            CustomTask workItem;
            bool dequeueSuccesful = false;
            await GetRandomDelay();
            do
            {
                dequeueSuccesful = concurrentStack.TryPop(out workItem);
                if (dequeueSuccesful)
                {
                    WriteLine($"Task {workItem.Id} has been processed by {name}");
                }
            }
            while (!token.IsCancellationRequested);
        }
        public Task GetRandomDelay()
        {
            int delay = new Random(DateTime.Now.Millisecond).Next(1, 500);
            return Task.Delay(delay);
        }
        public async Task RunProgram()
        {
            var taskQueue = new ConcurrentStack<CustomTask>();
            var Cts = new CancellationTokenSource();
            var taskSource = Task.Run(() => TaskProducer(taskQueue));
            Task[] processors = new Task[4];
            for (int i = 1; i <= 4; i++)
            {
                string processorld = i.ToString();
                processors[i - 1] = Task.Run(() => TaskProcessor(taskQueue, $"Processor  {processorld}", Cts.Token));
            }
            await taskSource;
            Cts.CancelAfter(TimeSpan.FromSeconds(2));
            await Task.WhenAll(processors);
        }
        [Fact]
        public void MainTest()
        {
            Task t = RunProgram();
            t.Wait();
        }
    }
    public class CustomTask
    {
        public int Id { get; set; }
    }
}
