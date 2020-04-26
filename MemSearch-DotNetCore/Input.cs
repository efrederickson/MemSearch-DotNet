using System;
using System.Collections.Generic;
using System.Text;

namespace MemSearch
{
    internal class Input
    {
        /// <summary>
        /// Sets the title displayed in the console when a match occurs
        /// </summary>
        public String Title { get; set; }
        /// <summary>
        /// The basic text marker that gets looked for not every memory section has regex run against it
        /// </summary>
        public String Marker { get; set; }
        /// <summary>
        /// The regex that will find the information wanted
        /// </summary>
        public String Regex { get; set; }
        /// <summary>
        /// Specifies whether Decode should apply an UrlDecode transform
        /// </summary>
        public Boolean UrlDecode { get; set; }

        /// <summary>
        /// Applies any applicable transformations to the value
        /// </summary>
        /// <param name="match">The result from matching the regex</param>
        /// <returns></returns>
        public String Decode(string match)
        {
            if (UrlDecode)
                return System.Net.WebUtility.UrlDecode(match);

            return match;
        }
    }
}
