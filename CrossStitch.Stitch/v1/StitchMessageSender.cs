using System;
using System.IO;
using Newtonsoft.Json;

namespace CrossStitch.Stitch.v1
{
    public class StitchMessageSender : IDisposable
    {
        private readonly StreamWriter _stdout;

        public StitchMessageSender(Stream stdout)
        {
            _stdout = new StreamWriter(stdout);
        }

        public void SendMessage(FromStitchMessage message)
        {
            string buffer = JsonConvert.SerializeObject(message);
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

        public void SendSync()
        {
            var msg = FromStitchMessage.Sync();
            SendMessage(msg);
        }

        public void Dispose()
        {
            _stdout.Dispose();
        }
    }
}