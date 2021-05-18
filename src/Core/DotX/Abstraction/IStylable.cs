using System.Collections.Generic;

namespace DotX.Abstraction
{
    public interface IStylable
    {
        IList<string> Classes { get; }
    }
}