namespace DotX.Interfaces
{
    //I think it should be removed in the future.
    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}