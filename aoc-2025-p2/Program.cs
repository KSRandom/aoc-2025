using System;
using System.IO;
using System.Linq;

namespace aoc_2025_p2
{
    public class Program
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

            string line = File.ReadAllText(inputPath).Trim();
            
            if (string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("Input file is empty");
                return;
            }

            var pairs = line.Split(',')
                .Select(item => item.Trim())
                .Where(item => item.Contains('-'))
                .Select(item => item.Split('-'))
                .Where(parts => parts.Length == 2 && long.TryParse(parts[0], out _) && long.TryParse(parts[1], out _))
                .Select(parts => new { First = long.Parse(parts[0]), Second = long.Parse(parts[1]) })
                .ToArray();

            Console.WriteLine($"Found {pairs.Length} number pairs:");
            
            long sum = 0;
            
            foreach (var pair in pairs)
            {
                Console.WriteLine($"{pair.First}-{pair.Second}");
                
                for (long number = pair.First; number <= pair.Second; number++)
                {
                    if (IsInvalidId(number))
                    {
                        Console.WriteLine($"Invalid Id: {number}");
                        sum += number;
                    }
                }
            }
            
            Console.WriteLine($"Sum of invalid IDs: {sum}");
        }

        public static bool IsInvalidId(long number)
        {
            string str = number.ToString();
            
            if (str.Length % 2 != 0)
                return false;

            int mid = str.Length / 2;
            string firstHalf = str.Substring(0, mid);
            string secondHalf = str.Substring(mid);

            return firstHalf == secondHalf;
        }
    }
}
