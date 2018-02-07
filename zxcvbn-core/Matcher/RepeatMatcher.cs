﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Zxcvbn.Matcher
{
    /// <inheritdoc />
    /// <summary>
    /// A match found with the RepeatMatcher
    /// </summary>
    public class RepeatMatch : Match
    {
        public long BaseGuesses { get; set; }

        public string BaseMatches { get; set; }

        public string BaseToken { get; set; }

        /// <summary>
        /// The character that was repeated
        /// </summary>
        public char RepeatChar { get; set; }

        public int RepeatCount { get; set; }
    }

    /// <inheritdoc />
    /// <summary>
    /// Match repeated characters in the password (repeats must be more than two characters long to count)
    /// </summary>
    public class RepeatMatcher : IMatcher
    {
        private const string RepeatPattern = "repeat";

        /// <inheritdoc />
        /// <summary>
        /// Find repeat matches in <paramref name="password" />
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <returns>List of repeat matches</returns>
        /// <seealso cref="T:Zxcvbn.Matcher.RepeatMatch" />
        public IEnumerable<Match> MatchPassword(string password)
        {
            var matches = new List<Match>();
            var greedy = "(.+)\\1+";
            var lazy = "(.+?)\\1+";
            var lazyAnchored = "^(.+?)\\1+$";
            var lastIndex = 0;

            while (lastIndex < password.Length)
            {
                var greedyLastIndex = lastIndex;
                var lazyLastIndex = lastIndex;

                var greedyMatch = Regex.Match(password.Substring(greedyLastIndex), greedy);
                var lazyMatch = Regex.Match(password.Substring(lazyLastIndex), lazy);

                if (!greedyMatch.Success) break;

                System.Text.RegularExpressions.Match match;
                string baseToken;

                if (greedyMatch.Length > lazyMatch.Length)
                {
                    match = greedyMatch;
                    baseToken = Regex.Match(match.Value, lazyAnchored).Value;
                }
                else
                {
                    match = lazyMatch;
                    baseToken = match.Value;
                }

                var i = match.Index;
                var j = match.Index + match.Length - 1;

                var baseAnalysis =
                    PasswordScoring.MostGuessableMatchSequence(baseToken, Zxcvbn.MatchPassword(baseToken));

                var baseMatches = baseAnalysis.Sequence;
                var baseGuesses = baseAnalysis.Guesses;

                matches.Add(new RepeatMatch()
                {
                    Pattern = RepeatPattern,
                    i = i,
                    j = j,
                    Token = match.Value,
                    BaseToken = baseToken,
                    BaseGuesses = baseGuesses,
                    BaseMatches = baseMatches,
                    RepeatCount = match.Length / baseToken.Length
                });
            }

            return matches;
        }

        private static double CalculateEntropy(string match)
        {
            return Math.Log(PasswordScoring.PasswordCardinality(match) * match.Length, 2);
        }
    }
}