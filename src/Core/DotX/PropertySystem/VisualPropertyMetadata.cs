using System;
using System.Collections.Generic;

namespace DotX.PropertySystem
{
    public class VisualPropertyMetadata<TOwner, TValue> : PropertyMetadata<TOwner, TValue>
        where TOwner : CompositeObject
    {
        public VisualPropertyMetadata(PropertyOptions options,
                                      TValue defaultValue, 
                                      Func<TOwner, TValue, TValue> coerceFunc = default, 
                                      Action<TOwner, TValue, TValue> changeValueFunc = default) :
            base(options, defaultValue, coerceFunc, changeValueFunc)
        {}

        public override void Changed<T>(CompositeObject obj, T oldVal, T newVal)
        {
            if(EqualityComparer<T>.Default.Equals(oldVal, newVal))
                return;
                
            if(obj is not Visual v)
                throw new InvalidOperationException();

            if(Options.HasFlag(PropertyOptions.AffectsMeaure))
                v.InvalidateMeasure();

            if(Options.HasFlag(PropertyOptions.AffectsArrange))
                v.InvalidateArrange();

            if(Options.HasFlag(PropertyOptions.AffectsRender))
                v.Invalidate();

            if(Options.HasFlag(PropertyOptions.AffectsParentRender))
                v.VisualParent?.Invalidate();

            base.Changed(obj, oldVal, newVal);
        }
    }
}