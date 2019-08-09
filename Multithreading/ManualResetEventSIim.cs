using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace 使用ManualResetEventSIim类
{
    //ManualResetEventSIim 工作方式有点像人群通过大门，一直保持大门敞开直到Reset
    //，相当于打开了大门从而允许准备好的线程接收信号并继续工作。然而线程3还处于睡眠状态，没有赶上时间。当调用mainEvent.Reset相当于关闭了大门。最后一个线程已经准备好执行，但是不得不等待下一个信号，即要等待好几秒钟 
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
        static ManualResetEventSlim _mainEvent = new ManualResetEventSlim(false);
        public void TravelThroughGates(string threadName,int seconds)
        {
            WriteLine($"{threadName} falls to sleep");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            WriteLine($"{threadName} wait for the gates to open");
            _mainEvent.Wait();
            WriteLine($"{threadName} enters the gates!");
        }
        [Fact]
        public void MainTest()
        {
            var t1 = new Thread(() => TravelThroughGates("Thread 1", 5));
            var t2 = new Thread(() => TravelThroughGates("Thread 2", 6));
            var t3 = new Thread(() => TravelThroughGates("Thread 3", 12));
            t1.Start();
            t2.Start();
            t3.Start();
            Thread.Sleep(TimeSpan.FromSeconds(6));
            WriteLine("The gates are now open!");
            _mainEvent.Set();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            _mainEvent.Reset();
            WriteLine("The gates have been closed!");
            Thread.Sleep(TimeSpan.FromSeconds(10));
            WriteLine("The Gates are now open for the second time!");
            _mainEvent.Set();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            WriteLine("The gates have been closed!");
            _mainEvent.Reset();
            Console.WriteLine();
        }
    }
}
