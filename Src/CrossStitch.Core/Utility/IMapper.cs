namespace CrossStitch.Core.Utility
{
    public interface IMapper<in TSource, out TDest>
    {
        TDest Map(TSource source);
    }
}
