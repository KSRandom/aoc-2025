namespace aoc_2025_p9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: aoc-2025-p9 <filepath>");
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
                var coordinates = new List<(long x, long y)>();

                Console.WriteLine($"Parsing {lines.Length} lines from '{filePath}'...\n");

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] parts = line.Split(',');
                    if (parts.Length != 2)
                    {
                        Console.WriteLine($"Warning: Line {i + 1} does not contain exactly 2 coordinates, skipping.");
                        continue;
                    }

                    if (long.TryParse(parts[0].Trim(), out long x) &&
                        long.TryParse(parts[1].Trim(), out long y))
                    {
                        coordinates.Add((x, y));
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Line {i + 1} contains non-integer values, skipping.");
                    }
                }

                Console.WriteLine($"Successfully parsed {coordinates.Count} coordinates.\n");

                // Display all coordinates
                Console.WriteLine("Coordinates:");
                for (int i = 0; i < coordinates.Count; i++)
                {
                    var (x, y) = coordinates[i];
                    Console.WriteLine($"  {i}: ({x}, {y})");
                }

                // Find the largest rectangle area formed between any two points
                long maxArea = 0;
                int point1Index = -1;
                int point2Index = -1;

                for (int i = 0; i < coordinates.Count; i++)
                {
                    for (int j = i + 1; j < coordinates.Count; j++)
                    {
                        var (x1, y1) = coordinates[i];
                        var (x2, y2) = coordinates[j];

                        // Calculate the area of the rectangle formed by these two points
                        long width = Math.Abs(x2 - x1) + 1;
                        long height = Math.Abs(y2 - y1) + 1;
                        long area = width * height;

                        if (area > maxArea)
                        {
                            maxArea = area;
                            point1Index = i;
                            point2Index = j;
                        }
                    }
                }

                Console.WriteLine($"\n\nLargest Rectangle:");
                if (point1Index >= 0 && point2Index >= 0)
                {
                    var (x1, y1) = coordinates[point1Index];
                    var (x2, y2) = coordinates[point2Index];
                    long width = Math.Abs(x2 - x1) + 1;
                    long height = Math.Abs(y2 - y1) + 1;

                    Console.WriteLine($"Point 1 (index {point1Index}): ({x1}, {y1})");
                    Console.WriteLine($"Point 2 (index {point2Index}): ({x2}, {y2})");
                    Console.WriteLine($"Width: {width}");
                    Console.WriteLine($"Height: {height}");
                    Console.WriteLine($"Area: {maxArea}");
                }
                else
                {
                    Console.WriteLine("Not enough points to form a rectangle.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
