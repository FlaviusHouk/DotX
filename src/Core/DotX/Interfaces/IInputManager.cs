using DotX.Data;

namespace DotX.Interfaces
{
    public interface IInputManager
    {
        void DispatchPointerMove(Visual visualToTest, PointerMoveEventArgs pointerMoveEventArgs);

        void DispatchKeyEvent(IRootVisual root, KeyEventArgs args);

        void DispatchPointerEvent(IRootVisual windowControl, PointerButtonEvent args);

        string MapKeyboarKeyValue(KeyEventArgs keyEventArgs);
    }
}