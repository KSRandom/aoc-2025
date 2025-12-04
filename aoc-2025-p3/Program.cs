using System;
using System.IO;

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
                if (line.Length < 2)
                    continue;

                int largestNumber = -1;
                int largestIndex = -1;

                // Find the largest digit and its first index (excluding last character)
                for (int i = 0; i < line.Length - 1; i++)
                {
                    if (char.IsDigit(line[i]))
                    {
                        int digit = int.Parse(line[i].ToString());
                        if (digit > largestNumber)
                        {
                            largestNumber = digit;
                            largestIndex = i;
                        }
                    }
                }

                // Find the largest digit after the first largest digit
                int largestAfter = -1;
                if (largestIndex != -1)
                {
                    for (int i = largestIndex + 1; i < line.Length; i++)
                    {
                        if (char.IsDigit(line[i]))
                        {
                            int digit = int.Parse(line[i].ToString());
                            if (digit > largestAfter)
                                largestAfter = digit;
                        }
                    }
                }

                // Concatenate the two largest values
                string concatenated = $"{largestNumber}{largestAfter}";
                if (long.TryParse(concatenated, out long value))
                {
                    sum += value;
                }
            }

            Console.WriteLine($"Sum of all concatenated values: {sum}");
        }
    }
}
