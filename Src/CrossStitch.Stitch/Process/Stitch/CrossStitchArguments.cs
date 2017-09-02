using System;
using System.Collections.Generic;
using System.Text;
using CrossStitch.Stitch.Utility.Extensions;

namespace CrossStitch.Stitch.Process.Stitch
{
    public class CrossStitchArguments
    {
        private readonly IReadOnlyDictionary<string, string> _arguments;

        public CrossStitchArguments(IReadOnlyDictionary<string, string> arguments)
        {
            _arguments = arguments;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var kvp in _arguments)
                builder.AppendFormat("{0}:{1}\n", kvp.Key, kvp.Value);
            return builder.ToString();
        }

        public string CoreId => GetCrossStitchArgument(Arguments.CoreId);
        public int CorePid => GetIntegerArgument(Arguments.CorePid);
        public string InstanceId => GetCrossStitchArgument(Arguments.InstanceId);
        public string ApplicationGroupName => GetCrossStitchArgument(Arguments.Application);

        public string ComponentGroupName
        {
            get
            {
                var application = GetCrossStitchArgument(Arguments.Application);
                var component = GetCrossStitchArgument(Arguments.Component);
                return $"{application}.{component}";
            }
        }

        public string VersionGroupName
        {
            get
            {
                var application = GetCrossStitchArgument(Arguments.Application);
                var component = GetCrossStitchArgument(Arguments.Component);
                var version = GetCrossStitchArgument(Arguments.Version);
                return $"{application}.{component}.{version}";
            }
        }

        public MessageChannelType MessageChannelType
        {
            get
            {
                bool ok = Enum.TryParse(GetCrossStitchArgument(Arguments.ChannelType), out MessageChannelType channelType);
                return ok ? channelType : MessageChannelType.Stdio;
            }
        }

        public MessageSerializerType MessageSerializerType
        {
            get
            {
                bool ok = Enum.TryParse(GetCrossStitchArgument(Arguments.Serializer), out MessageSerializerType serializerType);
                return ok ? serializerType : MessageSerializerType.Json;
            }
        }

        private int GetIntegerArgument(string name, int defaultValue = 0)
        {
            bool ok = int.TryParse(GetCrossStitchArgument(name), out int value);
            return ok ? value : 0;
        }

        private string GetCrossStitchArgument(string name)
        {
            return _arguments.GetOrDefault(name, string.Empty);
        }
    }
}