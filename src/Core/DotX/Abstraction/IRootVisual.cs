namespace DotX.Abstraction
{
    public interface IRootVisual
    {
        IWindowImpl WindowImpl { get; }

        bool IsVisible { get; }

        void Show();
    }
}