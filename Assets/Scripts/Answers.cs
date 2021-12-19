using System.Collections.Generic;

public class Answers
{
    private static readonly List<int> AnswersLevel1 = new()
    {
        10, 9, 10, 8, 9, 10, 7, 8, 9, 10, 6, 7, 8, 9, 10, 5, 6, 7, 8, 9, 10, 10, 4, 5,
        6, 7, 8, 9, 10, 3, 4, 5, 6, 7, 8, 9, 10,
        2, 3, 4, 5, 6, 7, 8, 9, 10, 6, 2
    };

    private static readonly List<int> AnswersLevel2 = new()
    {
        18, 17, 16, 15, 14, 13, 12
    };

    public readonly List<List<int>> AllAnswers = new() { AnswersLevel1, AnswersLevel2 };
}