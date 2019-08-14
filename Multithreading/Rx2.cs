using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rx2
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
        public void WriteLine(int message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message.ToString());
        }
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        public IEnumerable<int> EnumerableEventSequence()
        {
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                yield return i;
            }
        }
        [Fact]
        public void MainTest()
        {
            var observer = new CustomObserver();
            var goodObservable = new CustomSequence(new[] { 1, 2, 3, 4, 5 });
            var badObservable = new CustomSequence(null);
            using (IDisposable subscription=goodObservable.Subscribe(observer))
            {

            }
            using (IDisposable subscription = goodObservable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(observer))
            {
                Thread.Sleep(100);
            }
            using (IDisposable subscription = badObservable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(observer))
            {
                Console.ReadLine();
            }
        }
    }
    public class CustomObserver:IObserver<int>
    {
        public void OnNext(int value)
        {
            Trace.WriteLine($"Next value:{value} ;Thread Id:{Thread.CurrentThread.ManagedThreadId}");
        }
        public void OnError(Exception error)
        {
            Trace.WriteLine($"Error:{error.Message}");
        }
        public void OnCompleted()
        {
            Trace.WriteLine("Completed");
        }
    }
    public class CustomSequence : IObservable<int>
    {
        private readonly IEnumerable<int> _numbers;
        public CustomSequence(IEnumerable<int> numbers)
        {
            _numbers = numbers;
        }
        public IDisposable Subscribe(IObserver<int> observer)
        {
            foreach(var number in _numbers)
            {
                observer.OnNext(number);
            }
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}
