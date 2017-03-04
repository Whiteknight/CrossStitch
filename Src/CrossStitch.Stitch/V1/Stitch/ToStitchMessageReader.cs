using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossStitch.Stitch.V1.Stitch
{
    // Message reader to read messages from the Core to the Stitch
    public class ToStitchMessageReader : IDisposable
    {
        private readonly StreamReader _stdin;
        private const int ReadTimeoutMs = 10000;

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
                if (s.Trim() == "end")
                    break;
                lines.Add(s);
            }

            string buffer = string.Join("\n", lines);
            return JsonConvert.DeserializeObject<ToStitchMessage>(buffer);
        }

        public void Dispose()
        {
            _stdin.Dispose();
        }
    }
}