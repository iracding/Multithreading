using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Lock类
{
    abstract class CounterBase
    {
        public abstract void Increment();
        public abstract void Decrement();
    }
    class Counter : CounterBase
    {
        private int _count;
        public int Count { get { return _count; } }
        public override void Decrement()
        {
            _count--;
        }
        public override void Increment()
        {
            _count++;
        }
    }
    class CounterWithLock : CounterBase
    {
        private readonly object _syncRoot = new Object();
        private int _count;
        public int Count { get { return _count; } }
        public override void Decrement()
        {
            lock(_syncRoot)
            {
                _count--;
            }
        }
        public override void Increment()
        {
            lock (_syncRoot)
            {
                _count++;
            }
        }
    }
    public class Test
    {
        protected readonly ITestOutputHelper Output;
        public void WriteLine(string message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message);
        }
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        static void TestCounter(CounterBase counter)
        {
            for (int i = 0; i < 10000; i++)
            {
                counter.Increment();
                counter.Decrement();
            }
        }
        [Fact]
        public void MainTest()
        {
            WriteLine("Incorrect counter");
            var c = new Counter();
            var t1 = new Thread(() => TestCounter(c));
            var t2 = new Thread(() => TestCounter(c));
            var t3 = new Thread(() => TestCounter(c));
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();
            WriteLine($"Incorrect counter:{c.Count}");
            WriteLine("'''''''''''''''''''''''''''''''''''''''''");
            var c1 = new CounterWithLock();
            var t11 = new Thread(() => TestCounter(c1));
            var t22 = new Thread(() => TestCounter(c1));
            var t33 = new Thread(() => TestCounter(c1));
            t11.Start();
            t22.Start();
            t33.Start();
            t11.Join();
            t22.Join();
            t33.Join();
            WriteLine($"Correct counter :{c1.Count}");
        }
    }
}
