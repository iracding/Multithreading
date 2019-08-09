using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BlockingCollection类
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
        public async Task TaskProducer(BlockingCollection<CustomTask> collection)
        {
            for (int i = 1; i <= 20; i++)
            {
                await Task.Delay(50);
                var workItem = new CustomTask { Id = i };
                collection.Add(workItem);
                WriteLine($"Task{workItem.Id} has been posted");
            }
            collection.CompleteAdding();
        }
        public async Task TaskProcessor(BlockingCollection<CustomTask> collection, string name)
        {
            await GetRandomDelay();
           foreach(CustomTask item in collection.GetConsumingEnumerable())
            {
                WriteLine($"Task {item.Id} has been processed by {name}");
                await GetRandomDelay();
            }
                
        }
        public Task GetRandomDelay()
        {
            int delay = new Random(DateTime.Now.Millisecond).Next(1, 500);
            return Task.Delay(delay);
        }
        public async Task RunProgram(IProducerConsumerCollection<CustomTask> collection=null)
        {
            var taskCollection = new BlockingCollection<CustomTask>();
            if (null != collection)
            {
                taskCollection = new BlockingCollection<CustomTask>(collection);
            }
            var taskSource = Task.Run(() => TaskProducer(taskCollection));
            Task[] processors = new Task[4];
            for (int i = 1; i <= 4; i++)
            {
                string processorld = "Processor "+i.ToString();
                processors[i - 1] = Task.Run(() => TaskProcessor(taskCollection, processorld));
            }
            await taskSource;
            await Task.WhenAll(processors);
        }
        [Fact]
        public void MainTest()
        {
            WriteLine("Using a queue inside of BlockingCollection");

            Task t = RunProgram();
            t.Wait();
            WriteLine("Using a Stack inside of BlockingCollection");
            t = RunProgram(new ConcurrentStack<CustomTask>());
            t.Wait();
        }
    }
    public class CustomTask
    {
        public int Id { get; set; }
    }
}
