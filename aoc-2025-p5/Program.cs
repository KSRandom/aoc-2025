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

            // Check which numbers are in the ranges
            int countInRange = 0;
            foreach (var num in numbers)
            {
                bool inRange = false;
                foreach (var range in ranges)
                {
                    if (num >= range.start && num <= range.end)
                    {
                        inRange = true;
                        break;
                    }
                }
                if (inRange)
                {
                    countInRange++;
                }
            }

            Console.WriteLine($"\nNumbers in range: {countInRange}");
        }
    }
}
