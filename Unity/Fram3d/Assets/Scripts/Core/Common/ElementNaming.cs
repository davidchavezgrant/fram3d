using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Fram3d.Core.Common
{
    public static class ElementNaming
    {
        private static readonly Regex SUFFIX_PATTERN = new(@"^(.+)_(\d+)$");

        /// <summary>
        /// Generates a duplicate name by appending or incrementing a numeric
        /// suffix. "Chair" becomes "Chair_1"; "Chair_1" becomes "Chair_2".
        /// Finds the highest existing suffix for the same base name and
        /// returns one above it.
        /// </summary>
        public static string GenerateDuplicateName(string sourceName, IEnumerable<string> existingNames)
        {
            var baseName  = ParseBaseName(sourceName);
            var maxSuffix = 0;

            foreach (var name in existingNames)
            {
                var match = SUFFIX_PATTERN.Match(name);

                if (!match.Success || match.Groups[1].Value != baseName)
                {
                    continue;
                }

                var suffix = int.Parse(match.Groups[2].Value);

                if (suffix >= maxSuffix)
                {
                    maxSuffix = suffix + 1;
                }
            }

            // No numbered duplicates found — start at _1
            if (maxSuffix == 0)
            {
                maxSuffix = 1;
            }

            return baseName + "_" + maxSuffix;
        }

        /// <summary>
        /// Strips a trailing _N numeric suffix to find the base name.
        /// "Chair_2" returns "Chair". "Table_Top" returns "Table_Top"
        /// (because "Top" is not a number). "Chair" returns "Chair".
        /// </summary>
        public static string ParseBaseName(string name)
        {
            var match = SUFFIX_PATTERN.Match(name);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return name;
        }
    }
}
