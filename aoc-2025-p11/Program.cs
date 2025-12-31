namespace aoc_2025_p11
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a file path as an argument.");
                return;
            }

            string filePath = args[0];
            bool printGraphviz = args.Length > 1 && args[1] == "--print";
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            var graph = BuildGraph(filePath);
            Console.WriteLine($"Graph built with {graph.Count} nodes");
            
            if (printGraphviz)
            {
                PrintGraphGraphviz(graph);
            }
            else
            {
                PrintGraph(graph);
            }
            Console.WriteLine();

            DetectCycles(graph);
            Console.WriteLine();

            long pathCount = CountPaths(graph, "you", "out");
            Console.WriteLine($"Number of paths from 'you' to 'out': {pathCount}");
            Console.WriteLine();

            long pathCountThroughBoth = CountPathsThroughNodes(graph, "svr", "out", new[] { "fft", "dac" });
            Console.WriteLine($"Number of paths from 'svr' to 'out' through both 'fft' and 'dac': {pathCountThroughBoth}");
        }

        static Dictionary<string, List<string>> BuildGraph(string filePath)
        {
            var graph = new Dictionary<string, List<string>>();

            try
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(": ");
                    if (parts.Length != 2)
                    {
                        Console.WriteLine($"Warning: Invalid format: {line}");
                        continue;
                    }

                    string source = parts[0].Trim();
                    string[] targets = parts[1].Split(' ');

                    // Ensure source node exists in graph
                    if (!graph.ContainsKey(source))
                    {
                        graph[source] = new List<string>();
                    }

                    // Add all target nodes
                    foreach (var target in targets)
                    {
                        string trimmedTarget = target.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmedTarget))
                        {
                            graph[source].Add(trimmedTarget);

                            // Ensure target nodes exist in graph
                            if (!graph.ContainsKey(trimmedTarget))
                            {
                                graph[trimmedTarget] = new List<string>();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return graph;
        }

        static void PrintGraph(Dictionary<string, List<string>> graph)
        {
            foreach (var node in graph.OrderBy(x => x.Key))
            {
                if (node.Value.Count > 0)
                {
                    Console.WriteLine($"{node.Key} -> {string.Join(", ", node.Value)}");
                }
                else
                {
                    Console.WriteLine($"{node.Key} -> (no outgoing edges)");
                }
            }
        }

        static void PrintGraphGraphviz(Dictionary<string, List<string>> graph)
        {
            Console.WriteLine("digraph G {");
            foreach (var node in graph.OrderBy(x => x.Key))
            {
                foreach (var target in node.Value)
                {
                    Console.WriteLine($"  {node.Key} -> {target};");
                }
            }
            Console.WriteLine("}");
        }

        static void DetectCycles(Dictionary<string, List<string>> graph)
        {
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            var cycles = new List<List<string>>();

            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    DFS(node, graph, visited, recursionStack, new List<string>(), cycles);
                }
            }

            if (cycles.Count == 0)
            {
                Console.WriteLine("No cycles detected - this is a DAG (Directed Acyclic Graph)");
            }
            else
            {
                Console.WriteLine($"Detected {cycles.Count} cycle(s):");
                foreach (var cycle in cycles)
                {
                    Console.WriteLine($"  {string.Join(" -> ", cycle)} -> {cycle[0]}");
                }
            }
        }

        static void DFS(string node, Dictionary<string, List<string>> graph, 
                        HashSet<string> visited, HashSet<string> recursionStack, 
                        List<string> path, List<List<string>> cycles)
        {
            visited.Add(node);
            recursionStack.Add(node);
            path.Add(node);

            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                {
                    DFS(neighbor, graph, visited, recursionStack, path, cycles);
                }
                else if (recursionStack.Contains(neighbor))
                {
                    // Found a cycle
                    int cycleStart = path.IndexOf(neighbor);
                    var cycle = path.Skip(cycleStart).ToList();
                    cycles.Add(cycle);
                }
            }

            path.RemoveAt(path.Count - 1);
            recursionStack.Remove(node);
        }

        static long CountPaths(Dictionary<string, List<string>> graph, string start, string end)
        {
            if (!graph.ContainsKey(start))
            {
                Console.WriteLine($"Start node '{start}' not found in graph");
                return 0;
            }

            if (!graph.ContainsKey(end))
            {
                Console.WriteLine($"End node '{end}' not found in graph");
                return 0;
            }

            var memo = new Dictionary<string, long>();
            return CountPathsDFS(graph, start, end, memo);
        }

        static long CountPathsDFS(Dictionary<string, List<string>> graph, string current, string end, Dictionary<string, long> memo)
        {
            if (current == end)
                return 1;

            if (memo.ContainsKey(current))
                return memo[current];

            long pathCount = 0;
            foreach (var neighbor in graph[current])
            {
                pathCount += CountPathsDFS(graph, neighbor, end, memo);
            }

            memo[current] = pathCount;
            return pathCount;
        }

        static long CountPathsThroughNodes(Dictionary<string, List<string>> graph, string start, string end, string[] requiredNodes)
        {
            if (!graph.ContainsKey(start))
            {
                Console.WriteLine($"Start node '{start}' not found in graph");
                return 0;
            }

            if (!graph.ContainsKey(end))
            {
                Console.WriteLine($"End node '{end}' not found in graph");
                return 0;
            }

            foreach (var requiredNode in requiredNodes)
            {
                if (!graph.ContainsKey(requiredNode))
                {
                    Console.WriteLine($"Required node '{requiredNode}' not found in graph");
                    return 0;
                }
            }

            // Find all paths that go through all required nodes
            // We'll use a recursive approach with state tracking
            var memo = new Dictionary<(string, int), long>();
            return CountPathsWithRequiredDFS(graph, start, end, requiredNodes, 0, memo);
        }

        static long CountPathsWithRequiredDFS(Dictionary<string, List<string>> graph, string current, string end, 
                                               string[] requiredNodes, int visitedMask, Dictionary<(string, int), long> memo)
        {
            // If we've reached the end and visited all required nodes
            if (current == end)
            {
                int allVisitedMask = (1 << requiredNodes.Length) - 1;
                return visitedMask == allVisitedMask ? 1 : 0;
            }

            var key = (current, visitedMask);
            if (memo.ContainsKey(key))
                return memo[key];

            long pathCount = 0;
            int newMask = visitedMask;

            // Check if current node is a required node and mark it as visited
            for (int i = 0; i < requiredNodes.Length; i++)
            {
                if (current == requiredNodes[i])
                {
                    newMask |= (1 << i);
                    break;
                }
            }

            foreach (var neighbor in graph[current])
            {
                pathCount += CountPathsWithRequiredDFS(graph, neighbor, end, requiredNodes, newMask, memo);
            }

            memo[key] = pathCount;
            return pathCount;
        }
    }
}
