using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ContentDiscovery.Models
{
    public static class CoverInfoParser
    {
        // DD/MM/YYYY or D/M/YYYY - matches every date observed in the sample data.
        private static readonly Regex DatePattern =
            new Regex(@"(\d{1,2})/(\d{1,2})/(\d{4})", RegexOptions.Compiled);

        private const string Unknown = "Unknown";

        public static ContentItem Parse(string id, string rawCoverInfo)
        {
            var item = new ContentItem
            {
                Id = id,
                ContentName = Unknown,
                Author = Unknown,
                Category = Unknown,
                Date = null,
                ParseSucceeded = false
            };

            if (string.IsNullOrWhiteSpace(rawCoverInfo))
            {
                FinalizeLowerCaseFields(item);
                return item;
            }

            var dateMatch = DatePattern.Match(rawCoverInfo);

            if (!dateMatch.Success)
            {
                // No reliable anchor. Best-effort fallback: treat the whole
                // string as the name so it is at least searchable/visible,
                // rather than dropping the record.
                item.ContentName = rawCoverInfo.Trim();
                FinalizeLowerCaseFields(item);
                return item;
            }

            // --- Date ---
            item.Date = TryBuildDate(dateMatch);

            // --- Name / Author (left of the date) ---
            string beforeDate = rawCoverInfo.Substring(0, dateMatch.Index);
            beforeDate = beforeDate.TrimEnd('_', ' ');

            if (beforeDate.Length > 0)
            {
                int lastUnderscore = beforeDate.LastIndexOf('_');
                if (lastUnderscore >= 0 && lastUnderscore < beforeDate.Length - 1)
                {
                    item.ContentName = beforeDate.Substring(0, lastUnderscore).Trim();
                    item.Author = beforeDate.Substring(lastUnderscore + 1).Trim();
                }
                else
                {
                    item.ContentName = beforeDate.Trim();
                }
            }

            if (string.IsNullOrEmpty(item.ContentName)) item.ContentName = Unknown;
            if (string.IsNullOrEmpty(item.Author)) item.Author = Unknown;

            // --- Category (right of the date), best-effort only ---
            int afterStart = dateMatch.Index + dateMatch.Length;
            if (afterStart < rawCoverInfo.Length)
            {
                string afterDate = rawCoverInfo.Substring(afterStart).TrimStart('_', ' ');
                if (afterDate.Length > 0)
                {
                    int nextUnderscore = afterDate.IndexOf('_');
                    item.Category = nextUnderscore >= 0
                        ? afterDate.Substring(0, nextUnderscore)
                        : afterDate;

                    if (string.IsNullOrWhiteSpace(item.Category)) item.Category = Unknown;
                }
            }

            item.ParseSucceeded = item.ContentName != Unknown;
            FinalizeLowerCaseFields(item);
            return item;
        }

        private static DateTime? TryBuildDate(Match m)
        {
            int day = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            int month = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            int year = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);

            try { return new DateTime(year, month, day); }
            catch (ArgumentOutOfRangeException) { return null; } // e.g. "32/13/2025"
        }

        private static void FinalizeLowerCaseFields(ContentItem item)
        {
            item.NameLower = (item.ContentName ?? string.Empty).ToLowerInvariant();
            item.AuthorLower = (item.Author ?? string.Empty).ToLowerInvariant();
        }
    }
}
