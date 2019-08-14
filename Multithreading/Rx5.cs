using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rx5
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
        public void WriteLine(int message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message.ToString());
        }
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        public IDisposable OutputToConsole<T>(IObservable<T> sequence,int innerLever)
        {
            string delimiter = innerLever == 0 ? string.Empty : new string('-', innerLever * 3);
            return sequence.Subscribe(
                obj => WriteLine($"{delimiter}{obj}"), 
                ex => WriteLine($"Error:{ex.Message}"), 
                () => WriteLine($"{delimiter}Completed"));
        }
        [Fact]
        public void MainTest()
        {
            IObservable<long> sequence = Observable.Interval(TimeSpan.FromMilliseconds(50)).Take(21);
            var evenNumbers = from n in sequence
                              where n % 2 == 0
                              select n;
            var oddNumber = from n in sequence
                            where n % 2 != 0
                            select n;
            var combine = from n in evenNumbers.Concat(oddNumber) select n;
            var nums = (from n in combine where n % 5 == 0 select n).Do(n => WriteLine($"-------Number {n} is processed in Do method"));
            using (var sub = OutputToConsole(sequence, 0)) ;
            using (var sub2 = OutputToConsole(combine, 1)) ;
            using (var sub3 = OutputToConsole(nums, 2))
            {
                WriteLine("Press enter to finish the demo");
                Console.ReadLine();
            }
        }
    }
}
