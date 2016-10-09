namespace CrossStitch.Core.Messaging.RequestResponse
{
    public interface IRequest<TResponse>
    {
    }

    public interface IReqResSubscription<in TRequest, out TResponse>
        where TRequest : IRequest<TResponse>
    {
        TResponse Request(TRequest request);
    }
}