using Newtonsoft.Json;
using System;
using System.IO;

namespace CrossStitch.Stitch.V1.Core
{
    // Message sender to send a message from the Core to the Stitch
    public class ToStitchMessageSender : IDisposable
    {
        private readonly StreamWriter _stdout;

        public ToStitchMessageSender(Stream stdout)
        {
            _stdout = new StreamWriter(stdout);
        }

        public ToStitchMessageSender(StreamWriter stdout)
        {
            _stdout = stdout;
        }

        public void SendMessage(ToStitchMessage message)
        {
            string buffer = JsonConvert.SerializeObject(message);
            _stdout.Write(buffer);
            _stdout.Write("\nend\n");
            _stdout.Flush();
        }

        public void Dispose()
        {
            _stdout.Dispose();
        }
    }
}