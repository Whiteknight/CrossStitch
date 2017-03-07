using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Stitch;
using System;

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
                        ManagerHeartbeatReceived(msg);
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

        private static void ManagerHeartbeatReceived(ToStitchMessage heartbeat)
        {
            Log("Heartbeat " + heartbeat.Id);
        }

        public static void Log(string s)
        {
            _manager.SendLogs(new[] { s });
            //File.AppendAllText("D:\\Test\\StitchStart.Client.txt", s + "\n");
        }
    }
}
