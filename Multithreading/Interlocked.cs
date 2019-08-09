using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace 执行基本的原子操作
{
    //Interlocked提供了Increment、Decrement和Add等基本数学操作的原子方法，从而帮助我们在编写Counter类时无需使用锁 
    abstract class CounterBase
    {
        public abstract void Increment();
        public abstract void Decrement();
    }
    class Counter:CounterBase
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
    class CounterWithInterlocked : CounterBase
    {
        private int _count;
        public int Count { get { return _count; } }
        public override void Decrement()
        {
            Interlocked.Decrement(ref _count);
        }
        public override void Increment()
        {
            Interlocked.Increment(ref _count);
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
            for(int i=0;i<10000;i++)
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
            var c1 = new CounterWithInterlocked();
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
