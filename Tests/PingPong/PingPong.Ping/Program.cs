using System;
using System.IO;
using System.Threading;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Process.Stitch;

namespace PingPong.Ping
{
    class Program
    {
        // Read a "Ping" data message, wait a delay, write a "Pong" data message
        private static StitchMessageManager _manager;

        static void Main(string[] args)
        {
            _manager = new StitchMessageManager(args);
            try
            {
                _manager.Start();
                Thread.Sleep(5000);

                // Start the chain by sending a ping to the entire group
                string groupName = _manager.CrossStitchParameters.ApplicationGroupName;
                _manager.Send(FromStitchMessage.ToGroupData(groupName, "ping?"));

                while (true)
                {
                    var msg = _manager.GetNextMessage();
                    if (msg == null)
                        continue;

                    if (msg.Data == "pong!")
                    {
                        Thread.Sleep(5000);
                        _manager.Send(FromStitchMessage.Respond(msg, "ping?"));
                    }
                    else
                        _manager.SendLogs(new[] { "Unknown data " + msg.Data });
                }
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }

        public static void Log(string s)
        {
            File.AppendAllText("D:\\Test\\PingPong.Pong.txt", s + "\n");
        }
    }
}
