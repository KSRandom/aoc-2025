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
                .Where(parts => parts.Length == 2 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
                .Select(parts => new { First = int.Parse(parts[0]), Second = int.Parse(parts[1]) })
                .ToArray();

            Console.WriteLine($"Found {pairs.Length} number pairs:");
            foreach (var pair in pairs)
            {
                Console.WriteLine($"{pair.First}-{pair.Second}");
            }
        }

        public static bool IsInvalidId(int number)
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
