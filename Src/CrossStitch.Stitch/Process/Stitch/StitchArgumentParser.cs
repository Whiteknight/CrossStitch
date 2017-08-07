using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Stitch.Process.Stitch
{
    public class StitchArgumentParser
    {
        public ParsedArguments Parse(string[] processArgs)
        {
            if (processArgs == null || processArgs.Length == 0)
                return new ParsedArguments(null, null);

            int i = 0;
            var csArgs = new Dictionary<string, string>();
            for (; i < processArgs.Length; i++)
            {
                string s = processArgs[i];
                if (s == "--")
                {
                    i++;
                    break;
                }

                var parts = s.Split(new[] { '=' }, 2);
                if (parts.Length == 1)
                    csArgs.Add(parts[0], "1");
                else if (parts.Length == 2)
                    csArgs.Add(parts[0], parts[1]);
            }
            return new ParsedArguments(csArgs, processArgs.Skip(i).ToArray());
        }
    }
}