using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unity.LiveCapture.ARKitFaceCapture.DefaultMapper.Editor
{
    /// <summary>
    /// A class containing methods for processing blend shape names.
    /// </summary>
    static class BlendShapeUtility
    {
        /// <summary>
        /// Gets a shape location by name.
        /// </summary>
        /// <param name="locationName">The location name.</param>
        /// <returns>The location as an enum value.</returns>
        public static FaceBlendShape GetLocation(string locationName)
        {
            return (FaceBlendShape)Enum.Parse(typeof(FaceBlendShape), locationName);
        }

        /// <summary>
        /// Gets the names of all blend shapes for a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to get the blend shapes for.</param>
        /// <param name="removePrefix">Trims the blend shape names by removing the prefix shared by
        /// the blend shapes, if there is any.</param>
        /// <returns>A new array containing the blend shapes.</returns>
        public static string[] GetBlendShapeNames(Mesh mesh, bool removePrefix = true)
        {
            var names = new string[mesh.blendShapeCount];

            for (var i = 0; i < mesh.blendShapeCount; i++)
                names[i] = mesh.GetBlendShapeName(i);

            return removePrefix ? RemoveCommonPrefix(names).ToArray() : names;
        }

        /// <summary>
        /// Removes the longest prefix shared by all strings in a collection from each string.
        /// </summary>
        /// <param name="values">The collection of strings to process.</param>
        /// <returns>A new collection containing the values without the shared prefix.</returns>
        static IEnumerable<string> RemoveCommonPrefix(IEnumerable<string> values)
        {
            var commonPrefix = new string(values.First()
                .TakeWhile((c, i) => values.All(s => i < s.Length && s[i] == c))
                .ToArray());

            var delimiters = new[] { '_', '.' };
            var startIndex = commonPrefix.LastIndexOfAny(delimiters) + 1;

            return values.Select(name => name.Substring(startIndex));
        }

        /// <summary>
        /// Finds the pairs of most similar strings from two collections of strings.
        /// </summary>
        /// <param name="a">The first collection of strings.</param>
        /// <param name="b">The second collection of strings.</param>
        /// <param name="tolerance">The threshold in the range [0,1] determining how similar two strings must be to be considered a match.</param>
        /// <returns>The indices of the matching strings from <paramref name="a"/> and <paramref name="b"/> respectively.</returns>
        public static IEnumerable<(int indexA, int indexB)> FindMatches(string[] a, string[] b, float tolerance)
        {
            a = PrepareNames(a);
            b = PrepareNames(b);

            for (var i = 0; i < a.Length; i++)
            {
                var matchIndex = FindMatch(a[i], b, tolerance);
                if (matchIndex >= 0)
                    yield return (i, matchIndex);
            }
        }

        static string[] PrepareNames(string[] values)
        {
            return RemoveCommonPrefix(values)
                .Select(name => RemoveSpecialCharacters(name).ToLower())
                .ToArray();
        }

        static string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();

            foreach (var c in str)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        static int FindMatch(string str, string[] candidates, float tolerance)
        {
            // Check using contains first for fast path
            for (var i = 0; i < candidates.Length; i++)
            {
                if (candidates[i].Contains(str))
                {
                    return i;
                }
            }

            // Check using edit distance
            var match = candidates.OrderBy(s => LevenshteinDistance(str, s)).First();

            var distance = LevenshteinDistance(str, match);
            var percentage = 1f - ((float)distance / Mathf.Max(match.Length, str.Length));

            return percentage < tolerance ? -1 : Array.IndexOf(candidates, match);
        }

        /// <summary>
        /// Computes the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="s">The first string.</param>
        /// <param name="t">The second string.</param>
        /// <returns>The number of edits needed to transform on string into another.</returns>
        /// <seealso href="https://www.dotnetperls.com/levenshtein"/>
        static int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;
            if (m == 0)
                return n;

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
            {
                //Step 4
                for (var j = 1; j <= m; j++)
                {
                    // Step 5
                    var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }
    }
}
