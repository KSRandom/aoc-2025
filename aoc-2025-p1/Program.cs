using System;
using System.IO;

namespace aoc_2025_p1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: <input file path> <starting number>");
                return;
            }

            string inputPath = args[0];
            if (!int.TryParse(args[1], out int number))
            {
                Console.WriteLine($"Invalid starting number: {args[1]}");
                return;
            }

            int zeroCount = 0;

            if (File.Exists(inputPath))
            {
                foreach (var line in File.ReadLines(inputPath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.Length < 2)
                        continue;

                    char direction = line[0];
                    if (direction != 'L' && direction != 'R')
                    {
                        Console.WriteLine($"Invalid direction in line: {line}");
                        continue;
                    }

                    if (!int.TryParse(line.Substring(1), out int value))
                    {
                        Console.WriteLine($"Invalid number in line: {line}");
                        continue;
                    }

                    // Count full cycles of 100
                    zeroCount += value / 100;

                    value %= 100;

                    if (direction == 'L')
                    {
                        if (number != 0 && number - value < 0 && number - value != 0)
                        {
                            zeroCount++;
                        }
                        number = ((number - value) % 100 + 100) % 100;
                    }
                    else // direction == 'R'
                    {
                        if (number + value > 100)
                        {
                            zeroCount++;
                        }
                        number = (number + value) % 100;
                    }

                    // Count if number is 0
                    if (number == 0)
                    {
                        zeroCount++;
                    }
                }
                Console.WriteLine($"Final number: {number}");
                Console.WriteLine($"Number of times number was 0: {zeroCount}");
            }
            else
            {
                Console.WriteLine($"File not found: {inputPath}");
            }
        }
    }
}
