namespace aoc_2025_p8
{
    internal class Program
    {
        class Node
        {
            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }

            public override string ToString()
            {
                return $"Node {Id}: ({X}, {Y}, {Z})";
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: aoc-2025-p8 <filepath> [max_connections]");
                return;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' not found.");
                return;
            }

            int maxConnections = int.MaxValue;
            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out int maxConns))
                {
                    maxConnections = maxConns;
                    Console.WriteLine($"Limiting connections to {maxConnections}");
                }
                else
                {
                    Console.WriteLine($"Warning: Invalid max_connections value '{args[1]}', using unlimited.");
                }
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                var nodes = new List<Node>();

                Console.WriteLine($"Parsing {lines.Length} lines from '{filePath}'...\n");

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] parts = line.Split(',');
                    if (parts.Length != 3)
                    {
                        Console.WriteLine($"Warning: Line {i + 1} does not contain exactly 3 coordinates, skipping.");
                        continue;
                    }

                    if (int.TryParse(parts[0], out int x) &&
                        int.TryParse(parts[1], out int y) &&
                        int.TryParse(parts[2], out int z))
                    {
                        nodes.Add(new Node
                        {
                            Id = nodes.Count,
                            X = x,
                            Y = y,
                            Z = z
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Line {i + 1} contains non-integer values, skipping.");
                    }
                }

                Console.WriteLine($"Successfully created {nodes.Count} nodes.\n");

                // Display all nodes
                Console.WriteLine("Nodes:");
                foreach (var node in nodes)
                {
                    Console.WriteLine(node);
                }

                // Compute Euclidean distances between all pairs of nodes
                int nodeCount = nodes.Count;
                double[][] distances = new double[nodeCount][];
                for (int i = 0; i < nodeCount; i++)
                {
                    distances[i] = new double[nodeCount];
                    for (int j = 0; j < nodeCount; j++)
                    {
                        if (i == j)
                        {
                            distances[i][j] = 0;
                        }
                        else
                        {
                            Node node1 = nodes[i];
                            Node node2 = nodes[j];
                            double dx = node1.X - node2.X;
                            double dy = node1.Y - node2.Y;
                            double dz = node1.Z - node2.Z;
                            distances[i][j] = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        }
                    }
                }

                Console.WriteLine("\n\nDistance Matrix:");
                Console.WriteLine("Node distances (i -> j):");
                for (int i = 0; i < nodeCount; i++)
                {
                    Console.Write($"Node {i}: ");
                    for (int j = 0; j < nodeCount; j++)
                    {
                        Console.Write($"{distances[i][j]:F2} ");
                    }
                    Console.WriteLine();
                }

                // Build circuits by connecting nodes in order of shortest distance
                var nodeToCircuit = new Dictionary<int, int>(); // node id -> circuit id
                var circuits = new List<HashSet<int>>(); // list of circuits, each is a set of node ids

                // Create a list of all distances with their node pairs
                var distancePairs = new List<(double distance, int node1, int node2)>();
                for (int i = 0; i < nodeCount; i++)
                {
                    for (int j = i + 1; j < nodeCount; j++)
                    {
                        distancePairs.Add((distances[i][j], i, j));
                    }
                }

                // Sort by distance (shortest first)
                distancePairs.Sort((a, b) => a.distance.CompareTo(b.distance));

                Console.WriteLine("\n\nConnecting nodes by shortest distance:");
                int connectionCount = 0;
                foreach (var (distance, node1, node2) in distancePairs)
                {
                    bool node1InCircuit = nodeToCircuit.ContainsKey(node1);
                    bool node2InCircuit = nodeToCircuit.ContainsKey(node2);

                    if (!node1InCircuit && !node2InCircuit)
                    {
                        // Create a new circuit
                        int circuitId = circuits.Count;
                        var newCircuit = new HashSet<int> { node1, node2 };
                        circuits.Add(newCircuit);
                        nodeToCircuit[node1] = circuitId;
                        nodeToCircuit[node2] = circuitId;
                        connectionCount++;
                        Console.WriteLine($"Created circuit {circuitId}: Node {node1} -- Node {node2} (distance: {distance:F2})");
                    }
                    else if (node1InCircuit && !node2InCircuit)
                    {
                        // Add node2 to node1's circuit
                        int circuitId = nodeToCircuit[node1];
                        circuits[circuitId].Add(node2);
                        nodeToCircuit[node2] = circuitId;
                        connectionCount++;
                        Console.WriteLine($"Added Node {node2} to circuit {circuitId} (distance: {distance:F2})");
                    }
                    else if (!node1InCircuit && node2InCircuit)
                    {
                        // Add node1 to node2's circuit
                        int circuitId = nodeToCircuit[node2];
                        circuits[circuitId].Add(node1);
                        nodeToCircuit[node1] = circuitId;
                        connectionCount++;
                        Console.WriteLine($"Added Node {node1} to circuit {circuitId} (distance: {distance:F2})");
                    }
                    else
                    {
                        // Both nodes are in circuits
                        int circuit1 = nodeToCircuit[node1];
                        int circuit2 = nodeToCircuit[node2];

                        if (circuit1 != circuit2)
                        {
                            // Merge the circuits
                            var mergedCircuit = circuits[circuit1];
                            var circuitToMerge = circuits[circuit2];
                            mergedCircuit.UnionWith(circuitToMerge);

                            // Update all nodes in circuit2 to point to circuit1
                            foreach (var node in circuitToMerge)
                            {
                                nodeToCircuit[node] = circuit1;
                            }

                            // Remove the old circuit
                            circuits[circuit2] = null;
                            connectionCount++;
                            Console.WriteLine($"Merged circuit {circuit2} into circuit {circuit1} (distance: {distance:F2})");
                        } else
                        {
                            Console.WriteLine($"Nodes were already connecdted in circuit {circuit1}");
                            connectionCount++;
                        }
                    }

                    // Limit the number of connections if maxConnections is set
                    if (connectionCount >= maxConnections)
                    {
                        Console.WriteLine($"\nReached maximum number of connections ({maxConnections}). Stopping.");
                        break;
                    }
                }

                Console.WriteLine("\n\nFinal Circuits:");
                for (int i = 0; i < circuits.Count; i++)
                {
                    if (circuits[i] != null)
                    {
                        var sortedNodes = circuits[i].OrderBy(n => n).ToList();
                        Console.WriteLine($"Circuit {i}: {string.Join(", ", sortedNodes)}");
                    }
                }

                // Find the 3 circuits with the most nodes
                var activeCircuits = circuits.Where(c => c != null).ToList();
                var circuitsBySize = activeCircuits
                    .Select((circuit, index) => new { Circuit = circuit, Size = circuit.Count, OriginalIndex = index })
                    .OrderByDescending(c => c.Size)
                    .ToList();

                Console.WriteLine("\n\nCircuits by size:");
                foreach (var circuit in circuitsBySize)
                {
                    Console.WriteLine($"Circuit {circuit.OriginalIndex}: {circuit.Size} nodes");
                }

                // Get the top 3 circuits
                var top3Circuits = circuitsBySize.Take(3).ToList();

                if (top3Circuits.Count >= 3)
                {
                    long product = 1;
                    Console.WriteLine("\n\nTop 3 circuits by size:");
                    foreach (var circuit in top3Circuits)
                    {
                        Console.WriteLine($"Circuit {circuit.OriginalIndex}: {circuit.Size} nodes");
                        product *= circuit.Size;
                    }

                    Console.WriteLine($"\nProduct of top 3 circuit sizes: {product}");
                }
                else
                {
                    Console.WriteLine($"\n\nWarning: Only {top3Circuits.Count} active circuits found. Need at least 3 for multiplication.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
