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

namespace Rx3
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
       public IDisposable OutputToConsole<T>(IObservable<T> sequence)
        {
            return sequence.Subscribe(obj => WriteLine($"{obj}"), ex => WriteLine($"Error:{ex.Message}"), () => WriteLine("Completed"));
        }
        [Fact]
        public void MainTest()
        {
            WriteLine("Subject");
            var subject = new Subject<string>();
            subject.OnNext("A");
            using (var subscription=OutputToConsole(subject))
            {
                subject.OnNext("B");
                subject.OnNext("C");
                subject.OnNext("D");
                subject.OnCompleted();
                subject.OnNext("Will not be printed out");
            }
            WriteLine("ReplaySubject");
            var replaySubject = new ReplaySubject<string>();
            replaySubject.OnNext("A");
            using(var subscription=OutputToConsole(replaySubject))
            {
                replaySubject.OnNext("B");
                replaySubject.OnNext("C");
                replaySubject.OnNext("D");
                replaySubject.OnCompleted();
            }
            WriteLine("Buffered ReplaySubject");
            var bufferedSubject = new ReplaySubject<string>(2);
            bufferedSubject.OnNext("A");
            bufferedSubject.OnNext("b");
            bufferedSubject.OnNext("c");
            using(var subscription=OutputToConsole(bufferedSubject))
            {
                bufferedSubject.OnNext("D");
                bufferedSubject.OnCompleted();
            }
            WriteLine("Time window ReplaySubject");
            var timeSubject = new ReplaySubject<string>(TimeSpan.FromMilliseconds(200));
            timeSubject.OnNext("A");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            timeSubject.OnNext("B");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            timeSubject.OnNext("c");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            using(var subscription=OutputToConsole(timeSubject))
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(300));
                timeSubject.OnNext("D");
                timeSubject.OnCompleted();
            }
            WriteLine("AsyncSubject");
            var asyncSubject = new AsyncSubject<string>();
            asyncSubject.OnNext("A");
            using (var subscription = OutputToConsole(asyncSubject))
            {
                asyncSubject.OnNext("B");
                asyncSubject.OnNext("C");
                asyncSubject.OnNext("D");
                asyncSubject.OnCompleted();
            }
            WriteLine("BehaviorSubject");
            var behaviorSubject = new BehaviorSubject<string>("Default");
            using (var subscription = OutputToConsole(behaviorSubject))
            {
                behaviorSubject.OnNext("B");
                behaviorSubject.OnNext("C");
                behaviorSubject.OnNext("D");
                behaviorSubject.OnCompleted();
            }
        }
    }
}