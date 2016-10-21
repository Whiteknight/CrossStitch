using System.Collections.Generic;

namespace CrossStitch.App.Networking
{
    public class MessageEnvelope
    {
        public const string SendEventName = "Send";
        public const string ReceiveEventName = "Receive";

        public MessageHeader Header { get; set; }
        public List<string> CommandStrings { get; set; }
        public List<object> PayloadObjects { get; set; }
        public List<byte[]> RawFrames { get; set; }

        public MessageEnvelope CreateEmptyResponse()
        {
            return new MessageEnvelope {
                Header = new MessageHeader {
                    FromId = Header.ToId,
                    FromType = Header.ToType,
                    ToId = Header.FromId,
                    ToType = Header.FromType,
                    MessageId = Header.MessageId,
                    PayloadType = MessagePayloadType.None,
                    ZoneName = Header.ZoneName
                }
            };
        }

        public MessageEnvelope CreateFailureResponse()
        {
            var response = CreateEmptyResponse();
            response.Header.PayloadType = MessagePayloadType.FailureResponse;
            return response;
        }

    }
}