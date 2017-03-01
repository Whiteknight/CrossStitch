using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Stitch;
using System;
using System.Collections.Generic;

namespace StitchStart.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Some kind of mechanism to attach Console.KeyAvailable to CancellationSource.Cancel.
            StitchMessageManager manager = new StitchMessageManager(new MessageProcessor());
            manager.StartRunLoop();
        }
    }

    public class MessageProcessor : IToStitchMessageProcessor
    {
        public IEnumerable<FromStitchMessage> Process(ToStitchMessage message)
        {
            Console.WriteLine("Received {0} {1}: {2}", message.Id, message.ChannelName, message.Data);
            return new[]
            {
                FromStitchMessage.Ack(message.Id)
            };
        }
    }
}
