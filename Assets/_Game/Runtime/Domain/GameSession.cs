using System;

namespace MathPirates.Domain
{
    public enum GameMode { Hearts, Practice }
    public enum SessionPhase { Answering, Feedback, LevelComplete, Lost, Complete }
    public enum AnswerResult { Ignored, Correct, Incorrect }

    public sealed class GameSession
    {
        private readonly Campaign campaign;
        private readonly Random random;
        private int[] choices;

        public GameMode Mode { get; }
        public SessionPhase Phase { get; private set; } = SessionPhase.Answering;
        public int LevelIndex { get; private set; }
        public int ProblemIndex { get; private set; }
        public int Lives { get; private set; } = 5;
        public int Correct { get; private set; }
        public int Errors { get; private set; }
        public Level Level => campaign.Levels[LevelIndex];
        public Problem Problem => Level.Problems[ProblemIndex];
        public int LevelCount => campaign.Levels.Count;
        public System.Collections.Generic.IReadOnlyList<int> Choices => Array.AsReadOnly(choices);
        public AnswerResult LastResult { get; private set; }

        public GameSession(Campaign campaign, GameMode mode, int levelIndex = 0)
        {
            this.campaign = campaign ?? throw new ArgumentNullException(nameof(campaign));
            if (levelIndex < 0 || levelIndex >= campaign.Levels.Count) throw new ArgumentOutOfRangeException(nameof(levelIndex));
            Mode = mode;
            LevelIndex = levelIndex;
            random = new Random();
            choices = Problem.CreateChoices(random);
        }

        public AnswerResult Submit(int choiceIndex)
        {
            if (Phase != SessionPhase.Answering) return AnswerResult.Ignored;
            if (choiceIndex < 0 || choiceIndex >= choices.Length) return AnswerResult.Ignored;
            LastResult = choices[choiceIndex] == Problem.Answer ? AnswerResult.Correct : AnswerResult.Incorrect;
            Phase = SessionPhase.Feedback; // Lock input before presentation starts any animation.
            if (LastResult == AnswerResult.Correct) Correct++;
            else
            {
                Errors++;
                if (Mode == GameMode.Hearts && --Lives == 0) Phase = SessionPhase.Lost;
            }
            return LastResult;
        }

        public void FinishFeedback()
        {
            if (Phase != SessionPhase.Feedback) return;
            if (LastResult == AnswerResult.Incorrect) { Phase = SessionPhase.Answering; return; }
            if (ProblemIndex + 1 < Level.Problems.Count)
            {
                ProblemIndex++;
                choices = Problem.CreateChoices(random);
                Phase = SessionPhase.Answering;
            }
            else Phase = LevelIndex + 1 == LevelCount ? SessionPhase.Complete : SessionPhase.LevelComplete;
        }

        public void NextLevel()
        {
            if (Phase != SessionPhase.LevelComplete) return;
            LevelIndex++;
            ResetLevel();
        }

        public void RetryLevel()
        {
            if (Phase == SessionPhase.Lost) ResetLevel();
        }

        private void ResetLevel()
        {
            ProblemIndex = 0;
            Lives = 5;
            Phase = SessionPhase.Answering;
            LastResult = AnswerResult.Ignored;
            choices = Problem.CreateChoices(random);
        }
    }
}
