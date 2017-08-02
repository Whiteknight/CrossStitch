using System.IO.Pipes;

namespace CrossStitch.Stitch.Process.Pipes
{
    public class CorePipeMessageChannel : PipeMessageChannel, IMessageChannel
    {
        private readonly NamedPipeServerStream _outPipe;
        private readonly NamedPipeServerStream _inPipe;

        public CorePipeMessageChannel(string pipeName)
        {
            _inPipe = new NamedPipeServerStream(pipeName + "[in]", PipeDirection.In, 1, PipeTransmissionMode.Byte);
            _inPipe.WaitForConnection();
            _outPipe = new NamedPipeServerStream(pipeName + "[out]", PipeDirection.Out, 1, PipeTransmissionMode.Byte);
            _outPipe.WaitForConnection();
        }

        public string ReadMessage()
        {
            return ReadInternal(_inPipe);
        }

        public void Send(string message)
        {
            SendInternal(_outPipe, message);
        }

        public void Dispose()
        {
            _outPipe?.Dispose();
        }
    }
}
