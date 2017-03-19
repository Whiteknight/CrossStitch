using System;
using System.Collections.Generic;
using System.IO;

namespace CrossStitch.Stitch.ProcessV1.Core
{
    // Message reader to read messages from the Stitch to the Core
    public class FromStitchMessageReader : IDisposable
    {
        private readonly StreamReader _stdin;

        public FromStitchMessageReader(Stream stdin)
        {
            _stdin = new StreamReader(stdin);
        }

        public FromStitchMessageReader(StreamReader stdin)
        {
            _stdin = stdin;
        }

        public FromStitchMessage ReadMessage()
        {
            var lines = new List<string>();
            while (true)
            {
                var s = _stdin.ReadLine();
                if (s == null || s.Trim() == "end")
                    break;
                lines.Add(s);
            }

            string buffer = string.Join("\n", lines);
            return Utility.JsonUtility.Deserialize<FromStitchMessage>(buffer);
        }

        public void Dispose()
        {
            _stdin.Dispose();
        }
    }
}