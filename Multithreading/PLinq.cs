using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PLinq类
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
        public void PrintInfo(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            WriteLine($"{typeName} type was printed on a thread id{Thread.CurrentThread.ManagedThreadId}");
        }
        public  string EmulateProcessing(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            WriteLine($"{typeName} type was printed on a thread id{Thread.CurrentThread.ManagedThreadId}");
            return typeName;
        }
        public IEnumerable<string> GetTypes()
        {
            return from assembly in
                       AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetExportedTypes()
                   where type.Name.StartsWith("Web")
                   select type.Name;
        }
        public void MainTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            var query = from t in GetTypes() select EmulateProcessing(t);
            foreach(string typeName in query)
            {
                PrintInfo(typeName);
            }
            sw.Stop();
            WriteLine(".........");
            WriteLine("Sequential LINQ query.");
            WriteLine($"Time elapsed:{sw.Elapsed}");
            WriteLine("Press ENTER to continue...");
            Console.ReadLine();
            Console.Clear();
            sw.Reset();



        }
    }
}
