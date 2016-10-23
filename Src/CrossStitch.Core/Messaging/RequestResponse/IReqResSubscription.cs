namespace CrossStitch.Core.Messaging.RequestResponse
{
    public interface IRequest<TResponse>
    {
    }

    public interface IReqResSubscription<in TRequest, out TResponse>
        where TRequest : IRequest<TResponse>
    {
        bool CanHandle(TRequest request);
        TResponse Request(TRequest request);
    }
}