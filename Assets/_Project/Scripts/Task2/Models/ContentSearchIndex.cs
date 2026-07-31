using System;
using System.Collections.Generic;
using System.Linq;

namespace ContentDiscovery.Models
{
    public class ContentSearchIndex
    {
        private class TrieNode
        {
            public readonly Dictionary<char, TrieNode> Children = new Dictionary<char, TrieNode>();
            public HashSet<int> ItemIndices; // lazily created - most nodes are pass-through
        }

        private readonly TrieNode _root = new TrieNode();
        private readonly List<ContentItem> _items = new List<ContentItem>();

        public int Count => _items.Count;
        public IReadOnlyList<ContentItem> Items => _items;

        public void Add(ContentItem item)
        {
            int index = _items.Count;
            _items.Add(item);

            foreach (var token in Tokenize(item.NameLower))
                InsertToken(token, index);

            foreach (var token in Tokenize(item.AuthorLower))
                InsertToken(token, index);
        }

        public List<ContentItem> Search(string rawQuery, int maxResults = 200)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
                return new List<ContentItem>();

            var queryTokens = Tokenize(rawQuery.ToLowerInvariant()).ToList();
            if (queryTokens.Count == 0) return new List<ContentItem>();

            HashSet<int> candidates = null;

            foreach (var token in queryTokens)
            {
                var matches = CollectPrefixMatches(token);
                candidates = candidates == null ? matches : Intersect(candidates, matches);
                if (candidates.Count == 0) break; // short-circuit: no results possible
            }

            candidates ??= new HashSet<int>();

            return candidates
                .Select(i => _items[i])
                .OrderBy(i => i.NameLower, StringComparer.Ordinal)
                .Take(maxResults)
                .ToList();
        }

        private static HashSet<int> Intersect(HashSet<int> a, HashSet<int> b)
        {
            a.IntersectWith(b);
            return a;
        }

        private void InsertToken(string token, int itemIndex)
        {
            var node = _root;
            foreach (var c in token)
            {
                if (!node.Children.TryGetValue(c, out var next))
                {
                    next = new TrieNode();
                    node.Children[c] = next;
                }
                node = next;

                // Every prefix node along the way records the item, so a
                // partial query ("am") finds it without walking the full word.
                (node.ItemIndices ??= new HashSet<int>()).Add(itemIndex);
            }
        }

        private HashSet<int> CollectPrefixMatches(string prefix)
        {
            var node = _root;
            foreach (var c in prefix)
            {
                if (!node.Children.TryGetValue(c, out node))
                    return new HashSet<int>(); // prefix not present anywhere - fail fast
            }
            return node.ItemIndices != null ? new HashSet<int>(node.ItemIndices) : new HashSet<int>();
        }

        private static IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;

            int start = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    if (start < 0) start = i;
                }
                else if (start >= 0)
                {
                    yield return text.Substring(start, i - start);
                    start = -1;
                }
            }
            if (start >= 0) yield return text.Substring(start);
        }

        public void Clear()
        {
            _items.Clear();
            _root.Children.Clear();
            if (_root.ItemIndices != null)
            {
                _root.ItemIndices.Clear();
            }
        }
    }
}
