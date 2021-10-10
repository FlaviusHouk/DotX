using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotX.Interfaces;
using DotX.Attributes;

namespace DotX.Xaml
{
    public class XamlObject
    {
        private List<XamlProperty> _props = new List<XamlProperty>();
        private List<XamlObject> _children = new List<XamlObject>();

        public Type ObjType { get; }

        internal IReadOnlyCollection<XamlProperty> Properties => _props;

        internal IReadOnlyCollection<XamlObject> Children => _children;

        public XamlObject(Type objType)
        {
            if(objType is null)
                throw new ArgumentNullException();

            ObjType = objType;
        }

        internal void AddProperty(XamlProperty prop)
        {
            prop.Invalidate(ObjType);

            _props.Add(prop);
        }

        internal void AddToContent(XamlObject child)
        {
            var attr = ObjType.GetCustomAttributes<ContentMemberAttribute>(true)
                              .First();
            
            if (!attr.IsMethod)
            {
                PropertyInfo contentProp = ObjType.GetProperty(attr.MemberName);

                if (contentProp is null)
                {
                    throw new Exception($"There is no content property for {ObjType.FullName}.");
                }
            }

            _children.Add(child);
        }
    }
}