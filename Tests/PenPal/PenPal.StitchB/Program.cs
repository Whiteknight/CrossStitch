using System;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Process.Stdio;
using CrossStitch.Stitch.Process.Stitch;

namespace PenPal.StitchB
{
    class Program
    {
        private static StitchMessageManager _manager;

        static void Main(string[] args)
        {
            _manager = new StitchMessageManager(args);
            try
            {
                _manager.Start();
                string groupName = _manager.CrossStitchArguments[Arguments.Application];

                while (true)
                {
                    var msg = _manager.GetNextMessage();
                    if (msg == null)
                        continue;

                    // When we receive a message "B", log it
                    if (msg.Data == "A")
                    {
                        _manager.Send(FromStitchMessage.Respond(msg, "B"));
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
