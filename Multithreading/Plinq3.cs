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

namespace Plinq类3
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
            Thread.Sleep(TimeSpan.FromMilliseconds(150));
            string temp = typeName.Length % 2 == 0 ? "even" : "odd";
            WriteLine($"{typeName} type was printed on a thread id{Thread.CurrentThread.ManagedThreadId} has {temp} lenghth");
            return typeName;
        }
        public IEnumerable<string> GetTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).SelectMany(a => a.GetExportedTypes());
          
            return from type in types where type.Name.StartsWith("Web") select type.Name;
                   
        }
      
        [Fact]
        public void MainTest()
        {

            var partitioner = new StringPartitioner(GetTypes());
            var parallelQuery = from t in partitioner.AsParallel() select EmulateProcessing(t);
            parallelQuery.ForAll(PrintInfo);
        }
    }
    public class StringPartitioner : Partitioner<string>
    {
        private readonly IEnumerable<string> _data;
        public StringPartitioner(IEnumerable<string> data)
        {
            _data = data;
        }
        public override bool SupportsDynamicPartitions
        {
            get
            {
                return false;
            }
        }
        public override IList<IEnumerator<string>> GetPartitions(int partitionCount)
        {
            var result = new List<IEnumerator<string>>(2);
            result.Add(CreateEnumerator(true));
            result.Add(CreateEnumerator(false));
            return result;
        }
        IEnumerator<string> CreateEnumerator(bool isEven)
        {
            foreach(var d in _data)
            {
                if(!(d.Length%2==0^isEven))
                {
                    yield return d;
                }
            }
        }
    }
}
