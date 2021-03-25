namespace DotX.Abstraction
{
    public interface IPlatform
    {
        IWindowImpl CreateWindow();
        void ListenToEvents();
    }
}