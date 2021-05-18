namespace DotX.Abstraction
{
    public interface IInputElement
    {
        void OnPointerEnter(PointerMoveEventArgs eventArgs);

        void OnPointerMove(PointerMoveEventArgs pointerMoveEventArgs);
        
        void OnPointerLeave(PointerMoveEventArgs eventArgs);
    }
}