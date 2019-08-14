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

namespace Plinq类2
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
        public string EmulateProcessing(string typeName)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(new Random(DateTime.Now.Millisecond).Next(250,350)));
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
            var parallelQuery = from t in GetTypes().AsParallel()
                                select EmulateProcessing(t);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                parallelQuery.WithDegreeOfParallelism(Environment.ProcessorCount)
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .WithMergeOptions(ParallelMergeOptions.Default)
                    .WithCancellation(cts.Token).ForAll(WriteLine);
            }
            catch (OperationCanceledException)
            {
                WriteLine("......");
                WriteLine("Operation has been canceled!");
            }
            WriteLine("....");
            WriteLine("Unordered Pliq query execaution");
            var unorderedQuery = from i in ParallelEnumerable.Range(1,30) select  i;
            foreach(var i in unorderedQuery)
            {
                WriteLine(i.ToString());
            }
            WriteLine(".....");
            WriteLine("Ordered Plinq query execution");
            var orderedQuery = from i in ParallelEnumerable.Range(1, 30).AsOrdered() select i;
            foreach(var i in orderedQuery)
            {
                WriteLine(i.ToString());
            }
        }
    }
}
