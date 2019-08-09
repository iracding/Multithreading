using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ReaderWriterLockSIim类
{
    //ReaderWriterLockSlim来创建一个线程安全的机制，在多线程中对一个集合进行读写操作。ReaderWriterLockSlim代表了一个管理资源访问的锁，允许多个线程同时读取，以及独占写 
    //当主程序启动时，同时运行了三个线程来从字典中读取数据，还有另外两个线程向该字典中写人数据。我们使用ReaderWriterLockSlim类来实现线程安全，该类专为这样的场景而设计0 这里使用两种锁：读锁允许多线程读取数据，写锁在被释放前会阻塞了其他线程的所有操作。获取读锁时还有一个有意思的场景，即从集合中读取数据时，根据当前数据而决定是否获取一个写锁并修改该集合一旦得到写锁，会阻止阅读者读取数据，从而浪费大量的时间，因此获取写锁后集合会处于阻塞状态。为了最小化阻塞浪费的时间，可以使用 EnterUpgradeableReadLock和ExitUpgradeableReadLock方法。先获取读锁后读取数据。如果发现必须修改底层集合，只需使用EnterWriteLock方法升级锁，然后快速执行一次写操作，
    //最后使用ExitWriteLock释放写锁0 在本例中，我们先生成一个随机数。然后获取读锁并检查该数是否存在于字典的键集合中。如果不存在，将读锁更新为写锁然后将该新键加人到字典中。始终使用tyr/finaly代码块来确保在捕获锁后一定会释放锁，这是一项好的实践0 所有的线程都被创建为后台线程。主线程在所有后台线程完成后会等待30秒。

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
        static ReaderWriterLockSlim _rw = new ReaderWriterLockSlim();
        static Dictionary<int, int> _items = new Dictionary<int, int>();
        public void Read()
        {
            WriteLine("Reading contents of a dictionary");
            while (true)
            {
                try
                {
                    _rw.EnterReadLock();
                    foreach (var key in _items.Keys)
                    {
                        WriteLine($"key:{key} value: {_items[key]}");
                    }
                }
                finally
                {
                    _rw.ExitReadLock();
                }
            }

        }
        public void Write(string threadName)
        {
            while (true)
            {
                try
                {
                    int newKey = new Random().Next(250);
                    _rw.EnterUpgradeableReadLock();
                    if (!_items.ContainsKey(newKey))
                    {
                        try
                        {
                            _rw.EnterWriteLock();
                            _items[newKey] = 1;
                            WriteLine($"New key {newKey} is added to a dictionary by a {threadName}");
                        }
                        finally
                        {
                            _rw.ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    _rw.ExitUpgradeableReadLock();
                }
            }
        }
        [Fact]
        public void MainTest()
        {
            new Thread(() => Read()) { IsBackground = true }.Start();
            new Thread(() => Read()) { IsBackground = true }.Start();
            new Thread(() => Read()) { IsBackground = true }.Start();
            new Thread(() => Write("Thread 1")) { IsBackground = true }.Start();
            new Thread(() => Write("Thread 2")) { IsBackground = true }.Start();
            Console.WriteLine();
        }
    }
}
