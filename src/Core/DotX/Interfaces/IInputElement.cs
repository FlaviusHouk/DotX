using DotX.Data;

namespace DotX.Interfaces
{
    public interface IInputElement
    {
        void OnPointerEnter(PointerMoveEventArgs eventArgs);

        void OnPointerMove(PointerMoveEventArgs pointerMoveEventArgs);
        
        void OnPointerLeave(PointerMoveEventArgs eventArgs);

        void OnPointerButton(PointerButtonEvent buttonEvent);

        void OnKeyboardEvent(KeyEventArgs keyEvent);
    }
}