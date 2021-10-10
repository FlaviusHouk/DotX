using System;
using System.Linq;
using DotX.Interfaces;
using DotX.PropertySystem;
using DotX.Attributes;

namespace DotX.Styling
{
    [ContentMember(nameof(Value))]
    public class Setter : IEquatable<Setter>
    {
        private bool _wasSet;
        private IPropertyValue _oldValue;

        public string Property { get; set;}

        public IPropertyValue Value { get; set; }

        public void SetValue(CompositeObject obj)
        {
            var propToSet = PropertyManager.Instance.GetProperties(obj.GetType())
                                                    .FirstOrDefault(p => p.PropName == Property);

            if(propToSet is null)
                throw new Exception();

            _wasSet = obj.IsPropertySet(propToSet);

            if(_wasSet)
                _oldValue = obj.GetValue<IPropertyValue>(propToSet);

            obj.SetValue(propToSet, Value);
        }

        public void UnsetValue(CompositeObject obj)
        {
            if(_wasSet)
            {
                var propToUnset = PropertyManager.Instance.GetProperties(obj.GetType())
                                                          .FirstOrDefault(p => p.PropName == Property);

                obj.SetValue(propToUnset, Value);
            
                _oldValue = CompositeObjectProperty.UnsetValue;

                _wasSet = false;
            }
        }

        public void TransferValue(Setter setter)
        {
            setter._wasSet = _wasSet;
            setter._oldValue = _oldValue;
        }

        public override bool Equals(object obj)
        {
            return obj is Setter setter && Equals(setter);
        }

        public bool Equals(Setter other)
        {
            return Property == other.Property;
        }

        public override int GetHashCode()
        {
            return Property.GetHashCode();
        }
    }
}