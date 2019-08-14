using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rx1
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
            foreach(int i in EnumerableEventSequence())
            {
                WriteLine(i.ToString());
            }
            IObservable<int> o = EnumerableEventSequence().ToObservable();
            using (IDisposable subscription = o.Subscribe(WriteLine))
            {
                WriteLine("");
                WriteLine("IObservable");
            }
            o = EnumerableEventSequence().ToObservable().SubscribeOn(TaskPoolScheduler.Default);
            using (IDisposable subscription = o.Subscribe(WriteLine))
            {
                WriteLine("");
                WriteLine("IObservable async");
                Console.ReadLine();
            }

        }
        
    }
}