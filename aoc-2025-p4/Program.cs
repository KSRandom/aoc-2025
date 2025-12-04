using System;
using System.IO;

namespace aoc_2025_p4
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

            // Create 2D array to store grid
            int rows = lines.Length;
            int cols = lines[0].Length;
            char[,] grid = new char[rows, cols];

            // Fill the grid with characters from the file
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    grid[i, j] = lines[i][j];
                }
            }

            Console.WriteLine($"Grid dimensions: {rows} rows x {cols} columns");
            Console.WriteLine("\nGrid:");
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(grid[i, j]);
                }
                Console.WriteLine();
            }
        }
    }
}
