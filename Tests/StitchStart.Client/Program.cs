using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Stitch;
using System;
using System.IO;

namespace StitchStart.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log("Started " + Directory.GetCurrentDirectory());
                // TODO: Some kind of mechanism to attach Console.KeyAvailable to CancellationSource.Cancel.
                var manager = new StitchMessageManager(new MessageProcessor());
                manager.HeartbeatReceived += ManagerHeartbeatReceived;
                manager.StartRunLoop();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
            Log("Stopped");
        }

        private static void ManagerHeartbeatReceived(object sender, HeartbeatReceivedEventArgs e)
        {
            Log("Heartbeat " + e.Id);
        }

        public static void Log(string s)
        {
            File.AppendAllText("C:\\Test\\StitchStart.Client.txt", s + "\n");
        }
    }

    public class MessageProcessor : IToStitchMessageProcessor
    {
        public bool Process(ToStitchMessage message)
        {
            Program.Log(string.Format("Received {0} {1}: {2}", message.Id, message.ChannelName, message.Data));
            return true;
        }
    }
}
