using System;
using System.Collections.Generic;
using DotX.Interfaces;

namespace DotX.Xaml.Generation
{
    internal class StyledPropertyValue : IPropertyValue
    {
        private enum ValueType
        {
            String,
            SingleObject,
            Collection
        }

        private ValueType _valueType;
        private readonly string _rawValue;
        private readonly XamlObject _object;
        private readonly IEnumerable<XamlObject> _collection;

        private StyledPropertyValue(ValueType valueType)
        {
            _valueType = valueType;
        }

        public StyledPropertyValue(string rawValue) : 
            this(ValueType.String)
        {
            _rawValue = rawValue;
        }

        public StyledPropertyValue(XamlObject obj) :
            this(ValueType.SingleObject)
        {
            _object = obj;
        }

        public StyledPropertyValue(IEnumerable<XamlObject> objects) :
            this(ValueType.Collection)
        {
            _collection = objects;
        }

        public T GetValue<T>()
        {
            if(_valueType != ValueType.String)
                throw new NotImplementedException();

            var converter = Converters.Converters.GetConverterForType(typeof(T));
            return (T)converter.Convert(_rawValue, typeof(T));
        }

        public bool Is<T>()
        {
            if(_valueType != ValueType.String)
                throw new NotImplementedException();

            return Converters.Converters.TryGetConverterForType(typeof(T), out var _);
        }

        public T SetValue<T>(T value)
        {
            throw new System.NotImplementedException();
        }
    }
}