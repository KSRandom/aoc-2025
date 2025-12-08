namespace aoc_2025_p7
{
    internal class Program
    {
        class Node
        {
            public string Id { get; set; }
            public List<string> Connections { get; set; } = new();
        }

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

                // Build the graph
                var nodes = new Dictionary<string, Node>();
                
                // Create S node
                nodes["S"] = new Node { Id = "S" };

                // Create a single E node
                nodes["E"] = new Node { Id = "E" };

                // Create column nodes for the bottom edge
                var columnNodes = new Dictionary<int, string>();
                int colIndex = 0;
                for (int col = 0; col < grid[grid.Length - 1].Length; col++)
                {
                    if (grid[grid.Length - 1][col] == 'E')
                    {
                        string colId = $"C{colIndex}";
                        nodes[colId] = new Node { Id = colId };
                        columnNodes[col] = colId;
                        // Each column node connects to E
                        nodes[colId].Connections.Add("E");
                        colIndex++;
                    }
                }

                // Create V nodes and track their positions
                var vPositions = new Dictionary<string, (int row, int col)>();
                int vIndex = 0;
                for (int i = 0; i < grid.Length; i++)
                {
                    for (int j = 0; j < grid[i].Length; j++)
                    {
                        if (grid[i][j] == 'v')
                        {
                            string vId = $"V{vIndex}";
                            nodes[vId] = new Node { Id = vId };
                            vPositions[vId] = (i, j);
                            vIndex++;
                        }
                    }
                }

                // Connect S to V nodes that are directly below (via | path)
                for (int col = 0; col < grid[1].Length; col++)
                {
                    if (grid[1][col] == '|' || grid[1][col] == 'v')
                    {
                        // Find which V this connects to
                        var connectedV = vPositions.Where(kvp => kvp.Value.col == col).FirstOrDefault();
                        if (connectedV.Key != null)
                        {
                            nodes["S"].Connections.Add(connectedV.Key);
                        }
                        else if (grid[1][col] == '|')
                        {
                            // Direct connection to V below
                            for (int row = 2; row < grid.Length; row++)
                            {
                                if (grid[row][col] == 'v')
                                {
                                    var v = vPositions.FirstOrDefault(kvp => kvp.Value == (row, col));
                                    if (v.Key != null)
                                    {
                                        nodes["S"].Connections.Add(v.Key);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                // Connect V nodes to other V nodes and column nodes
                foreach (var vEntry in vPositions)
                {
                    string vId = vEntry.Key;
                    int vRow = vEntry.Value.row;
                    int vCol = vEntry.Value.col;

                    // Check left and right branches
                    for (int col = 0; col < grid[vRow].Length; col++)
                    {
                        if ((col == vCol - 1 || col == vCol + 1) && grid[vRow][col] == '|')
                        {
                            // Trace down from this position
                            for (int row = vRow + 1; row < grid.Length; row++)
                            {
                                if (grid[row][col] == 'v')
                                {
                                    var targetV = vPositions.FirstOrDefault(kvp => kvp.Value == (row, col));
                                    if (targetV.Key != null && !nodes[vId].Connections.Contains(targetV.Key))
                                    {
                                        nodes[vId].Connections.Add(targetV.Key);
                                    }
                                    break;
                                }
                                else if (grid[row][col] == 'E')
                                {
                                    if (columnNodes.ContainsKey(col) && !nodes[vId].Connections.Contains(columnNodes[col]))
                                    {
                                        nodes[vId].Connections.Add(columnNodes[col]);
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    // Check center path (if v connects to | below)
                    if (vRow + 1 < grid.Length && grid[vRow + 1][vCol] == '|')
                    {
                        // Trace down from center
                        for (int row = vRow + 2; row < grid.Length; row++)
                        {
                            if (grid[row][vCol] == 'v')
                            {
                                var targetV = vPositions.FirstOrDefault(kvp => kvp.Value == (row, vCol));
                                if (targetV.Key != null && !nodes[vId].Connections.Contains(targetV.Key))
                                {
                                    nodes[vId].Connections.Add(targetV.Key);
                                }
                                break;
                            }
                            else if (grid[row][vCol] == 'E')
                            {
                                if (columnNodes.ContainsKey(vCol) && !nodes[vId].Connections.Contains(columnNodes[vCol]))
                                {
                                    nodes[vId].Connections.Add(columnNodes[vCol]);
                                }
                                break;
                            }
                        }
                    }
                }

                Console.WriteLine("\n\nGraph structure:");
                foreach (var node in nodes.Values.OrderBy(n => n.Id))
                {
                    Console.WriteLine($"{node.Id} -> {string.Join(", ", node.Connections)}");
                }

                // Count all paths from S to E
                int pathCount = CountPaths(nodes, "S", "E", new HashSet<string>());
                Console.WriteLine($"\nTotal paths from S to E: {pathCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        static int CountPaths(Dictionary<string, Node> nodes, string current, string target, HashSet<string> visited)
        {
            if (current == target)
                return 1;

            visited.Add(current);
            int count = 0;

            foreach (var next in nodes[current].Connections)
            {
                if (!visited.Contains(next))
                {
                    count += CountPaths(nodes, next, target, new HashSet<string>(visited));
                }
            }

            return count;
        }
    }
}
