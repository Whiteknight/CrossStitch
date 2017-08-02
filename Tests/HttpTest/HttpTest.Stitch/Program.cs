using System;
using CrossStitch.Stitch.Process.Stdio;
using CrossStitch.Stitch.Process.Stitch;

namespace HttpTest.Stitch
{
    public class Program
    {
        private static StitchMessageManager _manager;

        static void Main(string[] args)
        {
            _manager = new StitchMessageManager(args);
            try
            {
                _manager.Start();
                while (true)
                {
                    var msg = _manager.GetNextMessage();
                    if (msg == null)
                        continue;

                    Log($"Message received {msg.ChannelName}, {msg.Id}");
                    _manager.AckMessage(msg.Id);
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
