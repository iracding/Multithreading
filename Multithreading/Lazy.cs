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

namespace Lazy
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
        public IDisposable OutputToConsole<T>(IObservable<T> sequence, int innerLever)
        {
            string delimiter = innerLever == 0 ? string.Empty : new string('-', innerLever * 3);
            return sequence.Subscribe(
                obj => WriteLine($"{delimiter}{obj}"),
                ex => WriteLine($"Error:{ex.Message}"),
                () => WriteLine($"{delimiter}Completed"));
        }
        public void Worker(IHasValue state)
        {
            WriteLine($"Worker runs on thread {Thread.CurrentThread.ManagedThreadId}");
            WriteLine($"State value:{state.Value.Text}");
        }
        public ValueToAccess Compute()
        {
            Trace.WriteLine($"The value is being constructed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }
        public void Worker(Lazy<ValueToAccess> state)
        {
            WriteLine($"Worker runs on thread id{Thread.CurrentThread.ManagedThreadId}");
            WriteLine($"State value:{state.Value.Text}");
        }
        public async Task ProcessAsyncchronously()
        {
            var unsafeState = new UnsafeState();
            Task[] tasks = new Task[4];
            for(int i=0;i<4;i++)
            {
                tasks[i] = Task.Run(() => Worker(unsafeState));
            }
            await Task.WhenAll(tasks);
            WriteLine("...............");
            var firstState = new DoubleCheckedLocking();
            for (int i = 0; i<4;i++)
            {
                tasks[i] = Task.Run(() => Worker(firstState));
            }
            await Task.WhenAll(tasks);
            WriteLine("...............");
            var secondState = new BCLDoubleChecked();
            for(int i=0;i<4;i++)
            {
                tasks[i] = Task.Run(() => Worker(secondState));
            }
            
            await Task.WhenAll(tasks);
            WriteLine("...............");
            var thirdState = new Lazy<ValueToAccess>(Compute);
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(thirdState));
            }

            await Task.WhenAll(tasks);
            WriteLine("...............");

            var fourthState = new BCLThreadSafeFactory();
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => Worker(fourthState));
            }

            await Task.WhenAll(tasks);
            WriteLine("...............");
        }
        [Fact]
        public void MainTest()
        {
            var t = ProcessAsyncchronously();
            t.GetAwaiter().GetResult();
            WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
        
    }
    public class ValueToAccess
    {
        private readonly string _text;
        public ValueToAccess(string text)
        {
            _text = text;
        }
        public string Text
        {
            get { return _text; }
        }
    }
    public interface IHasValue
    {
        ValueToAccess Value { get; }
    }
    public class UnsafeState:IHasValue
    {
        private ValueToAccess _value;
        public ValueToAccess Value
        {
            get
            {
                if(_value==null)
                {
                    _value = Compute();
                }
                return _value;
            }
        }
        public ValueToAccess Compute()
        {
            Trace.WriteLine($"The value is being constructed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }
    }
    public class DoubleCheckedLocking : IHasValue
    {
        private object _syncRoot = new object();
        private volatile ValueToAccess _value;
        public ValueToAccess Value
        {
            get
            {
                if (_value == null)
                {
                    lock(_syncRoot)
                    {
                        if (_value == null) _value = Compute();
                    }
                    
                }
                return _value;
            }
        }
        public ValueToAccess Compute()
        {
            Trace.WriteLine($"The value is being constructed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }
    }
    public class BCLDoubleChecked : IHasValue
    {
        private object _syncRoot = new object();
        private  ValueToAccess _value;
        private bool _initialized = false;
        public ValueToAccess Value
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _value, ref _initialized, ref _syncRoot, Compute);
            }
        }
        public ValueToAccess Compute()
        {
            Trace.WriteLine($"The value is being constructed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }
    }
    public class BCLThreadSafeFactory : IHasValue
    {
        private ValueToAccess _value;
        public ValueToAccess Value
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _value, Compute);
            }
        }
        public ValueToAccess Compute()
        {
            Trace.WriteLine($"The value is being constructed on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            return new ValueToAccess($"Constructed on thread id {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
