using System;
using System.Collections.Generic;
using System.IO;

namespace CrossStitch.Stitch.Process.Stdio
{
    public class StdioMessageChannel : IMessageChannel
    {
        private readonly StreamReader _stdin;
        private readonly StreamWriter _stdout;

        public StdioMessageChannel(StreamReader stdin = null, StreamWriter stdout = null)
        {
            _stdin = stdin ?? new StreamReader(Console.OpenStandardInput());
            _stdout = stdout ?? new StreamWriter(Console.OpenStandardOutput());
        }

        public string ReadMessage()
        {
            var lines = new List<string>();
            while (true)
            {
                var s = _stdin.ReadLine();
                if (s == null || s.Trim() == "end")
                    break;
                lines.Add(s);
            }

            return string.Join("\n", lines);
        }

        public void Send(string message)
        {
            _stdout.Write(message);
            _stdout.Write("\nend\n");
            _stdout.Flush();
        }

        public void Dispose()
        {
            _stdin?.Dispose();
            _stdout?.Dispose();
        }
    }
}
