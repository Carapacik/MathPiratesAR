using System;
using System.Collections.Generic;
using System.Globalization;

namespace MathPirates.Domain
{
    public sealed class Problem
    {
        public string Expression { get; }
        public int Answer { get; }

        public Problem(string expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Answer = Evaluate(expression);
        }

        // Deliberately restricted to the curriculum's integer arithmetic. No eval or scripting.
        private static int Evaluate(string expression)
        {
            string text = expression.Replace(" ", string.Empty);
            int index = 0;
            int ReadNumber()
            {
                int start = index;
                while (index < text.Length && text[index] >= '0' && text[index] <= '9') index++;
                if (start == index) throw new FormatException("Expected a non-negative integer.");
                return int.Parse(text.Substring(start, index - start), CultureInfo.InvariantCulture);
            }

            int sum = 0;
            int term = ReadNumber();
            while (index < text.Length)
            {
                char operation = text[index++];
                int right = ReadNumber();
                checked
                {
                    switch (operation)
                    {
                        case '+': sum += term; term = right; break;
                        case '-': sum += term; term = -right; break;
                        case '*': term *= right; break;
                        case '/':
                            if (right == 0 || term % right != 0) throw new FormatException("Division must be exact.");
                            term /= right;
                            break;
                        default: throw new FormatException("Unsupported arithmetic operator.");
                    }
                }
            }
            return checked(sum + term);
        }

        public int[] CreateChoices(Random random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            var choices = new List<int> { Answer };
            // A bounded candidate pool guarantees termination even for an answer of zero.
            var candidates = new List<int>();
            for (int delta = -5; delta <= 5; delta++)
                if (delta != 0 && (long)Answer + delta >= 0 && (long)Answer + delta <= int.MaxValue)
                    candidates.Add(Answer + delta);
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(candidates.Count);
                choices.Add(candidates[index]);
                candidates.RemoveAt(index);
            }
            for (int i = choices.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (choices[i], choices[j]) = (choices[j], choices[i]);
            }
            return choices.ToArray();
        }
    }
}
