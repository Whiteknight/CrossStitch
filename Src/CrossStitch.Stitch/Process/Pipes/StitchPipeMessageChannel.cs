using System.IO.Pipes;

namespace CrossStitch.Stitch.Process.Pipes
{
    public class StitchPipeMessageChannel : PipeMessageChannel, IMessageChannel
    {
        private readonly NamedPipeClientStream _outPipe;
        private readonly NamedPipeClientStream _inPipe;

        public StitchPipeMessageChannel(string pipeName)
        {
            _outPipe = new NamedPipeClientStream(".", pipeName + "[in]", PipeDirection.Out);
            _outPipe.Connect();
            _inPipe = new NamedPipeClientStream(".", pipeName + "[out]", PipeDirection.In);
            _inPipe.Connect();
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
            _inPipe?.Dispose();
            _outPipe?.Dispose();
        }
    }
}