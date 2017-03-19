using System;
using System.IO;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Stitch.ProcessV1.Core
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
            string buffer = JsonUtility.Serialize(message);
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