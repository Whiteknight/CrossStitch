using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CrossStitch.Stitch.V1.Core
{
    // Message reader to read messages from the Stitch to the Core
    public class FromStitchMessageReader : IDisposable
    {
        private readonly StreamReader _stdin;
        private const int ReadTimeoutMs = 10000;

        public FromStitchMessageReader(Stream stdin)
        {
            _stdin = new StreamReader(stdin);
        }

        public FromStitchMessageReader(StreamReader stdin)
        {
            _stdin = stdin;
        }

        public FromStitchMessage ReadMessage(CancellationToken cancellationToken)
        {
            List<string> lines = new List<string>();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;
                var task = _stdin.ReadLineAsync();
                bool ok = task.Wait(ReadTimeoutMs, cancellationToken);
                if (!ok)
                    continue;
                var s = task.Result;
                if (s.Trim() == "end")
                    break;
                lines.Add(s);
            }

            string buffer = string.Join("\n", lines);
            return JsonConvert.DeserializeObject<FromStitchMessage>(buffer);
        }

        public void Dispose()
        {
            _stdin.Dispose();
        }
    }
}