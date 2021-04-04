namespace DotX.Abstraction
{
    public interface IPlatform
    {
        IWindowImpl CreateWindow(int width, int height);
        void ListenToEvents();
    }
}