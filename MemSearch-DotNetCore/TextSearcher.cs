using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MemSearch
{
    internal static class TextSearcher
    {
        public static IEnumerable<String> Find(byte[] buffer, String marker, String regex)
        {
            // Because the target application could be encoding the text in any given encoding, 
            // check a few common ones
            var encodings = new Encoding[]{
                Encoding.ASCII,
                Encoding.Unicode,
                Encoding.UTF8
            };

            foreach (var encoding in encodings)
            {
                var str = encoding.GetString(buffer);
                if (str.Contains(marker))
                {
                    var res = Regex.Match(str, regex);
                    if (res.Success)
                    {
                        foreach (Group match in res.Groups)
                        {
                            if (!string.IsNullOrEmpty(match.Value))
                                yield return match.Value;
                        }
                    }
                }
            }
        }
    }
}
