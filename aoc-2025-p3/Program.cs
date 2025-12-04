using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace aoc_2025_p3
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

            long sum = 0;

            foreach (var line in File.ReadLines(inputPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Extract all digit characters from the line in order
                var digits = new LinkedList<char>();
                
                foreach (char c in line)
                {
                    if (char.IsDigit(c))
                    {
                        digits.AddLast(c);
                    }
                }

                // Find the largest digit that is at least 12 characters before the end
                int cutoffIndex = digits.Count - 12;
                char largestDigit = '0';
                int largestIndex = -1;
                
                var node = digits.First;
                int index = 0;
                while (node != null && index <= cutoffIndex)
                {
                    if (node.Value > largestDigit)
                    {
                        largestDigit = node.Value;
                        largestIndex = index;
                    }
                    node = node.Next;
                    index++;
                }

                // Remove all digits before the largest digit found
                if (largestIndex >= 0)
                {
                    node = digits.First;
                    index = 0;
                    while (node != null && index <= largestIndex)
                    {
                        var nextNode = node.Next;
                        if (index < largestIndex)
                        {
                            digits.Remove(node);
                        }
                        node = nextNode;
                        index++;
                    }
                }

                // Remove earliest lowest digits until exactly 12 remain
                char currentValue = '1';
                while (digits.Count > 12)
                {
                    node = digits.First;
                    while (node != null && digits.Count > 12)
                    {
                        var nextNode = node.Next;
                        if (node.Value == currentValue)
                        {
                            digits.Remove(node);
                        }
                        node = nextNode;
                    }
                    currentValue++;
                }

                // Concatenate remaining digits and convert to long
                string concatenated = string.Concat(digits);
                if (long.TryParse(concatenated, out long value))
                {
                    Console.WriteLine($"Line: {line} -> Remaining: {concatenated} -> Value: {value}");
                    sum += value;
                }
            }

            Console.WriteLine($"Final sum: {sum}");
        }
    }
}

// 167502486318468 is too low