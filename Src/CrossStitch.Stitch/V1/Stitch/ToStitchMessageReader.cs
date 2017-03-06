using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossStitch.Stitch.V1.Stitch
{
    // Message reader to read messages from the Core to the Stitch
    public class ToStitchMessageReader : IDisposable
    {
        // TODO: We need to detect when the parent process exits, otherwise the child will continue
        // on as a zombie. There are a few strategies we can test:
        // 1) get the core pid and check if the process is alive periodically
        // 2) Have the core create a named mutex, and child processes can detect if the named mutex still exists
        // 3) Detect when a heartbeat has not been received in a while, and exit


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
            return JsonConvert.DeserializeObject<ToStitchMessage>(buffer);
        }

        public void Dispose()
        {
            _stdin.Dispose();
        }
    }
}