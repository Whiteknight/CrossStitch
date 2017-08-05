﻿using CrossStitch.Core.Models;
using System;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Stitch;

namespace CrossStitch.Core.Modules.Stitches.Adaptors
{
    public interface IStitchAdaptor : IDisposable
    {
        AdaptorType Type { get; }
        bool Start();
        void Stop();
        void SendHeartbeat(long id);
        void SendMessage(long messageId, string channel, string data, string nodeId, string senderStitchInstanceId);
        StitchResourceUsage GetResources();
        CoreStitchContext StitchContext { get; }
    }
}