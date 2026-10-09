using MathPirates.Domain;
using UnityEngine;

namespace MathPirates.Infrastructure
{
    // Progress is local and separated by game mode. A checkpoint starts at the beginning of a level.
    public sealed class ProgressStore
    {
        private static string Key(GameMode mode) => $"mathpirates.v2.unlocked.{mode}";
        public int Unlocked(GameMode mode, Campaign campaign)
        {
            string id = PlayerPrefs.GetString(Key(mode), campaign.Levels[0].Id);
            for (int i = 0; i < campaign.Levels.Count; i++) if (campaign.Levels[i].Id == id) return i;
            return 0;
        }

        public void Unlock(GameMode mode, int level, Campaign campaign)
        {
            int index = Mathf.Max(Unlocked(mode, campaign), Mathf.Clamp(level, 0, campaign.Levels.Count - 1));
            PlayerPrefs.SetString(Key(mode), campaign.Levels[index].Id);
            PlayerPrefs.Save();
        }
    }
}
