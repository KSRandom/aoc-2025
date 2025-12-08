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

                // Build the graph
                var nodes = new Dictionary<string, Node>();
                
                // Create S node
                nodes["S"] = new Node { Id = "S" };

                // Create a single E node
                nodes["E"] = new Node { Id = "E" };

                // Track nodes by row
                var nodesByRow = new Dictionary<int, HashSet<string>>();
                nodesByRow[0] = new HashSet<string> { "S" };

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
                        
                        if (!nodesByRow.ContainsKey(grid.Length - 1))
                            nodesByRow[grid.Length - 1] = new HashSet<string>();
                        nodesByRow[grid.Length - 1].Add(colId);
                        
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
                            
                            if (!nodesByRow.ContainsKey(i))
                                nodesByRow[i] = new HashSet<string>();
                            nodesByRow[i].Add(vId);
                            
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

                Console.WriteLine("\n\nNodes by row:");
                foreach (var row in nodesByRow.OrderBy(kvp => kvp.Key))
                {
                    Console.WriteLine($"Row {row.Key}: {string.Join(", ", row.Value.OrderBy(x => x))}");
                }

                // Generate all combinations of nodes (one from each row)
                var rowsList = nodesByRow.OrderBy(kvp => kvp.Key).ToList();
                var validPaths = GenerateValidPaths(nodes, rowsList, 0, new List<string>());

                // Filter out paths that have . in first or last row
                var filteredPaths = validPaths.Where(path => path[0] != "." && path[path.Count - 1] != ".").ToList();

                Console.WriteLine("\nAll valid paths:");
                foreach (var path in filteredPaths)
                {
                    Console.WriteLine(string.Join(" -> ", path));
                }
                Console.WriteLine($"\n\nTotal valid paths: {filteredPaths.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
        }

        static List<List<string>> GenerateValidPaths(Dictionary<string, Node> nodes, List<KeyValuePair<int, HashSet<string>>> rowsList, int rowIndex, List<string> currentPath)
        {
            var result = new List<List<string>>();

            if (rowIndex == rowsList.Count)
            {
                result.Add(new List<string>(currentPath));
                return result;
            }

            var nodesInRow = rowsList[rowIndex].Value.OrderBy(x => x).ToList();
            foreach (var node in nodesInRow)
            {
                // Find the last non-placeholder node in the path
                string lastNode = null;
                for (int i = currentPath.Count - 1; i >= 0; i--)
                {
                    if (currentPath[i] != ".")
                    {
                        lastNode = currentPath[i];
                        break;
                    }
                }

                // Only continue if this node is connected from the last real node (or if it's the first node)
                if (lastNode == null || nodes[lastNode].Connections.Contains(node))
                {
                    currentPath.Add(node);
                    result.AddRange(GenerateValidPaths(nodes, rowsList, rowIndex + 1, currentPath));
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
            }

            // Also try skipping this row (add a placeholder)
            currentPath.Add(".");
            result.AddRange(GenerateValidPaths(nodes, rowsList, rowIndex + 1, currentPath));
            currentPath.RemoveAt(currentPath.Count - 1);

            return result;
        }
    }
}
