using System.IO;
using System.IO.Pipes;
using System.Text;

namespace CrossStitch.Stitch.Process.Pipes
{
    public abstract class PipeMessageChannel
    {
        protected string ReadInternal(PipeStream pipe)
        {
            var stream = new MemoryStream();
            var buffer = new byte[1024];
            while(true)
            {
                int read = pipe.Read(buffer, 0, 1024);
                if (read > 0)
                    stream.Write(buffer, 0, read);
                if (read < 1024)
                    break;
            }
            stream.Seek(0, SeekOrigin.Begin);
            int length = (int)stream.Length;
            buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        protected void SendInternal(PipeStream pipe, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var buffer = Encoding.UTF8.GetBytes(message);
            if (buffer.Length > 0)
                pipe.Write(buffer, 0, buffer.Length);
        }

        public static string GetPipeName(string nodeId, string instanceId)
        {
            // TODO: Need any kind of sanitizing?
            return $"crossstitch[nid={nodeId}|iid={instanceId}]";
        }
    }
}