using System.Collections.Generic;

namespace CrossStitch.Stitch.Process.Stitch
{
    public class ParsedArguments
    {
        public ParsedArguments(IReadOnlyDictionary<string, string> crossStitchArguments, IReadOnlyList<string> customArguments)
        {
            CrossStitchArguments = crossStitchArguments ?? new Dictionary<string, string>();
            CustomArguments = customArguments ?? new string[0];
        }

        public IReadOnlyDictionary<string, string> CrossStitchArguments { get; }
        public IReadOnlyList<string> CustomArguments { get; }
    }
}