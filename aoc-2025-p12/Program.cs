namespace aoc_2025_p12
{
    internal class Program
    {
        class Block
        {
            public int Id { get; set; }
            public List<(int row, int col)> Cells { get; set; }

            public Block(int id)
            {
                Id = id;
                Cells = new List<(int, int)>();
            }

            public List<List<(int row, int col)>> GetAllRotations()
            {
                var rotations = new List<List<(int, int)>>();
                var current = new List<(int, int)>(Cells);

                for (int i = 0; i < 4; i++)
                {
                    rotations.Add(NormalizeBlock(current));
                    current = RotateBlock(current);
                }

                return rotations.DistinctBy(r => string.Join("|", r.OrderBy(c => (c.Item1, c.Item2)))).ToList();
            }

            private List<(int, int)> RotateBlock(List<(int, int)> cells)
            {
                // Rotate 90 degrees clockwise: (r, c) -> (c, -r)
                var rotated = cells.Select(c => (c.Item2, -c.Item1)).ToList();
                return NormalizeBlock(rotated);
            }

            private List<(int, int)> NormalizeBlock(List<(int, int)> cells)
            {
                if (cells.Count == 0)
                    return cells;

                int minRow = cells.Min(c => c.Item1);
                int minCol = cells.Min(c => c.Item2);

                return cells.Select(c => (c.Item1 - minRow, c.Item2 - minCol)).ToList();
            }

            public override string ToString()
            {
                return $"Block {Id}: {Cells.Count} cells";
            }
        }

        class GridPlacement
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int[] BlockCounts { get; set; }  // BlockCounts[i] = how many of block i to place

            public GridPlacement(int width, int height, int[] blockCounts)
            {
                Width = width;
                Height = height;
                BlockCounts = blockCounts;
            }

            public override string ToString()
            {
                var summary = string.Join(", ", BlockCounts.Select((count, id) => count > 0 ? $"{count}x Block{id}" : "").Where(s => s != ""));
                return $"Grid {Width}x{Height}: {summary}";
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a file path as an argument.");
                return;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }

            var blocks = ParseBlocks(filePath);
            var placements = ParsePlacements(filePath);
            
            Console.WriteLine($"Parsed {blocks.Count} blocks\n");
            
            foreach (var block in blocks.OrderBy(b => b.Id))
            {
                Console.WriteLine(block);
                Console.WriteLine("Grid representation:");
                PrintBlockGrid(block);
                Console.WriteLine();
            }

            Console.WriteLine($"\nParsed {placements.Count} grid placements\n");

            int validCount = 0;
            foreach (var placement in placements)
            {
                bool isValid = CanTilePlacement(placement, blocks);
                if (isValid)
                {
                    validCount++;
                }
                Console.WriteLine($"{placement} => {(isValid ? "Valid" : "Invalid")}");
            }

            Console.WriteLine($"Valid placements: {validCount} out of {placements.Count}");
        }

        static List<Block> ParseBlocks(string filePath)
        {
            var blocks = new List<Block>();
            var lines = File.ReadAllLines(filePath);

            Block currentBlock = null;
            int currentRow = 0;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    currentRow = 0;
                    continue;
                }

                // Check if this is a block definition line (e.g., "0:" or "1:")
                if (line.Contains(':') && !line.Contains('x'))
                {
                    var parts = line.Split(':');
                    if (int.TryParse(parts[0].Trim(), out int blockId))
                    {
                        currentBlock = new Block(blockId);
                        blocks.Add(currentBlock);
                        currentRow = 0;
                        continue;
                    }
                }

                // Skip lines with 'x' in them (numeric data)
                if (line.Contains('x'))
                {
                    continue;
                }

                // Parse block grid lines
                if (currentBlock != null && (line.Contains('#') || line.Contains('.')))
                {
                    for (int col = 0; col < line.Length; col++)
                    {
                        if (line[col] == '#')
                        {
                            currentBlock.Cells.Add((currentRow, col));
                        }
                    }
                    currentRow++;
                }
            }

            return blocks;
        }

        static List<GridPlacement> ParsePlacements(string filePath)
        {
            var placements = new List<GridPlacement>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Check if this is a placement line (e.g., "4x4: 0 0 0 0 2 0")
                if (line.Contains('x') && line.Contains(':'))
                {
                    var parts = line.Split(':');
                    var dimensions = parts[0].Trim().Split('x');

                    if (int.TryParse(dimensions[0], out int width) && int.TryParse(dimensions[1], out int height))
                    {
                        var countStrs = parts[1].Trim().Split(' ');
                        var blockCounts = new int[countStrs.Length];

                        for (int i = 0; i < countStrs.Length; i++)
                        {
                            if (int.TryParse(countStrs[i].Trim(), out int count))
                            {
                                blockCounts[i] = count;
                            }
                        }

                        placements.Add(new GridPlacement(width, height, blockCounts));
                    }
                }
            }

            return placements;
        }

        static bool CanTilePlacement(GridPlacement placement, List<Block> blocks)
        {
            // Convert block counts to a dictionary of needed blocks
            Dictionary<int, int> neededBlocks = new Dictionary<int, int>();
            int totalCellsNeeded = 0;
            
            for (int i = 0; i < placement.BlockCounts.Length; i++)
            {
                if (placement.BlockCounts[i] > 0)
                {
                    Block block = blocks.FirstOrDefault(b => b.Id == i);
                    if (block != null)
                    {
                        neededBlocks[i] = placement.BlockCounts[i];
                        totalCellsNeeded += block.Cells.Count * placement.BlockCounts[i];
                    }
                }
            }

            // Early exit: if we need more cells than the grid has, it's impossible
            int gridArea = placement.Height * placement.Width;
            if (totalCellsNeeded > gridArea)
                return false;

            // If we don't need any blocks, the placement is valid (empty grid)
            if (neededBlocks.Count == 0)
                return true;

            // Create a grid to track which cells are filled
            var grid = new int[placement.Height, placement.Width];
            for (int i = 0; i < placement.Height; i++)
                for (int j = 0; j < placement.Width; j++)
                    grid[i, j] = -1; // -1 means empty

            // Sort blocks by size (descending) for better pruning
            var sortedBlockIds = neededBlocks.Keys
                .OrderByDescending(id => blocks.FirstOrDefault(b => b.Id == id)?.Cells.Count ?? 0)
                .ToList();

            // Try to place all needed blocks
            return CanPlaceAllBlocks(grid, placement, blocks, neededBlocks, sortedBlockIds, 0);
        }

        static bool CanPlaceAllBlocks(int[,] grid, GridPlacement placement, List<Block> blocks, 
                                       Dictionary<int, int> neededBlocks, List<int> sortedBlockIds, int blockIndex)
        {
            // Check if we've placed all required blocks
            if (blockIndex >= sortedBlockIds.Count)
                return true;

            int blockId = sortedBlockIds[blockIndex];
            if (neededBlocks[blockId] <= 0)
                return CanPlaceAllBlocks(grid, placement, blocks, neededBlocks, sortedBlockIds, blockIndex + 1);

            Block block = blocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null)
                return false;

            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            // Try all rotations of this block
            var rotations = block.GetAllRotations();
            foreach (var rotation in rotations)
            {
                // Try placing this block at every possible position in the grid
                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        if (CanPlaceBlockAt(grid, row, col, rotation, blockId))
                        {
                            // Place the block
                            PlaceBlock(grid, row, col, rotation, blockId);
                            neededBlocks[blockId]--;

                            // Recursively try to place more instances or move to next block type
                            bool success = false;
                            if (neededBlocks[blockId] > 0)
                            {
                                // Try placing another instance of this block
                                success = CanPlaceAllBlocks(grid, placement, blocks, neededBlocks, sortedBlockIds, blockIndex);
                            }
                            else
                            {
                                // Move to next block type
                                success = CanPlaceAllBlocks(grid, placement, blocks, neededBlocks, sortedBlockIds, blockIndex + 1);
                            }

                            if (success)
                                return true;

                            // Backtrack
                            neededBlocks[blockId]++;
                            RemoveBlock(grid, row, col, rotation);
                        }
                    }
                }
            }

            return false;
        }

        static bool CanPlaceBlockAt(int[,] grid, int startRow, int startCol, List<(int, int)> blockCells, int blockId)
        {
            int height = grid.GetLength(0);
            int width = grid.GetLength(1);

            foreach (var (bRow, bCol) in blockCells)
            {
                int gridRow = startRow + bRow;
                int gridCol = startCol + bCol;

                // Check bounds
                if (gridRow < 0 || gridRow >= height || gridCol < 0 || gridCol >= width)
                    return false;

                // Check if cell is already occupied
                if (grid[gridRow, gridCol] != -1)
                    return false;
            }

            return true;
        }

        static void PlaceBlock(int[,] grid, int startRow, int startCol, List<(int, int)> blockCells, int blockId)
        {
            foreach (var (bRow, bCol) in blockCells)
            {
                grid[startRow + bRow, startCol + bCol] = blockId;
            }
        }

        static void RemoveBlock(int[,] grid, int startRow, int startCol, List<(int, int)> blockCells)
        {
            foreach (var (bRow, bCol) in blockCells)
            {
                grid[startRow + bRow, startCol + bCol] = -1;
            }
        }

        static void PrintBlockGrid(Block block)
        {
            if (block.Cells.Count == 0)
            {
                Console.WriteLine("(empty block)");
                return;
            }

            int maxRow = block.Cells.Max(c => c.row);
            int maxCol = block.Cells.Max(c => c.col);

            for (int row = 0; row <= maxRow; row++)
            {
                for (int col = 0; col <= maxCol; col++)
                {
                    if (block.Cells.Contains((row, col)))
                    {
                        Console.Write('#');
                    }
                    else
                    {
                        Console.Write('.');
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
