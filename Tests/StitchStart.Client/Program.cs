using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Stitch;
using System;
using System.Linq;

namespace StitchStart.Client
{
    class Program
    {
        private static StitchMessageManager _manager;
        static void Main(string[] args)
        {
            _manager = new StitchMessageManager(args);
            _manager.ReceiveHeartbeats = true;
            try
            {
                _manager.Start();
                while (true)
                {
                    var msg = _manager.GetNextMessage();
                    if (msg == null)
                        continue;

                    if (msg.IsHeartbeatMessage())
                    {
                        _manager.SyncHeartbeat(msg.Id);
                        _manager.SendLogs(_manager.CrossStitchArguments.Select(kvp => kvp.Key + "=" + kvp.Value).ToArray());
                    }
                    else
                    {
                        // TODO: Real processing.
                        Log($"Message received {msg.ChannelName}, {msg.Id}");
                        _manager.AckMessage(msg.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
            Log("Stopped");
        }

        public static void Log(string s)
        {
            _manager.SendLogs(new[] { s });
            //File.AppendAllText("D:\\Test\\StitchStart.Client.txt", s + "\n");
        }
    }
}
