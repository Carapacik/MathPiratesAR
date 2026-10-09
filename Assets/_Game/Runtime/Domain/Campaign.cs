using System;
using System.Collections.Generic;
using System.Linq;

namespace MathPirates.Domain
{
    public sealed class Level
    {
        public string Id { get; }
        public string SectionId { get; }
        public IReadOnlyList<Problem> Problems { get; }

        public Level(string id, string sectionId, IEnumerable<string> expressions)
        {
            Id = id;
            SectionId = sectionId;
            var problems = expressions.Select(x => new Problem(x)).ToArray();
            if (problems.Length == 0) throw new ArgumentException("A level must contain problems.");
            Problems = Array.AsReadOnly(problems);
        }
    }

    public sealed class Campaign
    {
        public IReadOnlyList<Level> Levels { get; }
        public Campaign(IEnumerable<Level> levels, IEnumerable<string> sectionIds)
        {
            var copy = levels.ToArray();
            if (copy.Length == 0) throw new ArgumentException("A campaign must contain levels.");
            Levels = Array.AsReadOnly(copy);
            if (copy.Select(x => x.Id).Distinct().Count() != copy.Length) throw new ArgumentException("Level IDs must be unique.");
            var sections = new HashSet<string>(sectionIds);
            if (copy.Any(x => !sections.Contains(x.SectionId))) throw new ArgumentException("Unknown section ID.");
        }
    }
}
