namespace aoc_2025_p10_2
{
    using Microsoft.Z3;

    public class LinearSolver
    {
        public static long? SolveForButtonPresses(int[] target, int[][] buttons)
        {
            using (var ctx = new Context())
            {
                var optimizer = ctx.MkOptimize();

                // Create variables for each button (number of times to press it)
                var buttonVars = new IntExpr[buttons.Length];
                for (int i = 0; i < buttons.Length; i++)
                {
                    buttonVars[i] = ctx.MkIntConst($"button_{i}");
                    // Add non-negativity constraint
                    optimizer.Add(ctx.MkGe(buttonVars[i], ctx.MkInt(0)));
                }

                // Add equations: for each index, sum of button effects must equal target
                for (int i = 0; i < target.Length; i++)
                {
                    ArithExpr sum = ctx.MkInt(0);
                    
                    for (int j = 0; j < buttons.Length; j++)
                    {
                        // Count how many times button j affects index i
                        int count = 0;
                        foreach (int idx in buttons[j])
                        {
                            if (idx == i)
                                count++;
                        }
                        
                        if (count > 0)
                        {
                            var term = ctx.MkMul(ctx.MkInt(count), buttonVars[j]);
                            sum = ctx.MkAdd(sum, term);
                        }
                    }
                    
                    optimizer.Add(ctx.MkEq(sum, ctx.MkInt(target[i])));
                }

                // Minimize the total number of button presses
                ArithExpr totalPresses = ctx.MkInt(0);
                foreach (var buttonVar in buttonVars)
                {
                    totalPresses = ctx.MkAdd(totalPresses, buttonVar);
                }
                optimizer.MkMinimize(totalPresses);

                // Check satisfiability
                if (optimizer.Check() == Status.SATISFIABLE)
                {
                    var model = optimizer.Model;
                    long minPresses = 0;
                    
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        var value = model.Eval(buttonVars[i], true);
                        if (long.TryParse(value.ToString(), out long presses))
                        {
                            minPresses += presses;
                        }
                    }
                    
                    return minPresses;
                }

                return null;
            }
        }
    }
}
