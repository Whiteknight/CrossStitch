﻿using CrossStitch.App;
using System;
using System.IO;

namespace AppDomainStart.Client
{
    public class MyComponent : ICallIn
    {
        public void Start()
        {
            var communicationPort = AppDomain.CurrentDomain.GetData("_communicationPort");
            File.AppendAllText("C:/Test/MyComponent.txt", "Started:" + communicationPort + "\n");
        }

        public void Stop()
        {
            File.AppendAllText("C:/Test/MyComponent.txt", "Stopped\n");
        }
    }
}
