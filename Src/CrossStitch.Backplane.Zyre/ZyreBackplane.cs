using CrossStitch.Backplane.Zyre.Networking;
using CrossStitch.Backplane.Zyre.Networking.NetMq;
using CrossStitch.Core;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Modules.Master;
using CrossStitch.Core.Utility.Extensions;
using CrossStitch.Core.Utility.Serialization;
using CrossStitch.Stitch.Events;
using NetMQ.Zyre.ZyreEvents;
using System;
using System.Linq;

namespace CrossStitch.Backplane.Zyre
{
    public sealed class ZyreBackplane : IClusterBackplane
    {
        private readonly CrossStitchCore _core;
        private readonly BackplaneConfiguration _config;
        private readonly ISerializer _serializer;
        private readonly NetMQ.Zyre.Zyre _zyre;

        private NetMqMessageMapper _mapper;
        private Guid _uuid;
        private bool _connected;

        public ZyreBackplane(CrossStitchCore core, BackplaneConfiguration config = null, ISerializer serializer = null)
        {
            _core = core;
            _config = config ?? BackplaneConfiguration.GetDefault();
            _serializer = serializer ?? new JsonSerializer();

            _zyre = new NetMQ.Zyre.Zyre(core.NodeId.ToString());
            _zyre.EnterEvent += ZyreEnterEvent;
            _zyre.StopEvent += ZyreStopEvent;
            _zyre.ExitEvent += ZyreExitEvent;
            _zyre.EvasiveEvent += ZyreEvasiveEvent;
            _zyre.JoinEvent += ZyreJoinEvent;
            _zyre.LeaveEvent += ZyreLeaveEvent;
            _zyre.WhisperEvent += ZyreWhisperEvent;
            _zyre.ShoutEvent += ZyreShoutEvent;
        }

        public event EventHandler<PayloadEventArgs<MessageEnvelope>> MessageReceived;
        public event EventHandler<PayloadEventArgs<ZoneMemberEvent>> ZoneMember;
        public event EventHandler<PayloadEventArgs<ClusterMemberEvent>> ClusterMember;

        public BackplaneContext Start()
        {
            if (_connected)
                throw new Exception("Backplane is already started");

            _zyre.Start();
            _uuid = _zyre.Uuid();
            _zyre.Join(Zones.ZoneAll);
            foreach (string zone in _config.Zones.OrEmptyIfNull().Where(z => z != Zones.ZoneAll))
                _zyre.Join(zone);

            var envelopeFactory = new MessageEnvelopeBuilderFactory(_uuid, _core.NodeId);
            _mapper = new NetMqMessageMapper(_serializer, envelopeFactory);

            _connected = true;
            return new BackplaneContext
            {
                EnvelopeFactory = envelopeFactory,
                NodeNetworkId = _uuid
            };
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
            if (envelope.Header.ProxyNodeNetworkId.HasValue)
                _zyre.Whisper(envelope.Header.ProxyNodeNetworkId.Value, message);
            else if (envelope.Header.ToType == TargetType.Cluster)
                _zyre.Shout(Zones.ZoneAll, message);
            else if (envelope.Header.ToType == TargetType.Zone)
                _zyre.Shout(envelope.Header.ZoneName, message);
            else if (envelope.Header.ToType == TargetType.Node)
                _zyre.Whisper(envelope.Header.GetToNetworkUuid(), message);
        }

        public void Dispose()
        {
            Stop();
            _zyre.Dispose();
        }

        private void ZyreShoutEvent(object sender, ZyreEventShout e)
        {
            // Receive a message which is sent to the entire zone/cluster
            // We do not currently differentiate between messages which are whispered or shouted
            if (_mapper == null)
                return;

            var envelope = _mapper.Map(e.Content);
            MessageReceived.Raise(this, envelope.GetReceiveEventName(), envelope);
        }

        private void ZyreWhisperEvent(object sender, ZyreEventWhisper e)
        {
            // Receive a message which is sent point-to-point from a peer to this node
            // We do not currently differentiate between messages which are whispered or shouted
            if (_mapper == null)
                return;

            var envelope = _mapper.Map(e.Content);
            // TODO: Handle proxy cases, where we are receiving the message as a proxy, and need
            // to forward it on to the actual destination.

            MessageReceived.Raise(this, envelope.GetReceiveEventName(), envelope);
        }

        private void ZyreJoinEvent(object sender, ZyreEventJoin e)
        {
            // This node has joined a zone
            ZoneMember.Raise(this, ZoneMemberEvent.JoiningEvent, new ZoneMemberEvent
            {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid,
                Zone = e.GroupName
            });
        }

        private void ZyreLeaveEvent(object sender, ZyreEventLeave e)
        {
            // This node has left the zone
            ZoneMember.Raise(this, ZoneMemberEvent.LeavingEvent, new ZoneMemberEvent
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

        private void ZyreStopEvent(object sender, ZyreEventStop e)
        {
            // TODO: What to do here?
        }

        private void ZyreEnterEvent(object sender, ZyreEventEnter e)
        {
            // A peer node has joined the cluster
            ClusterMember.Raise(this, ClusterMemberEvent.EnteringEvent, new ClusterMemberEvent
            {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid
            });
        }

        private void ZyreExitEvent(object sender, ZyreEventExit e)
        {
            // A peer node has left the cluster
            ClusterMember.Raise(this, ClusterMemberEvent.ExitingEvent, new ClusterMemberEvent
            {
                NodeName = e.SenderName,
                NodeUuid = e.SenderUuid
            });
        }
    }
}