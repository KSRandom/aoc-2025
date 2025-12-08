namespace aoc_2025_p7
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: aoc-2025-p7 <filepath>");
                return;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                char[][] grid = new char[lines.Length][];

                for (int i = 0; i < lines.Length; i++)
                {
                    grid[i] = lines[i].ToCharArray();
                }

                Console.WriteLine($"Successfully parsed {grid.Length} rows");
                for (int i = 0; i < grid.Length; i++)
                {
                    Console.WriteLine($"Row {i}: {grid[i].Length} columns");
                }

                // Find the starting position (S)
                int startCol = -1;
                for (int j = 0; j < grid[0].Length; j++)
                {
                    if (grid[0][j] == 'S')
                    {
                        startCol = j;
                        break;
                    }
                }

                if (startCol == -1)
                {
                    Console.WriteLine("Error: 'S' not found in the first row.");
                    return;
                }

                // Process the grid row by row, starting below S
                var activePositions = new List<int> { startCol };

                for (int row = 1; row < grid.Length; row++)
                {
                    var newActivePositions = new List<int>();
                    var nowUnactivePositions = new List<int>();

                    foreach (var col in activePositions)
                    {
                        if (col < 0 || col >= grid[row].Length)
                            continue;

                        if (grid[row][col] == '^')
                        {
                            // Replace ^ with v and add left and right positions
                            grid[row][col] = 'v';
                            newActivePositions.Add(col - 1);
                            nowUnactivePositions.Add(col);
                            newActivePositions.Add(col + 1);

                            grid[row][col - 1] = '|';
                            grid[row][col + 1] = '|';
                        }
                        else
                        {
                            // Draw | and continue down
                            if (grid[row][col] == ' ' || grid[row][col] == '.')
                            {
                                grid[row][col] = '|';
                            }
                            newActivePositions.Add(col);
                        }
                    }

                    // Remove duplicates and sort for next iteration
                    activePositions = newActivePositions.Distinct().OrderBy(x => x).ToList();
                    activePositions.RemoveAll(x => nowUnactivePositions.Contains(x));
                }

                Console.WriteLine("\nGrid contents:");
                for (int i = 0; i < grid.Length; i++)
                {
                    for (int j = 0; j < grid[i].Length; j++)
                    {
                        Console.Write(grid[i][j]);
                    }
                    Console.WriteLine();
                }

                // Count 'v' characters
                int vCount = 0;
                for (int i = 0; i < grid.Length; i++)
                {
                    for (int j = 0; j < grid[i].Length; j++)
                    {
                        if (grid[i][j] == 'v')
                        {
                            vCount++;
                        }
                    }
                }

                Console.WriteLine($"\nTotal 'v' characters: {vCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }
    }
}
