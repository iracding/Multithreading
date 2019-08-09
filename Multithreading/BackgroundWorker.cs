using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace BackgroundWorker组件
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
        public void Worker_DoWork(object sender,DoWorkEventArgs e)
        {
            WriteLine($"DoWrok thread pool thread id:{Thread.CurrentThread.ManagedThreadId}");
            var bw = (BackgroundWorker)sender;
            for (int i=1;i<=100;i++)
            {
                if(bw.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                if(i%10==0)
                {
                    bw.ReportProgress(i);
                }
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
            e.Result = 42;
        }
        public void Worker_ProgressChanged(object sender,ProgressChangedEventArgs e)
        {
            WriteLine($"{e.ProgressPercentage}% completed. Progress thread pool thread id :{Thread.CurrentThread.ManagedThreadId}");

        }
        public void Worker_Completed(object sender,RunWorkerCompletedEventArgs e)
        {
            WriteLine($"Completed thread pool thread id :{Thread.CurrentThread.ManagedThreadId}");
            if(e.Error!=null)
            {
                WriteLine($"Exception{e.Error.Message} has occured.");
            }
            else if(e.Cancelled)
            {
                WriteLine("Operation has been canceled.");
                
            }
            else
            {
                WriteLine($"The answer is :{e.Result}");
            }
        }
        [Fact]
        public void MainTest()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += Worker_DoWork;
            bw.ProgressChanged += Worker_ProgressChanged;
            bw.RunWorkerCompleted += Worker_Completed;
            bw.RunWorkerAsync();
            WriteLine("Press C to cancel work");
            do
            {
                if (Console.ReadKey(true).KeyChar == 'C')
                {
                    bw.CancelAsync();
                }
            }
            while (bw.IsBusy);
        }
    }
}
