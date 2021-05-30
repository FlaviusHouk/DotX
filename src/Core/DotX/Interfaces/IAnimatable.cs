namespace DotX.Interfaces
{
    public interface IAnimatable
    {
        public int Period { get; }
        public void Tick();
    }
}