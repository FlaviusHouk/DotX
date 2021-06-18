namespace DotX.Data
{
    public class PointerButtonEvent : EventArgsWithCoords
    {
        public int Key { get; }
        public bool IsPressed { get; }

        public PointerButtonEvent(int x,
                                  int y,
                                  int key,
                                  bool isPressed) :
            base(x, y)
        {
            Key = key;
            IsPressed = isPressed;
        }
    }
}