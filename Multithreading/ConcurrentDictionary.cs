using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
namespace ConcurrentDictionary类
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
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        [Fact]
        public void MainTest()
        {
            var concurrentDictionary = new ConcurrentDictionary<int, string>();
            var dictionary = new Dictionary<int, string>();
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000000; i++)
            {
                lock (dictionary)
                {
                    dictionary[i] = i.ToString();
                }
            }
            sw.Stop();
            WriteLine($"Writing to dictionary with a lock:  {sw.Elapsed}");
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                concurrentDictionary[i] = i.ToString();
            }
            sw.Stop();
            WriteLine($"Writing to dictionary with a lock:  {sw.Elapsed}");
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                lock (dictionary)
                {
                    var item=dictionary[i];
                }
            }
            sw.Stop();
            WriteLine($"Reading from dictionary with a lock ·  {sw. Elapsed}");
            sw.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                var item = concurrentDictionary[i];
            }
            sw.Stop();
            WriteLine($"Reading from concurrentDictionary with a lock ·  {sw.Elapsed}");
        }
    }
}
