namespace DotX.Interfaces
{
    public interface IFocusable
    {
        bool CanFocus { get; }

        bool Focus();
    }
}