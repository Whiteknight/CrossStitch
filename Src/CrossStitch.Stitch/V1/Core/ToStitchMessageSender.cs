using System;
using System.IO;
using Newtonsoft.Json;

namespace CrossStitch.Stitch.V1.Core
{
    // Message sender to send a message from the Core to the Stitch
    public class ToStitchMessageSender : IDisposable
    {
        private readonly string _nodeName;
        private readonly StreamWriter _stdout;

        public ToStitchMessageSender(Stream stdout, string nodeName)
        {
            _nodeName = nodeName;
            _stdout = new StreamWriter(stdout);
        }

        public void SendMessage(ToStitchMessage message)
        {
            string buffer = JsonConvert.SerializeObject(message);
            _stdout.Write(buffer);
            _stdout.Write("\nend\n");
            _stdout.Flush();
        }

        public void SendHeartbeat(long id)
        {
            SendMessage(new ToStitchMessage
            {
                ChannelName = ToStitchMessage.HeartbeatChannelName,
                Data = string.Empty,
                Id = id,
                NodeName = _nodeName,
                StitchId = 0
            });
        }

        public void Dispose()
        {
            _stdout.Dispose();
        }
    }
}