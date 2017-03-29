using CrossStitch.Core;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Utility.Extensions;
using CrossStitch.Core.Utility.Serialization;
using CrossStitch.Stitch.Events;
using NetMQ.Zyre.ZyreEvents;
using System;
using System.IO;
using System.Linq;
using CrossStitch.Backplane.Zyre.Models;
using CrossStitch.Core.Models;

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

            // TODO: Need to expose more zyre options in the config, including broadcast port, broadcast interface, 
            // and beacon interval.

            _zyre = new NetMQ.Zyre.Zyre(core.NodeId);
            _zyre.EnterEvent += ZyreEnterEvent;
            _zyre.StopEvent += ZyreStopEvent;
            _zyre.ExitEvent += ZyreExitEvent;
            _zyre.EvasiveEvent += ZyreEvasiveEvent;
            _zyre.JoinEvent += ZyreJoinEvent;
            _zyre.LeaveEvent += ZyreLeaveEvent;
            _zyre.WhisperEvent += ZyreWhisperEvent;
            _zyre.ShoutEvent += ZyreShoutEvent;
        }

        public event EventHandler<PayloadEventArgs<ClusterMessage>> MessageReceived;
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

            _mapper = new NetMqMessageMapper(_serializer);

            _connected = true;
            return new BackplaneContext
            {
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

        public void Send(ClusterMessage envelope)
        {
            envelope.Header.FromNetworkId = _uuid.ToString();
            envelope.Header.FromNodeId = _core.NodeId;
            if (envelope.Header.FromType == TargetType.Node)
                envelope.Header.FromEntityId = _core.NodeId;
            if (!envelope.IsSendable())
                return;
            envelope.FillInNetworkNodeId(envelope.Header.FromNetworkId);

            var message = _mapper.Map(envelope);

            // If we have a proxy node ID, send the message there and let the proxy sort out
            // further actions.
            // Otherwise a zone type becomes a Shout and a Node type becomes a Whisper
            if (!string.IsNullOrEmpty(envelope.Header.ProxyNodeNetworkId))
            {
                Guid uuid;
                bool ok = Guid.TryParse(envelope.Header.ProxyNodeNetworkId, out uuid);
                if (ok)
                    _zyre.Whisper(uuid, message);
            }
            else if (envelope.Header.ToType == TargetType.Cluster)
                _zyre.Shout(Zones.ZoneAll, message);
            else if (envelope.Header.ToType == TargetType.Zone)
                _zyre.Shout(envelope.Header.ZoneName, message);
            else if (envelope.Header.ToType == TargetType.Node)
                _zyre.Whisper(envelope.Header.GetToNetworkUuid(), message);
        }

        public void TransferPackageFile(StitchGroupName groupName, string toNodeId, string filePath, string fileName, string jobId, string taskId)
        {
            // TODO: More validation and error handling
            // TODO: We need to get more sophisticated about this, such as doing the transfer in chunks and allowing restarts
            if (!groupName.IsValid() || !groupName.IsVersionGroup())
                throw new Exception("Must use a valid version name for a package upload file");

            var bytes = File.ReadAllBytes(filePath);
            var envelope = new FileTransferEnvelope
            {
                Contents = bytes,
                GroupName = groupName.VersionString,
                JobId = jobId,
                TaskId = taskId,
                PacketNumber = 1,
                TotalNumberOfPackets = 1,
                FileName = fileName
            };
            var message = new ClusterMessageBuilder()
                .ToNode(toNodeId)
                .FromNode()
                .WithInternalObjectPayload(envelope)
                .Build();
            Send(message);
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
                NodeId = e.SenderName,
                NetworkNodeId = e.SenderUuid.ToString()
            });
        }

        private void ZyreExitEvent(object sender, ZyreEventExit e)
        {
            // A peer node has left the cluster
            ClusterMember.Raise(this, ClusterMemberEvent.ExitingEvent, new ClusterMemberEvent
            {
                NodeId = e.SenderName,
                NetworkNodeId = e.SenderUuid.ToString()
            });
        }
    }
}