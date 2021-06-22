namespace DotX.Interfaces
{
    public interface ITimeline
    {
        void Register(IAnimatable animatable);

        void Reset(IAnimatable animatable);
    }
}