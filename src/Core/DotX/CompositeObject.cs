using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Interfaces;
using DotX.PropertySystem;

namespace DotX
{
    public class CompositeObject : IObservable<CompositeObjectProperty>
    {
        private ICollection<IObserver<CompositeObjectProperty>> _observers =
            new List<IObserver<CompositeObjectProperty>>();

        public CompositeObject()
        {
            var props = PropertyManager.Instance.GetProperties(GetType());
            ValueStorage.Storage.Init(this, props);
        }

        public T GetValue<T>(CompositeObjectProperty prop)
        {
            if(!CanSet(prop))
                throw new System.Exception();

            prop = PropertyManager.Instance.GetVirtualProperty(GetType(), prop);
                
            var propVal = ValueStorage.Storage.GetValue(this, prop);

            if(propVal == CompositeObjectProperty.UnsetValue)
            {
                if(prop.Metadata is IPropertyMetadata<T> typedMetadata)
                    return typedMetadata.DefaultValue;
                else
                    return default;
            }
               
            return propVal.GetValue<T>();
        }

        public IPropertyValue GetValue(CompositeObjectProperty prop)
        {
            if(!CanSet(prop))
                throw new System.Exception();

            prop = PropertyManager.Instance.GetVirtualProperty(GetType(), prop);
                
            return ValueStorage.Storage.GetValue(this, prop);
        }

        public void SetValue(CompositeObjectProperty prop, IPropertyValue value)
        {
            if(!CanSet(prop))
                throw new System.Exception();

            prop = PropertyManager.Instance.GetVirtualProperty(GetType(), prop);

            ValueStorage.Storage.SetValue(this, prop, value);
        }

        public void SetValue<T>(CompositeObjectProperty prop, T value)
        {
            if(!CanSet(prop))
                throw new Exception();

            prop = PropertyManager.Instance.GetVirtualProperty(GetType(), prop);

            if(!prop.PropertyType.IsAssignableFrom(value.GetType()))
                throw new InvalidCastException();

            value = prop.Metadata.Coerce(this, value);
            T oldValue = default;
            
            var oldPropValue = ValueStorage.Storage.GetValue(this, prop);
            if(oldPropValue == CompositeObjectProperty.UnsetValue)
            {
                oldPropValue = new PropertyValue<T>(value);
                ValueStorage.Storage.SetValue(this, prop, oldPropValue);

                if(prop.Metadata is IPropertyMetadata<T> typedMetadata)
                    oldValue = typedMetadata.DefaultValue;
            }
            else
            {
                oldValue = oldPropValue.GetValue<T>();
                oldPropValue.SetValue<T>(value);
                oldPropValue.OnChanged(this, prop);
            }

            prop.Metadata.Changed(this, oldValue, value);
            NotifyPropertyChanged(prop);
        }

        public bool TryGetProperty(string propName, out CompositeObjectProperty prop)
        {
            prop = PropertyManager.Instance.GetProperties(this.GetType())
                                           .FirstOrDefault(p => p.PropName == propName);

            return prop is not null;
        }

        public bool IsPropertySet(CompositeObjectProperty prop)
        {
            return ValueStorage.Storage.IsSet(this, prop);
        }

        private bool CanSet(CompositeObjectProperty prop)
        {
            return PropertyManager.Instance.IsPropertyAvailable(GetType(), prop);
        }

        public IDisposable Subscribe(IObserver<CompositeObjectProperty> observer)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));

            _observers.Add(observer);

            return new SubscriptionHolder<CompositeObjectProperty>(_observers, observer);
        }

        protected void NotifyPropertyChanged(CompositeObjectProperty prop)
        {
            foreach(var observer in _observers)
                observer.OnNext(prop);
        }
    }
}