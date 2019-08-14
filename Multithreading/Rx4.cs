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

namespace Rx4
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
            IObservable<int> o = Observable.Return(0);
            using (var sub = OutputToConsole(o))
                WriteLine("............");
            o = Observable.Throw<int>(new Exception());
            using (var sub = OutputToConsole(o)) ;
            WriteLine("............");
            o = Observable.Repeat(42);
            using (var sub = OutputToConsole(o.Take(5)))
                WriteLine("............");
            o = Observable.Range(0, 10);
            using (var sub = OutputToConsole(o.Take(5)))
                WriteLine("............");
            o = Observable.Create<int>(ob =>
              {
                  for (int i = 0; i < 10; i++)
                  {
                      ob.OnNext(i);
                  }
                  return Disposable.Empty;
              });
            using (var sub = OutputToConsole(o)) ;
            WriteLine(".................");
            o = Observable.Generate(
                0,
                i => i < 5,
                i => ++i,
                i => i * 2
                );
            using (var sub = OutputToConsole(o)) ;
            WriteLine("------------------");
            IObservable<long> ol = Observable.Interval(TimeSpan.FromSeconds(1));
            using(var sub=OutputToConsole(ol))
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            WriteLine("-------------");
            ol = Observable.Timer(DateTimeOffset.Now.AddSeconds(2));
            using(var sub=OutputToConsole(ol))
            {
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            WriteLine("--------------");
        }
    }
}
