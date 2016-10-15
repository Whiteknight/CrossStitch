namespace CrossStitch.App
{
    public interface IMapper<in TSource, out TDest>
    {
        TDest Map(TSource source);
    }
}
