using System.Collections.Generic;

namespace CrossStitch.Core
{
    public interface IBrokeredResponse<out TResponse>
    {
        IReadOnlyList<TResponse> Responses { get; }
    }
     
    public class BrokeredResponse<TResponse> : IBrokeredResponse<TResponse>
    {
        public BrokeredResponse(IReadOnlyList<TResponse> responses)
        {
            Responses = responses;
        }

        public IReadOnlyList<TResponse> Responses { get; private set; }
    }
}