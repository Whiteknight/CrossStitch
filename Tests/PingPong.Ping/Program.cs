﻿using CrossStitch.Stitch.V1;
using CrossStitch.Stitch.V1.Stitch;
using System;
using System.IO;
using System.Threading;

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
