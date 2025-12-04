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

            // Remove rolls of paper iteratively
            int removedCount = 0;
            bool removed = true;

            while (removed)
            {
                removed = false;
                int[] dirX2 = { -1, -1, -1, 0, 0, 1, 1, 1 };
                int[] dirY2 = { -1, 0, 1, -1, 1, -1, 0, 1 };

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (grid[i, j] == '@')
                        {
                            int adjacentCount = 0;
                            for (int d = 0; d < 8; d++)
                            {
                                int newI = i + dirX2[d];
                                int newJ = j + dirY2[d];
                                if (newI >= 0 && newI < rows && newJ >= 0 && newJ < cols && grid[newI, newJ] == '@')
                                {
                                    adjacentCount++;
                                }
                            }

                            if (adjacentCount < 4)
                            {
                                grid[i, j] = '.';
                                removedCount++;
                                removed = true;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Total rolls of paper removed: {removedCount}");
            Console.WriteLine("\nGrid after removal:");
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
