using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace aoc_2025_p3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: <input file path>");
                return;
            }

            string inputPath = args[0];

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"File not found: {inputPath}");
                return;
            }

            long sum = 0;
            int lineNumber = 0;

            foreach (var line in File.ReadLines(inputPath))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Extract all digit characters from the line in order
                var digits = new LinkedList<char>();
                
                foreach (char c in line)
                {
                    if (char.IsDigit(c))
                    {
                        digits.AddLast(c);
                    }
                }

                // Find largest digits iteratively, each time requiring one fewer digit remaining
                int remainingRequired = 11;
                var node = digits.First;
                
                while (remainingRequired > 0 && node != null)
                {
                    // Find the largest digit starting from current node with at least remainingRequired digits after it
                    char largestDigit = '0';
                    var largestNode = node;
                    var searchNode = node;
                    
                    while (searchNode != null)
                    {
                        // Check if there are enough digits remaining after this position
                        int digitsAfter = 0;
                        var countNode = searchNode.Next;
                        while (countNode != null)
                        {
                            digitsAfter++;
                            countNode = countNode.Next;
                        }
                        
                        if (digitsAfter >= remainingRequired && searchNode.Value > largestDigit)
                        {
                            largestDigit = searchNode.Value;
                            largestNode = searchNode;
                        }
                        else if (searchNode.Value > largestDigit && digitsAfter >= remainingRequired)
                        {
                            largestNode = searchNode;
                        }
                        
                        searchNode = searchNode.Next;
                    }
                    
                    // Remove everything before the largest digit found
                    var removeNode = node;
                    while (removeNode != largestNode && removeNode != null)
                    {
                        var nextNode = removeNode.Next;
                        digits.Remove(removeNode);
                        removeNode = nextNode;
                    }
                    
                    // Move to the next digit after the largest one found
                    node = largestNode.Next;
                    remainingRequired--;
                }

                // Remove earliest lowest digits until exactly 12 remain
                char currentValue = '1';
                while (digits.Count > 12)
                {
                    node = digits.First;
                    while (node != null && digits.Count > 12)
                    {
                        var nextNode = node.Next;
                        if (node.Value == currentValue)
                        {
                            digits.Remove(node);
                        }
                        node = nextNode;
                    }
                    currentValue++;
                }

                // Concatenate remaining digits and convert to long
                string concatenated = string.Concat(digits);
                if (long.TryParse(concatenated, out long value))
                {
                    Console.WriteLine($"Line {lineNumber}: {line} -> Remaining: {concatenated} -> Value: {value}");
                    sum += value;
                }
            }

            Console.WriteLine($"Total lines processed: {lineNumber}");
            Console.WriteLine($"Final sum: {sum}");
        }
    }
}

// 167502486318468 is too low