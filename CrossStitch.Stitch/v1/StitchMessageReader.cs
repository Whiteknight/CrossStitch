using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace CrossStitch.Stitch.v1
{
    public class StitchMessageReader : IDisposable
    {
        private readonly StreamReader _stdin;

        public StitchMessageReader(Stream stdin)
        {
            _stdin = new StreamReader(stdin);
        }

        public ToStitchMessage ReadMessage(CancellationToken cancellationToken)
        {
            string s;
            List<string> lines = new List<string>();
            do
            {
                s = _stdin.ReadLine();
                if (s == "end")
                    break;
                lines.Add(s);
            } while (!cancellationToken.IsCancellationRequested);

            string buffer = string.Join("\n", s);
            return JsonConvert.DeserializeObject<ToStitchMessage>(buffer);
        }

        public void Dispose()
        {
            _stdin.Dispose();
        }
    }
}