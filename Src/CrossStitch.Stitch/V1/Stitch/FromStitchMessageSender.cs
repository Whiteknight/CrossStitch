using CrossStitch.Stitch.Utility;
using System;
using System.IO;

namespace CrossStitch.Stitch.V1.Stitch
{
    // Message sender for the Stitch to send responses to the Core
    public class FromStitchMessageSender : IDisposable
    {
        private readonly StreamWriter _stdout;

        public FromStitchMessageSender(Stream stdout)
        {
            _stdout = new StreamWriter(stdout);
        }

        public void SendMessage(FromStitchMessage message)
        {
            string buffer = JsonUtility.Serialize(message);
            _stdout.Write(buffer);
            _stdout.Write("\nend\n");
            _stdout.Flush();
        }

        public void SendAck(long id)
        {
            var msg = FromStitchMessage.Ack(id);
            SendMessage(msg);
        }

        public void SendFail(long id)
        {
            var msg = FromStitchMessage.Fail(id);
            SendMessage(msg);
        }

        public void SendSync(long heartbeatId)
        {
            var msg = FromStitchMessage.Sync(heartbeatId);
            SendMessage(msg);
        }

        public void Dispose()
        {
            _stdout.Dispose();
        }
    }
}