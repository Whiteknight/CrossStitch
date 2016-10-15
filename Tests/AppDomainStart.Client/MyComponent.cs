using System;
using System.IO;
using CrossStitch.App;

namespace AppDomainStart.Client
{
    public class MyComponent : ICallIn
    {
        public void Start()
        {
            var communicationPort = AppDomain.CurrentDomain.GetData("CommunicationPort");
            File.WriteAllText("C:/Test/MyComponent.txt", "Started:" + communicationPort);
        }

        public void Stop()
        {
            File.WriteAllText("C:/Test/MyComponent.txt", "Stopped");
        }
    }
}
