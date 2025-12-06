using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace aoc_2025_p5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: <input file path>");
                return;
            }

            string inputPath = args[0];

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"File not found: {inputPath}");
                return;
            }

            var lines = File.ReadAllLines(inputPath);
            var ranges = new List<(long start, long end)>();
            var numbers = new List<long>();

            // Parse ranges from the first part
            int i = 0;
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]))
            {
                string line = lines[i];
                var parts = line.Split('-');
                if (parts.Length == 2 && long.TryParse(parts[0], out long start) && long.TryParse(parts[1], out long end))
                {
                    ranges.Add((start, end));
                }
                i++;
            }

            // Skip the empty line
            i++;

            // Parse numbers from the second part
            while (i < lines.Length)
            {
                if (long.TryParse(lines[i], out long number))
                {
                    numbers.Add(number);
                }
                i++;
            }

            Console.WriteLine($"Ranges parsed: {ranges.Count}");
            foreach (var range in ranges)
            {
                Console.WriteLine($"  {range.start}-{range.end}");
            }

            Console.WriteLine($"\nNumbers parsed: {numbers.Count}");
            foreach (var num in numbers)
            {
                Console.WriteLine($"  {num}");
            }

            // Merge overlapping ranges
            var mergedRanges = new List<(long start, long end)>();
            
            foreach (var range in ranges.OrderBy(r => r.start))
            {
                if (mergedRanges.Count == 0)
                {
                    mergedRanges.Add(range);
                }
                else
                {
                    var lastRange = mergedRanges[mergedRanges.Count - 1];
                    
                    // Check if current range overlaps with the last merged range
                    if (range.start <= lastRange.end + 1)
                    {
                        // Merge: extend the last range if necessary
                        long newEnd = Math.Max(lastRange.end, range.end);
                        mergedRanges[mergedRanges.Count - 1] = (lastRange.start, newEnd);
                    }
                    else
                    {
                        // No overlap, add as new range
                        mergedRanges.Add(range);
                    }
                }
            }

            Console.WriteLine($"\nMerged ranges: {mergedRanges.Count}");
            foreach (var range in mergedRanges)
            {
                Console.WriteLine($"  {range.start}-{range.end}");
            }

            // Calculate the sum of differences (end - start + 1) for each range
            long sumOfDifferences = 0;
            foreach (var range in mergedRanges)
            {
                long difference = range.end - range.start + 1;
                sumOfDifferences += difference;
                Console.WriteLine($"  Range {range.start}-{range.end}: difference = {difference}");
            }

            Console.WriteLine($"\nSum of differences: {sumOfDifferences}");
        }
    }
}
