namespace DotX.Interfaces
{
    public interface ILogger
    {
        void Log(string category, 
                 LogLevel level, 
                 string format, 
                 params object[] parameters);
    }
}