using System;
using System.Linq;
using CrossStitch.Stitch.Process.Stitch;

namespace StitchStart.Client
{
    class Program
    {
        private static StitchMessageManager _manager;
        static void Main(string[] args)
        {
            try
            {
                _manager = new StitchMessageManager(args);
                _manager.ReceiveHeartbeats = true;
                _manager.ReceiveExitMessage = true;
                _manager.Start();
                
                while (true)
                {
                    var msg = _manager.GetNextMessage();
                    if (msg == null)
                        continue;

                    if (msg.IsExitMessage())
                    {
                        //Log("Got EXIT message from manager");
                        Environment.Exit(0);
                    }
                    if (msg.IsHeartbeatMessage())
                    {
                        Log($"Got HEARTBEAT message from manager");
                        _manager.SyncHeartbeat(msg.Id);
                        _manager.SendLogs(_manager.CrossStitchParameters.ToString());
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
            //System.IO.File.AppendAllText("D:\\Test\\StitchStart.Client.txt", s + "\n");
            _manager.SendLogs(new[] { s });
        }
    }
}
