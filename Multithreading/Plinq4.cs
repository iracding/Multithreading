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

namespace Plinq类4
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
        public ConcurrentDictionary<char,int> AccumulateLettersInfomation(ConcurrentDictionary<char,int> taskToal,string item)
        {
            foreach(var c in item)
            {
                if(taskToal.ContainsKey(c))
                {
                    taskToal[c] = taskToal[c] + 1;
                }
                else
                {
                    taskToal[c] = 1;
                }
            }
            WriteLine($"{item} type was printed on a thread id{Thread.CurrentThread.ManagedThreadId}");
            return taskToal;
        }
        public ConcurrentDictionary<char,int> MergeAccumulators(ConcurrentDictionary<char,int> total,ConcurrentDictionary<char,int> taskTotal)
        {
            foreach(var key in taskTotal.Keys)
            {
                if(total.ContainsKey(key))
                {
                    total[key] = total[key] + taskTotal[key];
                }
                else
                {
                    total[key] = taskTotal[key];
                }
            }
            WriteLine("'''''");
            WriteLine($"Total aggregate value was calculated on a thread id {Thread.CurrentThread.ManagedThreadId}");
            return total;
        }
        public IEnumerable<string> GetTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).SelectMany(a => a.GetExportedTypes());

            return from type in types where type.Name.StartsWith("Web") select type.Name;

        }
        [Fact]
        public void MainTest()
        {
            var parallelQuery = from t in GetTypes().AsParallel() select t;
            var parallelAggregator=parallelQuery.Aggregate(
                 ()=> new ConcurrentDictionary<char,int>(),
                 (taskTotal,item)=>AccumulateLettersInfomation(taskTotal,item),
                (total,taskTotal)=>MergeAccumulators(total,taskTotal),
                total=>total);
            WriteLine("There were the following letters in type names:");
            var orderedKeys=from k in parallelAggregator.Keys orderby parallelAggregator[k] descending
                            select k;
            foreach(var c in orderedKeys)
            {
                WriteLine($"Letter {c} -----{parallelAggregator[c]} times");
            }
        }
    }
}
