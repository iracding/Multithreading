using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace 使用Mutex类
{
    //工作原理当主程序启动时，定义了一个指定名称的互斥量，设置initialOwner标志为falseo这意
    //味着如果互斥量已经被创建，则允许程序获取该互斥量如果没有获取到互斥量，程序则简单地显示Running.等待直到按下了任何键，然后释放该互斥量并退出 如果再运行同样一个程序，则会在5秒钟内尝试获取互斥量。如果此时在第一个程序中按下了任何键，第二个程序则会开始执行然而，如果保持等待5秒钟，第二个程序将无法获取到该互斥量
    //请注意具名的互斥量是全局的操作系统对象！请务必正确关闭互斥量。最好是使用 using代码块来包裹互斥量对象 该方式可用于在不同的程序中同步线程，可被推广到大量的使用场景中

 public   class Test
    {
        const string Mutexname = "Tesst";
        protected readonly ITestOutputHelper Output;
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        public void WriteLine(string message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message);
        }

        public void TestMain()
        {
            using (var m = new Mutex(false, Mutexname))
            {
                if (!m.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    WriteLine("Second instance is running!");
                }
                else
                {
                    WriteLine("Running!");
                    Console.ReadLine();
                    m.ReleaseMutex();
                }
            }
        }

    }
}
