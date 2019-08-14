using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MapReduce
{
    public class Test
    {
        private const int CollectionsNumber = 4;
        private const int Count = 10;
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
        [Fact]
        public void MainTest()
        {
            string textToParse = @"
Call me Ishmael. Some years ago - never mind how long precisely - having little or no money in my purse, 
and nothing particular to interest me on shore, I thought I would sail about a little and see the watery 
part of the world. It is a way I have of driving off the spleen, and regulating the circulation. Whenever 
I find myself growing grim about the mouth; whenever it is a damp, drizzly November in my soul; whenever 
I find myself involuntarily pausing before coffin warehouses, and bringing up the rear of every funeral 
I meet; and especially whenever my hypos get such an upper hand of me, that it requires a strong moral 
principle to prevent me from deliberately stepping into the street, and methodically knocking people's hats off - then, 
I account it high time to get to sea as soon as I can.
― Herman Melville, Moby Dick.
";
            char[] delimiters = Enumerable.Range(0, 256).Select(i => (char)i)
            .Where(c => !char.IsLetterOrDigit(c))
            .ToArray();
            var q = textToParse.Split(delimiters)
            .AsParallel()
            .MapReduce(
                s => s.ToLower().ToCharArray()
            , c => c
            , g => new[] { new { Char = g.Key, Count = g.Count() } })
            .Where(c => char.IsLetterOrDigit(c.Char))
            .OrderByDescending(c => c.Count);

            foreach (var info in q)
            {
                WriteLine($"Character {info.Char} occured in the text {info.Count} times");
            }
            WriteLine(" ------------------------------------------------");
            const string searchPattern = "en";

            var q2 = textToParse.Split(delimiters)
                .AsParallel()
                .Where(s => s.Contains(searchPattern))
                .MapReduce(
                    s => new[] { s }
                    , s => s
                    , g => new[] { new { Word = g.Key, Count = g.Count() } })
                .OrderByDescending(s => s.Count);

            WriteLine($"Words with search pattern '{searchPattern}':");
            foreach (var info in q2)
            {
                WriteLine($"{info.Word} occured in the text {info.Count} times");
            }

            int halfLengthWordIndex = textToParse.IndexOf(' ', textToParse.Length / 2);

            using (var sw = File.CreateText("1.txt"))
            {
                sw.Write(textToParse.Substring(0, halfLengthWordIndex));
            }

            using (var sw = File.CreateText("2.txt"))
            {
                sw.Write(textToParse.Substring(halfLengthWordIndex));
            }

            string[] paths = new[] { ".\\" };

            Console.WriteLine(" ------------------------------------------------");
            var q3 = paths
                .SelectMany(p => Directory.EnumerateFiles(p, "*.txt"))
                .AsParallel()
                .MapReduce(
                    path => File.ReadLines(path).SelectMany(line => line.Trim(delimiters).Split(delimiters))
                    , word => string.IsNullOrWhiteSpace(word) ? '\t' : word.ToLower()[0]
                    , g => new[] { new { FirstLetter = g.Key, Count = g.Count() } })
                .Where(s => char.IsLetterOrDigit(s.FirstLetter))
                .OrderByDescending(s => s.Count);

            WriteLine("Words from text files");

            foreach (var info in q3)
            {
                WriteLine($"Words starting with letter '{info.FirstLetter}' occured in the text {info.Count}");
            }
        }
    }
    static class PLINQExtensions
    {
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
            this ParallelQuery<TSource> source,
            Func<TSource, IEnumerable<TMapped>> map,
            Func<TMapped, TKey> keySelector,
            Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce)
        {
            return source.SelectMany(map)
            .GroupBy(keySelector)
            .SelectMany(reduce);
        }
    }
}
