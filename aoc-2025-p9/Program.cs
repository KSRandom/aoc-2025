namespace aoc_2025_p9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

                // Find the lowest x and y coordinates
                long minX = long.MaxValue;
                long minY = long.MaxValue;

                foreach (var (x, y) in coordinates)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                }

                Console.WriteLine($"\n\nLowest Coordinates:");
                Console.WriteLine($"Minimum X: {minX}");
                Console.WriteLine($"Minimum Y: {minY}");

                // Translate coordinates by subtracting (minX - 1) and (minY - 1)
                // This shifts the lowest values to 1 instead of 0
                long offsetX = minX - 1;
                long offsetY = minY - 1;

                var translatedCoordinates = new List<(long x, long y)>();
                foreach (var (x, y) in coordinates)
                {
                    long newX = x - offsetX;
                    long newY = y - offsetY;
                    translatedCoordinates.Add((newX, newY));
                }

                Console.WriteLine($"\n\nTranslated Coordinates (offset by -{offsetX} for X, -{offsetY} for Y):");
                for (int i = 0; i < translatedCoordinates.Count; i++)
                {
                    var (x, y) = translatedCoordinates[i];
                    Console.WriteLine($"  {i}: ({x}, {y})");
                }

                // Find the new smallest and largest x and y coordinates
                long newMinX = long.MaxValue;
                long newMaxX = long.MinValue;
                long newMinY = long.MaxValue;
                long newMaxY = long.MinValue;

                foreach (var (x, y) in translatedCoordinates)
                {
                    newMinX = Math.Min(newMinX, x);
                    newMaxX = Math.Max(newMaxX, x);
                    newMinY = Math.Min(newMinY, y);
                    newMaxY = Math.Max(newMaxY, y);
                }

                Console.WriteLine($"\n\nTranslated Bounds:");
                Console.WriteLine($"Minimum X: {newMinX}");
                Console.WriteLine($"Maximum X: {newMaxX}");
                Console.WriteLine($"Minimum Y: {newMinY}");
                Console.WriteLine($"Maximum Y: {newMaxY}");
                Console.WriteLine($"Width (X range): {newMaxX - newMinX + 1}");
                Console.WriteLine($"Height (Y range): {newMaxY - newMinY + 1}");

                // Create 2D character array
                long width = newMaxX - newMinX + 1;
                long height = newMaxY - newMinY + 1;
                char[][] grid = new char[(int)height][];
                for (int i = 0; i < height; i++)
                {
                    grid[i] = new char[(int)width];
                    for (int j = 0; j < width; j++)
                    {
                        grid[i][j] = '.';
                    }
                }
                Console.WriteLine("Grid filled with '.'");

                // Draw lines between consecutive coordinates
                for (int i = 0; i < translatedCoordinates.Count; i++)
                {
                    int nextI = (i + 1) % translatedCoordinates.Count;
                    var (x1, y1) = translatedCoordinates[i];
                    var (x2, y2) = translatedCoordinates[nextI];

                    // Draw line from (x1, y1) to (x2, y2)
                    if (x1 == x2)
                    {
                        // Vertical line
                        long startY = Math.Min(y1, y2);
                        long endY = Math.Max(y1, y2);
                        for (long y = startY; y <= endY; y++)
                        {
                            grid[y - newMinY][(int)(x1 - newMinX)] = 'X';
                        }
                    }
                    else if (y1 == y2)
                    {
                        // Horizontal line
                        long startX = Math.Min(x1, x2);
                        long endX = Math.Max(x1, x2);
                        for (long x = startX; x <= endX; x++)
                        {
                            grid[(int)(y1 - newMinY)][x - newMinX] = 'X';
                        }
                    }
                }
                Console.WriteLine("Lines drawn.");

                // Fill interior using flood fill from edges
                // First, mark all exterior cells
                bool[][] exterior = new bool[(int)height][];
                for (int i = 0; i < height; i++)
                {
                    exterior[i] = new bool[(int)width];
                }

                // Flood fill from edges to mark exterior cells
                Queue<(int, int)> queue = new Queue<(int, int)>();
                
                // Add all edge cells to queue
                for (int i = 0; i < height; i++)
                {
                    if (grid[i][0] != 'X' && !exterior[i][0])
                    {
                        queue.Enqueue((i, 0));
                        exterior[i][0] = true;
                    }
                    if (grid[i][(int)width - 1] != 'X' && !exterior[i][(int)width - 1])
                    {
                        queue.Enqueue((i, (int)width - 1));
                        exterior[i][(int)width - 1] = true;
                    }
                }
                for (int j = 0; j < width; j++)
                {
                    if (grid[0][j] != 'X' && !exterior[0][j])
                    {
                        queue.Enqueue((0, j));
                        exterior[0][j] = true;
                    }
                    if (grid[(int)height - 1][j] != 'X' && !exterior[(int)height - 1][j])
                    {
                        queue.Enqueue(((int)height - 1, j));
                        exterior[(int)height - 1][j] = true;
                    }
                }

                // Flood fill
                while (queue.Count > 0)
                {
                    var (y, x) = queue.Dequeue();

                    // Check all 4 neighbors
                    int[] dy = { -1, 1, 0, 0 };
                    int[] dx = { 0, 0, -1, 1 };

                    for (int d = 0; d < 4; d++)
                    {
                        int ny = y + dy[d];
                        int nx = x + dx[d];

                        if (ny >= 0 && ny < height && nx >= 0 && nx < width && 
                            grid[ny][nx] != 'X' && !exterior[ny][nx])
                        {
                            exterior[ny][nx] = true;
                            queue.Enqueue((ny, nx));
                        }
                    }
                }

                // Fill interior with 'I'
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (grid[i][j] != 'X' && !exterior[i][j])
                        {
                            grid[i][j] = 'I';
                        }
                    }
                }

                // Print the grid
                Console.WriteLine($"Grid built.");

                // Count interior cells
                int interiorCount = 0;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (grid[i][j] == 'I')
                        {
                            interiorCount++;
                        }
                    }
                }
                Console.WriteLine($"\nInterior cells: {interiorCount}");

                // Find the largest rectangle where all points inside are marked with '#' , 'X', or 'I'
                long maxRectangleArea = 0;
                int maxRect_y1 = -1, maxRect_x1 = -1, maxRect_y2 = -1, maxRect_x2 = -1;

                // Only consider coordinates from the translated list as potential corners
                var rectangleTasks = new List<Task<(long area, int y1, int x1, int y2, int x2)>>();

                for (int i = 0; i < translatedCoordinates.Count; i++)
                {
                    for (int j = i + 1; j < translatedCoordinates.Count; j++)
                    {
                        int ii = i; // Capture for closure
                        int jj = j;

                        var task = Task.Run(() =>
                        {
                            var (x1, y1) = translatedCoordinates[ii];
                            var (x2, y2) = translatedCoordinates[jj];

                            // Ensure we have a proper rectangle (x1 != x2 and y1 != y2)
                            if (x1 == x2 || y1 == y2)
                                return (0L, -1, -1, -1, -1);

                            // Get the actual bounds of the rectangle
                            int rectX1 = (int)Math.Min(x1, x2) - (int)newMinX;
                            int rectX2 = (int)Math.Max(x1, x2) - (int)newMinX;
                            int rectY1 = (int)Math.Min(y1, y2) - (int)newMinY;
                            int rectY2 = (int)Math.Max(y1, y2) - (int)newMinY;

                            // Check if all points in rectangle are marked
                            for (int y = rectY1; y <= rectY2; y++)
                            {
                                for (int x = rectX1; x <= rectX2; x++)
                                {
                                    if (grid[y][x] != '#' && grid[y][x] != 'X' && grid[y][x] != 'I')
                                    {
                                        return (0L, -1, -1, -1, -1);
                                    }
                                }
                            }

                            long area = (long)(rectY2 - rectY1 + 1) * (rectX2 - rectX1 + 1);
                            return (area, rectY1, rectX1, rectY2, rectX2);
                        });

                        rectangleTasks.Add(task);
                    }
                }

                // Wait for all tasks and find the maximum with progress reporting
                var remainingTasks = new HashSet<Task<(long area, int y1, int x1, int y2, int x2)>>(rectangleTasks);
                int completedTasks = 0;
                int totalTasks = rectangleTasks.Count;

                while (remainingTasks.Count > 0)
                {
                    var completed = Task.WhenAny(remainingTasks).Result;
                    remainingTasks.Remove(completed);
                    completedTasks++;

                    if (completedTasks % Math.Max(1, totalTasks / 10) == 0 || completedTasks == totalTasks)
                    {
                        Console.WriteLine($"Rectangle verification progress: {completedTasks}/{totalTasks} ({100 * completedTasks / totalTasks}%)");
                    }
                }

                foreach (var task in rectangleTasks)
                {
                    var (area, y1, x1, y2, x2) = task.Result;
                    if (area > maxRectangleArea)
                    {
                        maxRectangleArea = area;
                        maxRect_y1 = y1;
                        maxRect_x1 = x1;
                        maxRect_y2 = y2;
                        maxRect_x2 = x2;
                    }
                }

                Console.WriteLine($"\n\nLargest Valid Rectangle:");
                if (maxRectangleArea > 0)
                {
                    long rectWidth = maxRect_x2 - maxRect_x1 + 1;
                    long rectHeight = maxRect_y2 - maxRect_y1 + 1;
                    Console.WriteLine($"Top-left: ({maxRect_x1}, {maxRect_y1})");
                    Console.WriteLine($"Bottom-right: ({maxRect_x2}, {maxRect_y2})");
                    Console.WriteLine($"Width: {rectWidth}");
                    Console.WriteLine($"Height: {rectHeight}");
                    Console.WriteLine($"Area: {maxRectangleArea}");

                    // Mark all points in the largest rectangle with 'O'
                    for (int y = maxRect_y1; y <= maxRect_y2; y++)
                    {
                        for (int x = maxRect_x1; x <= maxRect_x2; x++)
                        {
                            grid[y][x] = 'O';
                        }
                    }
                    /*
                    // Print the grid
                    Console.WriteLine($"\n\nGrid with Largest Rectangle Marked:");
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            Console.Write(grid[i][j]);
                        }
                        Console.WriteLine();
                    }*/
                }
                else
                {
                    Console.WriteLine("No valid rectangle found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            stopwatch.Stop();
            Console.WriteLine($"\n\nTotal execution time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }
    }
}
