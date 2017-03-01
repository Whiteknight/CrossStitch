namespace CrossStitch.Core.MessageBus
{
    public class GenericResponse
    {
        public GenericResponse(bool success)
        {
            Success = success;
        }

        public bool Success { get; private set; }
    }
}
