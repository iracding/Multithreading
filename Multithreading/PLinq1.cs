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

namespace PLinq类1
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
                       AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic)
                   from type in assembly.GetExportedTypes()
                   where type.Name.StartsWith("Web")
                   select type.Name;
        }
        [Fact]
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
            Console.Clear();
            sw.Reset();

            sw.Start();
            var parallelQuery = from t in ParallelEnumerable.AsParallel(GetTypes())
                                select EmulateProcessing(t);
            foreach(string typeName in parallelQuery)
            {
                PrintInfo(typeName);
            }
            sw.Stop();
            WriteLine(".......");
            WriteLine("Parallel Linq query. The results are being merged on singel thread");
            WriteLine($"Time elapsed:{sw.Elapsed}");
            WriteLine("Press ENTER to continue...");

            sw.Start();
            parallelQuery = from t in GetTypes().AsParallel()
                                select EmulateProcessing(t);
            parallelQuery.ForAll(PrintInfo);
            sw.Stop();
            WriteLine(".......");
            WriteLine("Parallel Linq query. The results are being merged on singel parallel");
            WriteLine($"Time elapsed:{sw.Elapsed}");
            WriteLine("Press ENTER to continue...");
            sw.Reset();

            sw.Start();
            query = from t in GetTypes().AsParallel().AsSequential()
                    select EmulateProcessing(t);
            foreach(var typeName in query)
            {
                PrintInfo(typeName);
            }
            sw.Stop();
            sw.Stop();
            WriteLine(".......");
            WriteLine("Parallel Linq query , transformed into sequential .");
            WriteLine($"Time elapsed:{sw.Elapsed}");
            WriteLine("Press ENTER to continue...");
        }
    }
}
