using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace AutoResetEvent类
{
    //类似旋转门一次只允许一个人通过
    //AutoResetEvent类来从一个线程向另一个线程发诺诵知。AutoResetEvent 类可以通知等待的线程有某事件发生。
    //当主程序启动时，定义了两个AutoResetEvent实例。其中一个是从子线程向主线程发信号，另一个实例是从主线程向子线程发信号。我们向AutoResetEvent构造方法传人false, 定义了这两个实例的初始状态为unsignaledo这意味着任何线程调用这两个
    //对象中的任何一个的WaitOne方法将会被阻塞，直到我们调用了set方法如果初始事件状态为true,那么 AutoResetEvent实例的状态为signaled,如果线程调用WaitOne方法则会被立即处理。然后事件状态自动变为unsignaled,所以需要再对该实例调用一次
    //   set方法，以便让其他的线程对该实例调用WaitOne方法从而继续执行 
    //然后我们创建了第二个线程，其会执行第一个操作10秒钟，然后等待从第二个线程发出的信号。该信号意味着第一个操作已经完成。现在第二个线程在等待主线程的信号。我们对主线程做了一些附加工作，并通过调用mainEvent.Set方法发送了一个信号。
    //然后等待从第二个线程发出的另一个信号
    //AutoResetEvent类采用的是内核时间模式，所以等待时间不能太长。使用2．6节中的
    //ManualResetEventslim类更好，因为它使用的是混合模式

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
        private static AutoResetEvent _workerEvent = new AutoResetEvent(false);
        private static AutoResetEvent _mainEvent = new AutoResetEvent(false);
        public void Process(int seconds)
        {
            WriteLine("Starting a long running work.......");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            WriteLine("Work is done!");
            _workerEvent.Set();
            WriteLine("Waiting for a main thread to complete its work!");
            _mainEvent.WaitOne();
            WriteLine("Starting second operation...");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            WriteLine("Wrok is done!");
            _workerEvent.Set();
        }
        [Fact]
        public void MainTest()
        {
            var t = new Thread(() => Process(10));
            t.Start();
            WriteLine("Waiting for another thread to complete work");
            _workerEvent.WaitOne();
            WriteLine("First operation is completed");
            WriteLine("Performing a operation on a main thread");
            Thread.Sleep(TimeSpan.FromSeconds(5));
            _mainEvent.Set();
            WriteLine("Now running the second operation on a second thread");
            _workerEvent.WaitOne();
            WriteLine("Second operation is completed!");
            Console.ReadLine();
        }
    }
}
