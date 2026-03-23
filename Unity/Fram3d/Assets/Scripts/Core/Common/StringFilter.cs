using System;
using System.Collections.Generic;
namespace Fram3d.Core.Common
{
    /// <summary>
    /// Multi-word, case-insensitive substring filter. An item matches when
    /// every space-separated word in the query appears as a substring.
    /// </summary>
    public static class StringFilter
    {
        public static List<string> Match(IReadOnlyList<string> items, string query)
        {
            if (items == null)
            {
                return new List<string>();
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<string>(items);
            }

            var words   = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var results = new List<string>();

            for (var i = 0; i < items.Count; i++)
            {
                var item    = items[i];
                var matches = true;

                for (var w = 0; w < words.Length; w++)
                {
                    if (item.IndexOf(words[w], StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    results.Add(item);
                }
            }

            return results;
        }
    }
}