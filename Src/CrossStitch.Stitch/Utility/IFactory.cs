namespace CrossStitch.Stitch.Utility
{
    public interface IFactory<T>
    {
        T Create();
    }

    public interface IFactory<T, TArg>
    {
        T Create(TArg arg);
    }
}
