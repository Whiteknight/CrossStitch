using System;
using CrossStitch.Core.Backplane.Events;
using CrossStitch.Core.Networking;
using CrossStitch.Core.Utility;
using CrossStitch.Core.Utility.Extensions;
using NetMQ.Zyre;
using NetMQ.Zyre.ZyreEvents;

namespace CrossStitch.Core.Backplane
{
    public sealed class ZyreBackplane : IClusterBackplane
    {
        private readonly BackplaneConfiguration _config;
        private readonly Zyre _zyre;
        private readonly NetMqMessageMapper _mapper;
        private Guid _uuid;
        private bool _connected;

        public ZyreBackplane(BackplaneConfiguration config, string nodeName, ISerializer serializer)
        {
            _zyre = new Zyre(nodeName);
            _zyre.EnterEvent += ZyreEnterEvent;
            _zyre.StopEvent += ZyreStopEvent;
            _zyre.ExitEvent += ZyreExitEvent;
            _zyre.EvasiveEvent += ZyreEvasiveEvent;
            _zyre.JoinEvent += ZyreJoinEvent;
            _zyre.LeaveEvent += ZyreLeaveEvent;
            _zyre.WhisperEvent += ZyreWhisperEvent;
            _zyre.ShoutEvent += ZyreShoutEvent;
            _config = config;
            _mapper = new NetMqMessageMapper(serializer);
        }

        public event EventHandler<PayloadEventArgs<MessageEnvelope>> MessageReceived;
        public event EventHandler<PayloadEventArgs<ZoneMemberEvent>> ZoneMember;
        public event EventHandler<PayloadEventArgs<ClusterMemberEvent>> ClusterMember;

        private void ZyreShoutEvent(object sender, ZyreEventShout e)
        {
            var envelope = _mapper.Map(e.Content);
            MessageReceived.Raise(this, MessageEnvelope.ReceiveEventName, envelope);
        }

        private void ZyreWhisperEvent(object sender, ZyreEventWhisper e)
        {
            var envelope = _mapper.Map(e.Content);
            MessageReceived.Raise(this, MessageEnvelope.ReceiveEventName, envelope);
        }

        private void ZyreLeaveEvent(object sender, ZyreEventLeave e)
        {
            ZoneMember.Raise(this, ZoneMemberEvent.LeavingEvent, new ZoneMemberEvent {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid,
                Zone = e.GroupName
            });
        }

        private void ZyreJoinEvent(object sender, ZyreEventJoin e)
        {
            ZoneMember.Raise(this, ZoneMemberEvent.JoiningEvent, new ZoneMemberEvent
            {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid,
                Zone = e.GroupName
            });
        }

        private void ZyreEvasiveEvent(object sender, ZyreEventEvasive e)
        {
            // TODO: What to do here?
        }

        private void ZyreExitEvent(object sender, ZyreEventExit e)
        {
            ClusterMember.Raise(this, ClusterMemberEvent.ExitingEvent, new ClusterMemberEvent
            {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid
            });
        }

        private void ZyreStopEvent(object sender, ZyreEventStop e)
        {
            // TODO: What to do here?
        }

        private void ZyreEnterEvent(object sender, ZyreEventEnter e)
        {
            var peers = _zyre.Peers();
            foreach (var peerUuid in peers)
            {
                ClusterMember.Raise(this, ClusterMemberEvent.EnteringEvent, new ClusterMemberEvent
                {
                    NodeName = e.SenderName,
                    NodeUuid = peerUuid
                });
            }
            ClusterMember.Raise(this, ClusterMemberEvent.EnteringEvent, new ClusterMemberEvent
            {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid
            });
        }

        public void Start(RunningNode context)
        {
            if (_connected)
                throw new Exception("Backplane is already started");

            _zyre.Start();
            foreach (string zone in _config.Zones.OrEmptyIfNull())
                _zyre.Join(zone);

            _uuid = _zyre.Uuid();
            context.NodeId = _uuid;
            
            _connected = true;
        }

        public void Stop()
        {
            if (!_connected)
                throw new Exception("Backplane is already stopped");

            foreach (string zone in _config.Zones)
                _zyre.Leave(zone);

            _zyre.Stop();
            _connected = false;
        }

        public void Send(MessageEnvelope envelope)
        {
            var message = _mapper.Map(envelope);

            // If we have a proxy node ID, send the message there and let the proxy sort out
            // further actions.
            // Otherwise a zone type becomes a Shout and a Node type becomes a Whisper
            if (envelope.Header.ProxyNodeId.HasValue)
                _zyre.Whisper(envelope.Header.ProxyNodeId.Value, message);
            else if (envelope.Header.ToType == TargetType.Zone)
                _zyre.Shout(envelope.Header.ZoneName, message);
            else if (envelope.Header.ToType == TargetType.Node)
                _zyre.Whisper(envelope.Header.ToId, message);
        }

        public void Dispose()
        {
            Stop();
            _zyre.Dispose();
        }
    }
}