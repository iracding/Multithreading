using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
namespace ConcurrentBag类
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
        private Dictionary<string, string[]> _contentEmulation = new Dictionary<string, string[]>(); 
        public async Task RunProgram()
        {
            var bag = new ConcurrentBag<CrawlingTask>();
            string[] urls = new[] { "http://google.com", "http://microsoft.com", "http://facebook.com", "http://twitter.com" };
            var crawlers = new Task[4];
            for(int i=1;i<=4;i++)
            {
                string crawlerName = $"Crawler{i.ToString()}";
                bag.Add(new CrawlingTask { UrlToCrawl=urls[i-1],ProucerName="root"});
                crawlers[i - 1] = Task.Run(() => Crawl(bag, crawlerName));
            }
        }
        public async Task Crawl(ConcurrentBag<CrawlingTask> bag,string crawlerName)
        {
            CrawlingTask task;
            while(bag.TryTake(out task))
            {
                IEnumerable<string> urls = await GetLinksFromContent(task);
                if(urls!=null)
                {
                    foreach(var url in urls)
                    {
                        var t = new CrawlingTask
                        {
                            UrlToCrawl = url,
                            ProucerName = crawlerName
                        };
                        bag.Add(t);
                    }
                   
                }
            }
        }
        public void CreateLinks()
        {
            _contentEmulation["http://microsoft.com/"] = new[] { "http://microsoft.com/a.html", "http://microsoft.com/b.html" };
            _contentEmulation["http://microsoft.com/a.html"] = new[] { "http://microsoft.com/c.html", "http://microsoft.com/d.html" };
            _contentEmulation["http://microsoft.com/b.html"] = new[] { "http://microsoft.com/e.html", "http://microsoft.com/f.html" };
            _contentEmulation["http://google.com/"] = new[] { "http://google.com/a.html", "http://google.com/b.html" };
            _contentEmulation["http://google.com/a.html"] = new[] { "http://google.com/c.html", "http://google.com/d.html" };
            _contentEmulation["http://google.com/b.html"] = new[] { "http://google.com/e.html", "http://google.com/f.html" };
            _contentEmulation["http://google.com/c.html"] = new[] { "http://google.com/h.html", "http://google.com/i.html" };
            _contentEmulation["http://facebook.com/"] = new[] { "http://facebook.com/a.html", "http://facebook.com/b.html" };
            _contentEmulation["http://facebook.com/a.html"] = new[] { "http://facebook.com/c.html", "http://facebook.com/d.html" };
            _contentEmulation["http://facebook.com/b.html"] = new[] { "http://facebook.com/e.html", "http://facebook.com/f.html" };
            _contentEmulation["http://twitter.com/"] = new[] { "http://twitter.com/a.html", "http://twitter.com/b.html" };
            _contentEmulation["http://twitter.com/a.html"] = new[] { "http://twitter.com/c.html", "http://twitter.com/d.html" };
            _contentEmulation["http://twitter.com/b.html"] = new[] { "http://twitter.com/e.html", "http://twitter.com/f.html" };
        }
             
        public Task GetRandomDelay()
        {
            int delay = new Random(DateTime.Now.Millisecond).Next(1, 500);
            return Task.Delay(delay);
        }
        public async Task<IEnumerable<string>> GetLinksFromContent(CrawlingTask task)
        {
            await GetRandomDelay();
            if(_contentEmulation.ContainsKey(task.UrlToCrawl))
            {
                return _contentEmulation[task.UrlToCrawl];
            }
            return null;
        }
        [Fact]
        public void MainTest()
        {
            CreateLinks();
            Task t = RunProgram();
            t.Wait();
        }
    }
    public class CrawlingTask
    {
        public string UrlToCrawl { get; set; }
        public string ProucerName { get; set; }
    }
}
