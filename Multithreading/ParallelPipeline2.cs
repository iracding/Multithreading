using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;
using Xunit.Abstractions;

namespace ParallelPipeline2
{
    public class Test
    {
        private const int CollectionsNumber = 4;
        private const int Count = 10;
        protected readonly ITestOutputHelper Output;
        object lockObject = new object();
        public void WriteLine(string message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message);
        }
        public void WriteLine(int message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message.ToString());
        }
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        [Fact]
        public async Task MainTest()
        {
            var cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'c')
                {
                    cts.Cancel();
                }
            });
            var inputBlock = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 5, CancellationToken = cts.Token });
            var filter1Block = new TransformBlock<int, decimal>(n =>
               {
                   decimal result = Convert.ToDecimal(n * 0.97);
                   WriteLine($"Filter 1 send {result} to the next stage on thread id {Thread.CurrentThread.ManagedThreadId}");
                   Thread.Sleep(TimeSpan.FromMilliseconds(100));
                   return result;
               }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });
            var filter2Block = new TransformBlock<decimal, string>(n =>
            {
                string result = $"--{n}--";
                WriteLine($"Filter 1 send {result} to the next stage on thread id {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                return result;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });
            var outputBlock = new ActionBlock<string>
            (
                s =>
                {
                    WriteLine($"Filter 1 send {s} to the next stage on thread id {Thread.CurrentThread.ManagedThreadId}");
                }
            , new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token });
            inputBlock.LinkTo(filter1Block, new DataflowLinkOptions { PropagateCompletion = true });
            filter1Block.LinkTo(filter2Block, new DataflowLinkOptions { PropagateCompletion = true });
            filter2Block.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });
            try
            {
                Parallel.For(0, 20, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cts.Token }, i =>
                {
                    WriteLine($"added {i} to source data on thread id {Thread.CurrentThread.ManagedThreadId}");
                    inputBlock.SendAsync(i).GetAwaiter().GetResult();
                });
                inputBlock.Complete();
                await outputBlock.Completion;
                WriteLine("Press enter to exist");
            }
            catch (OperationCanceledException)
            {
                WriteLine("Operation has been canceled! Press Enter to exit.");
            }
        }
    }
}
