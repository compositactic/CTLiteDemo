using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CTLite
{
    internal class CompositePath
    {
        public CompositePath(string path)
        {
            var pathParts = Regex.Split(path, @"\?");

            Query = pathParts.Length == 2 ? "?" + pathParts[1] : string.Empty;
            PathAndQuery = "/" + path;

            var segmentList = new List<string>() { "/" };
            if (!string.IsNullOrEmpty(path) && path != "?")
                segmentList.AddRange(Regex.Split(pathParts[0], @"/").Select(s => s + "/"));

            if (!string.IsNullOrEmpty(path) && path != "?")
                segmentList[^1] = segmentList[^1].Trim('/');

            segmentList.Select(s => !string.IsNullOrEmpty(s)).ToArray();

            Segments = segmentList.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        }

        public string Query { get; }
        public string[] Segments { get; }
        public string PathAndQuery { get; }
    }
}
