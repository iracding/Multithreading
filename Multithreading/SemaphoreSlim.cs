using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace 使用SemaphoreSlim类
{
    //当主程序启动时，创建了SemaphoreSlim的一个实例，并在其构造函数中指定允许的并发线程数量。然后启动了6个不同名称和不同初始运行时间的线程0
    //每个线程都尝试获取数据库的访问，但是我们借助于信号系统限制了访问数据库的并发数为4个线程当有4个线程获取了数据库的访问后，其他两个线程需要等待，直到之前线程中的某一个完成工作并调用semaphore.Release方法来发出信号。
    //借助SemaphoreS1im类来限制访问同一个资源的线程数量：
    public class Test
    {
        static SemaphoreSlim _semaphore = new SemaphoreSlim(4);
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
        public void AccessDatabase(string name, int seconds)
        {
            WriteLine($"{name} waits to access a database");
            _semaphore.Wait();
            WriteLine($"{name} was granted an access to a database");
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
            WriteLine($"{name} is completed");
            _semaphore.Release();
        }
        [Fact]
        public void TestMain()
        {
            for (int i = 1; i <= 20; i++)
            {
                string threadName = "Thread " + i;
                int secondsToWait = 2 + 2 * i;
                var t = new Thread(() => AccessDatabase(threadName, secondsToWait));
                t.Start();
                Console.WriteLine();
            }
        }
    }
}
