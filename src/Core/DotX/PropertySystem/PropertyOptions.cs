using System;

namespace DotX.PropertySystem
{
    [Flags]
    public enum PropertyOptions
    {
        None = 0,
        Inherits = 1,
        AvailableForChildren = 1<<1,
        AffectsMeaure = 1<<2,
        AffectsArrange = 1<<3,
        AffectsRender = 1<<4,
        AffectsParentRender = 1<<5
    }
}