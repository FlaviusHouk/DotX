using System;

namespace DotX
{
    [Flags]
    public enum PropertyOptions
    {
        Inherits = 1,
        AffectsMeaure = 1<<1,
        AffectsArrange = 1<<2,
        AffectsRender = 1<<3
    }
}