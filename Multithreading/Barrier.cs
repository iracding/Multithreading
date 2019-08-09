using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Barrier类
{
    //工作原理我们创建了Barrier类，指定了我们想要同步两个线程在两个线程中的任何一个调用了bamer.SignalAndWait方法后，会执行一个回调函数来打印出阶段0 每个线程将向Bamer发送两次信号，
    //所以会有两个阶段每次这两个线程调用SignalAndWait方法时，Bamer将执行回调函数。这在多线程迭代运算中非常有用，可以在每个迭代结束前执行一些计算。当最后一个线程调用SignalAndWaIt
    //方法时可以在迭代结束时进行交互 
    //组织多个线程及时在某个时刻碰面。其提供了一个回调函数，每次线程调用了SignalAndWait方法后该回调函数会被执行 
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
        static Barrier _barrier = new Barrier(2, b => Trace.WriteLine($"End of phase {b.CurrentPhaseNumber+1}"));
        public void PlayMusic(string name,string message,int seconds)
        {
            for(int i=1;i<3;i++)
            {
                WriteLine("..........................................");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                WriteLine($"{name} starts to {message}");
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                WriteLine($"{name} finished to {message}");
                _barrier.SignalAndWait();
            }
        }
        [Fact]
        public void MainTest()
        {
            var t1 = new Thread(() =>PlayMusic("the guitarist","play an amazing solo",5));
            var t2= new Thread(() => PlayMusic("the singer", "sing the song", 2));
            t1.Start();
            t2.Start();
            Console.WriteLine();
        }
    }
}
