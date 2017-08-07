using System;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Process.Stitch;

namespace PenPal.StitchA
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
                string groupName = _manager.CrossStitchParameters.ApplicationGroupName;

                while (true)
                {
                    var msg = _manager.GetNextMessage();
                    if (msg == null)
                        continue;

                    // When we receive a heartbeat, broadcast a message to all stitches in the application group
                    if (msg.IsHeartbeatMessage())
                    {
                        _manager.SyncHeartbeat(msg.Id);
                        _manager.Send(FromStitchMessage.ToGroupData(groupName, "A"));
                    }

                    // When we receive a message "B", log it
                    else if (msg.Data == "B")
                    {
                        _manager.SendLogs(new[] { "Received B" });
                    }

                    // Else, log that we've gotten something unexpected
                    else
                        _manager.SendLogs(new[] { "Unknown data " + msg.Data });
                }
            }
            catch (Exception e)
            {
                _manager.SendLogs(new[] { e.ToString() });
            }
        }
    }
}
