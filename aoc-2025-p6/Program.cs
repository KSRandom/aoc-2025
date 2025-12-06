using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace aoc_2025_p6
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
            
            if (lines.Length < 2)
            {
                Console.WriteLine("File must have at least 2 lines (data and operator line)");
                return;
            }

            // Parse columns of numbers from all lines except the last
            var columns = new List<List<long>>();
            
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var parts = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                for (int j = 0; j < parts.Length; j++)
                {
                    if (long.TryParse(parts[j], out long number))
                    {
                        // Ensure we have enough columns
                        while (columns.Count <= j)
                        {
                            columns.Add(new List<long>());
                        }
                        columns[j].Add(number);
                    }
                }
            }

            // Parse the operator line (last line)
            var operators = lines[lines.Length - 1].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            Console.WriteLine($"Columns parsed: {columns.Count}");
            for (int i = 0; i < columns.Count; i++)
            {
                Console.WriteLine($"  Column {i}: {string.Join(", ", columns[i])} - Operator: {(i < operators.Count ? operators[i] : "N/A")}");
            }

            // Perform operations on each column based on its operator
            long totalSum = 0;

            for (int i = 0; i < columns.Count; i++)
            {
                string op = i < operators.Count ? operators[i] : "";
                long result = 0;

                if (op == "+")
                {
                    // Add all numbers in the column
                    result = columns[i].Sum();
                }
                else if (op == "*")
                {
                    // Multiply all numbers in the column
                    result = 1;
                    foreach (var num in columns[i])
                    {
                        result *= num;
                    }
                }

                totalSum += result;
                Console.WriteLine($"Column {i} ({op}): {result}");
            }

            Console.WriteLine($"\nTotal sum of all results: {totalSum}");
        }
    }
}
