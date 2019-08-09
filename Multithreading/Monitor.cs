using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace Monitor类
{
    public class Test
    {
        protected readonly ITestOutputHelper Output;
        object lockObject=new object();
        public void WriteLine(string message)
        {
            Trace.WriteLine(message);
            Output.WriteLine(message);
        }
        public Test(ITestOutputHelper tempOutput)
        {
            Output = tempOutput;
        }
        public void TestMain()
        {
            bool acquiredLock = false;
            try
            {
                Monitor.Enter(lockObject, ref acquiredLock);
                //Code that accesses Resources that
            }
            finally
            {
                if(acquiredLock)
                {
                    Monitor.Exit(lockObject);
                }
            }
        }
    }
}
