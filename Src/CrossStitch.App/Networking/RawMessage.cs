//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CrossStitch.Core.Networking
//{
//    public class RawMessage
//    {
//        private readonly List<byte[]> _frames;

//        public RawMessage(List<byte[]> frames)
//        {
//            _frames = frames;
//            if (frames.Count < 1)
//                throw new Exception("Not enough frames");
//            Command = Encoding.Unicode.GetString(frames[0]);
//            _frames.RemoveAt(0);
//        }

//        public string Command { get; private set; }

//        public PayloadMessage AsPayloadMessage(ISerializer serializer)
//        {
//            return new PayloadMessage(Command, _frames, serializer);
//        }
//    }

//    public class PayloadMessage
//    {
//        public string Command { get; private set; }
//        public IReadOnlyList<object> Payloads { get; private set; }
//        public Type PayloadType { get; private set; }

//        public PayloadMessage(string command, List<byte[]> frames, ISerializer serializer)
//        {
//            Command = command;
//            if (frames.Count < 2)
//                throw new Exception("Not enough frames");
//            string typeName = Encoding.Unicode.GetString(frames[0]);
//            PayloadType = Type.GetType(typeName);
//            if (PayloadType == null)
//                throw new Exception("Unknown type");

//            List<object> payloads = new List<object>();
//            for (int i = 1; i < frames.Count; i++)
//            {
//                var obj = serializer.DeserializeObject(PayloadType, frames[0]);
//                payloads.Add(obj);
//            }
//            Payloads = payloads;
//        }

//        public PayloadMessage<TPayload> Cast<TPayload>()
//        {
//            return new PayloadMessage<TPayload>(Command, PayloadType, Payloads);
//        }
//    }

//    public class PayloadMessage<TPayload>
//    {
//        public string Command { get; private set; }
//        public IReadOnlyList<TPayload> Payloads { get; private set; }
//        public PayloadMessage(string command, Type type, IEnumerable<object> payloads)
//        {
//            Command = command;
//            if (!typeof (TPayload).IsAssignableFrom(type))
//                throw new Exception("Incompatible type");
//            Payloads = payloads.OfType<TPayload>().ToList();
//        }
//    }
//}
