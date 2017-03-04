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
        private const int ReadTimeoutMs = 5000;

        public FromStitchMessageReader(Stream stdin)
        {
            _stdin = new StreamReader(stdin);
        }

        public FromStitchMessageReader(StreamReader stdin)
        {
            _stdin = stdin;
        }

        private const int MaxFailures = 5;

        public FromStitchMessage ReadMessage(CancellationToken cancellationToken)
        {
            int failures = 0;
            var lines = new List<string>();
            while (true)
            {

                var task = _stdin.ReadLineAsync();
                bool ok = task.Wait(ReadTimeoutMs, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                if (!ok)
                {
                    failures++;
                    if (failures >= MaxFailures)
                        return null;
                    continue;
                }

                failures = 0;
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