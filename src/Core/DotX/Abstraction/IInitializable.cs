namespace DotX.Abstraction
{
    public interface IInitializable
    {
        bool IsInitialized { get; }

        void Initialize();
    }
}