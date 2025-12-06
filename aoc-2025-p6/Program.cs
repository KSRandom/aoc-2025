using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            
            if (lines.Length == 0)
            {
                Console.WriteLine("File is empty");
                return;
            }

            // Parse all lines into a 2D character array
            int rows = lines.Length;
            int cols = lines.Max(line => line.Length);
            char[,] grid = new char[rows, cols];

            // Fill the grid with characters (including spaces)
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (j < lines[i].Length)
                    {
                        grid[i, j] = lines[i][j];
                    }
                    else
                    {
                        grid[i, j] = ' '; // Fill with spaces if line is shorter
                    }
                }
            }

            // Walk down each column from right to left and extract numbers/operators
            long totalSum = 0;
            var numberList = new List<long>();

            for (int col = cols - 1; col >= 0; col--)
            {
                string columnValue = "";
                
                // Walk down the column from top to bottom, excluding the last row
                for (int row = 0; row < rows - 1; row++)
                {
                    if (grid[row, col] != ' ')
                    {
                        columnValue += grid[row, col];
                    }
                }
                // Add number to our list
                // Add the number from this column if it exists
                if (long.TryParse(columnValue, out long number))
                {
                    numberList.Add(number);
                    Console.WriteLine($"Column {col}: {number}");
                }

                // Check the last row for operator
                string operatorValue = grid[rows - 1, col].ToString().Trim();

                if (operatorValue == "+" || operatorValue == "*")
                {
                    // We hit an operator, perform the operation on our list
                    long result = 0;
                    
                    if (operatorValue == "+")
                    {
                        result = numberList.Sum();
                    }
                    else if (operatorValue == "*")
                    {
                        result = 1;
                        foreach (var num in numberList)
                        {
                            result *= num;
                        }
                    }

                    totalSum += result;
                    Console.WriteLine($"Column {col}: Operator '{operatorValue}' - Calculated {operatorValue} on {string.Join(", ", numberList)} = {result}");

                    // Reset for next operator
                    numberList.Clear();
                }
            }

            Console.WriteLine($"\nFinal total sum: {totalSum}");
        }
    }
}
