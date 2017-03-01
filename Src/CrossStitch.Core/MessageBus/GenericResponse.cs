namespace CrossStitch.Core.Messages
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
