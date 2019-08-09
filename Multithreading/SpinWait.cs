using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace SpinWait类
{
    //我们介绍了Spinwait,它是一个混合同步构造，被设计为使用用户模式等待一段时间，然后切换到内核模式以节省
    //当主程序启动时，定义了一个线程，将执行一个无止境的循环，直到20毫秒后主线程设置iscompleted变量为trueo我们可以试验运行该周期为20一30秒，通过Windows任务管理器测量CPU的负载情况。
    //取决于CPU内核数量，任务管理器将显示一个显著的处理时间0 我们使用volatile关键字来声明iscompleted静态字段。volatile关键字指出一个字段可能会被同时执行的多个线程修改。
    //声明为volatile的字段不会被编译器和处理器优化为只能被单个线程访问。这确保了该字段总是最新的值 然后我们使用了Spinwait版本，用于在每个迭代打印一个特殊标志位来显示线程是否切换为阻塞状态。
    //运行该线程5毫秒来查看结果。刚开始，Spinwait尝试使用用户模式，在9 个迭代后，开始切换线程为阻塞状态。如果尝试测量该版本的CPU负载，在Windows任务管理器将不会看到任何CPU的使用 
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
        static volatile bool _isCompleted = false;
        public void UserModeWait()
        {
            while (!_isCompleted)
            {
                WriteLine(".");
            }
            WriteLine("Waiting is complete");
        }
        public void HybridSpinWait()
        {
            var w = new SpinWait();
            while (!_isCompleted)
            {
                w.SpinOnce();
                WriteLine(w.NextSpinWillYield.ToString());
            }
            WriteLine("Waiting is complete");
        }
        [Fact]
        public void MainTest()
        {
            var t1 = new Thread(() => UserModeWait());
            var t2 = new Thread(() => HybridSpinWait());
            WriteLine("Running user mode waiting");
            t1.Start();
            Thread.Sleep(20);
            _isCompleted = true;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            _isCompleted = false;
            WriteLine("Running hybrid SpinWait construct waiting ");
            t2.Start();
            Thread.Sleep(5);
            _isCompleted = true;
        }
    }
}
