using System;
using System.Linq;
using MathPirates.Domain;
using UnityEngine;

namespace MathPirates.Content
{
    public static class CampaignAsset
    {
        [Serializable] private sealed class Document { public int schemaVersion; public Entry[] levels; public SectionEntry[] sections; }
        [Serializable] private sealed class Entry { public string id; public string sectionId; public string[] expressions; }
        [Serializable] private sealed class SectionEntry { public string id; }

        public static Campaign Load(TextAsset asset)
        {
            if (asset == null) throw new InvalidOperationException("Campaign asset is missing.");
            var data = JsonUtility.FromJson<Document>(asset.text);
            if (data?.levels == null) throw new FormatException("Campaign levels are missing.");
            if (data.schemaVersion != 1 || data.sections == null) throw new FormatException("Unsupported campaign schema.");
            return new Campaign(data.levels.Select(x => new Level(x.id, x.sectionId, x.expressions)), data.sections.Select(x => x.id));
        }
    }
}
