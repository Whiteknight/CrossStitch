using System;
using System.Collections.Generic;
using System.IO;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Stitch.ProcessV1.Stitch
{
    // Message reader to read messages from the Core to the Stitch
    public class ToStitchMessageReader : IDisposable
    {
        private readonly StreamReader _stdin;

        public ToStitchMessageReader(Stream stdin)
        {
            _stdin = new StreamReader(stdin);
        }

        public ToStitchMessage ReadMessage()
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
            return JsonUtility.Deserialize<ToStitchMessage>(buffer);
        }

        public void Dispose()
        {
            _stdin.Dispose();
        }
    }
}