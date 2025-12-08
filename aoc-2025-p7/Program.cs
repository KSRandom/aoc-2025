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

                // Handle paths going off the bottom edge
                for (int col = 0; col < grid[grid.Length - 1].Length; col++)
                {
                    if (grid[grid.Length - 1][col] == '|')
                    {
                        grid[grid.Length - 1][col] = 'E';
                    }
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
                int[] vCountByRow = new int[grid.Length];
                for (int i = 0; i < grid.Length; i++)
                {
                    for (int j = 0; j < grid[i].Length; j++)
                    {
                        if (grid[i][j] == 'v')
                        {
                            vCount++;
                            vCountByRow[i]++;
                        }
                    }
                }

                Console.WriteLine($"\nTotal 'v' characters: {vCount}, Max in a row: {vCountByRow.Max()}");

                // Count paths into each node
                // Track nodes by row and their positions
                var nodesByRow = new Dictionary<int, Dictionary<int, string>>();
                nodesByRow[0] = new Dictionary<int, string> { { startCol, "S" } };

                // Collect V nodes by row and position
                for (int i = 0; i < grid.Length; i++)
                {
                    if (!nodesByRow.ContainsKey(i))
                        nodesByRow[i] = new Dictionary<int, string>();

                    for (int j = 0; j < grid[i].Length; j++)
                    {
                        if (grid[i][j] == 'v')
                        {
                            nodesByRow[i][j] = $"V{i}_{j}";
                        }
                        else if (grid[i][j] == 'E')
                        {
                            nodesByRow[i][j] = $"E{j}";
                        }
                    }
                }

                // Count paths into each node
                var pathCounts = new Dictionary<string, long>();
                pathCounts["S"] = 1;

                // Process each row from top to bottom
                for (int row = 0; row < grid.Length; row++)
                {
                    // For each node in this row, count paths to nodes in the next row
                    foreach (var nodeEntry in nodesByRow[row])
                    {
                        int col = nodeEntry.Key;
                        string nodeId = nodeEntry.Value;
                        long pathsToThisNode = pathCounts[nodeId];

                        // Find where paths go from this node
                        // Check below (if it's a | path)
                        if (row + 1 < grid.Length && grid[row + 1][col] == '|')
                        {
                            // Trace down to find the next node
                            for (int nextRow = row + 1; nextRow < grid.Length; nextRow++)
                            {
                                if (grid[nextRow][col] == 'v' || grid[nextRow][col] == 'E')
                                {
                                    string nextNodeId = nodesByRow[nextRow][col];
                                    if (!pathCounts.ContainsKey(nextNodeId))
                                        pathCounts[nextNodeId] = 0;
                                    pathCounts[nextNodeId] += pathsToThisNode;
                                    break;
                                }
                            }
                        }

                        // Check left and right branches (if this node is a 'v')
                        if (grid[row][col] == 'v')
                        {
                            // Check left
                            if (col > 0 && grid[row][col - 1] == '|')
                            {
                                for (int nextRow = row + 1; nextRow < grid.Length; nextRow++)
                                {
                                    if (grid[nextRow][col - 1] == 'v' || grid[nextRow][col - 1] == 'E')
                                    {
                                        string nextNodeId = nodesByRow[nextRow][col - 1];
                                        if (!pathCounts.ContainsKey(nextNodeId))
                                            pathCounts[nextNodeId] = 0;
                                        pathCounts[nextNodeId] += pathsToThisNode;
                                        break;
                                    }
                                }
                            }

                            // Check right
                            if (col + 1 < grid[row].Length && grid[row][col + 1] == '|')
                            {
                                for (int nextRow = row + 1; nextRow < grid.Length; nextRow++)
                                {
                                    if (grid[nextRow][col + 1] == 'v' || grid[nextRow][col + 1] == 'E')
                                    {
                                        string nextNodeId = nodesByRow[nextRow][col + 1];
                                        if (!pathCounts.ContainsKey(nextNodeId))
                                            pathCounts[nextNodeId] = 0;
                                        pathCounts[nextNodeId] += pathsToThisNode;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // Display path counts by row
                Console.WriteLine("\n\nPath counts by row:");
                foreach (var row in nodesByRow.OrderBy(kvp => kvp.Key))
                {
                    Console.WriteLine($"Row {row.Key}:");
                    foreach (var node in row.Value.OrderBy(kvp => kvp.Key))
                    {
                        string nodeId = node.Value;
                        long count = pathCounts.ContainsKey(nodeId) ? pathCounts[nodeId] : 0;
                        Console.WriteLine($"  {nodeId}: {count} paths");
                    }
                }

                // Display total paths to E
                long totalPaths = 0;
                foreach (var entry in pathCounts)
                {
                    if (entry.Key.StartsWith("E"))
                    {
                        totalPaths += entry.Value;
                    }
                }
                Console.WriteLine($"\n\nTotal paths to E: {totalPaths}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }
    }
}
