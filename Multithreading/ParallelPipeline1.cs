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
using Xunit;
using Xunit.Abstractions;

namespace ParallelPipeline1
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
        public void MainTest()
        {
            var cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                if (Console.ReadKey().KeyChar == 'C')
                    cts.Cancel();
            });
            var sourceArrays = new BlockingCollection<int>[CollectionsNumber];
            for (int i = 0; i < sourceArrays.Length; i++)
            {
                sourceArrays[i] = new BlockingCollection<int>(Count);
            }
            var filter1 = new PipelineWorker<int, decimal>(sourceArrays, (n) => Convert.ToDecimal(n * 0.97), cts.Token, "filter1");
            var filter2 = new PipelineWorker<decimal, string>(filter1.Output, (s) => string.Format("--{0}--", s), cts.Token, "filter2");
            var filter3 = new PipelineWorker<string, string>(filter2.Output, (s) => WriteLine($"The final result is {s} on thread id {Thread.CurrentThread.ManagedThreadId}"), cts.Token, "filter3");
            try
            {
                Parallel.Invoke(() =>
                {
                    Parallel.For(0, sourceArrays.Length * Count, (j, state) =>
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            state.Stop();
                        }
                        int k = BlockingCollection<int>.AddToAny(sourceArrays, j);
                        if (k >= 0)
                        {
                            WriteLine($"added{j} to souce Data on thread id  {Thread.CurrentThread.ManagedThreadId}");
                            Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        }
                    });
                    foreach (var arr in sourceArrays)
                    {
                        arr.CompleteAdding();
                    }
                },
             () => filter1.Run(),
            () => filter2.Run(),
            () => filter3.Run()
            );

            }
            catch (AggregateException ae)
            {
              foreach(var ex in ae.InnerExceptions)
                {
                    WriteLine(ex.Message + ex.StackTrace.ToString());
                }
            }
            if(cts.Token.IsCancellationRequested)
            {
                WriteLine($"Operation has been canceled! Press Enter to exit.");
            }
            else
            {
                WriteLine("Press Enter to exit.");
            }
            //Console.ReadLine();
        }

    }
    public class PipelineWorker<TInput, TOutput>
    {
        private const int CollectionsNumber = 4;
        private const int Count = 10;
        Func<TInput, TOutput> _processor = null;
        Action<TInput> _outputProcessor = null;
        BlockingCollection<TInput>[] _input;
        CancellationToken _token;
        public BlockingCollection<TOutput>[] Output { get; private set; }
        public string Name { get; private set; }
        public PipelineWorker(BlockingCollection<TInput>[] input, Func<TInput, TOutput> processor, CancellationToken token, string name)
        {
            _input = input;
            Output = new BlockingCollection<TOutput>[_input.Length];
            for (int i = 0; i < Output.Length; i++)
            {
                Output[i] = null == input[i] ? null : new BlockingCollection<TOutput>(Count);
            }
            _processor = processor;
            _token = token;
            Name = name;
        }
        public PipelineWorker(BlockingCollection<TInput>[] input, Action<TInput> renderer, CancellationToken token, string name)
        {
            _input = input;
            _outputProcessor = renderer;
            _token = token;
            Name = name;
            Output = null;
        }
        public void Run()
        {
            Trace.WriteLine($"{this.Name} is running");
            while (!_input.All(bc => bc.IsCompleted) && !_token.IsCancellationRequested)
            {
                TInput receivedItem;
                int i = BlockingCollection<TInput>.TryTakeFromAny(_input, out receivedItem, 50, _token);
                if (i >= 0)
                {
                    if (Output != null)
                    {
                        TOutput outputItem = _processor(receivedItem);
                        BlockingCollection<TOutput>.AddToAny(Output, outputItem);
                        Trace.WriteLine($"{Name} sent {outputItem} tp next,on thread id {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    }
                    else
                    {
                        _outputProcessor(receivedItem);
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                }
            }
            if (Output != null)
            {
                foreach (var bc in Output) bc.CompleteAdding();
            }
        }

    }
}