using System;

namespace DotX.Xaml
{
    internal class MarkupExtensionParser
    {
        private readonly string _value;
        private int _current;

        public MarkupExtensionParser(string value)
        {
            _value = value;
        }

        private char CurrentChar => _value[_current];

        public XamlObject ParseExtension(XamlParseContext context)
        {
            _current = -1;

            XamlObject obj = default;
            string attrName = string.Empty;
            string attrValue = string.Empty;

            while(MoveNext())
            {
                if(CurrentChar == '{')
                {
                    MoveNext();

                    while(char.IsWhiteSpace(CurrentChar))
                        MoveNext();

                    HandleRootCreation(ref obj, context);
                }
                else if(CurrentChar == '}')
                {
                    return obj;
                }
                else if(char.IsWhiteSpace(CurrentChar) ||
                        !string.IsNullOrEmpty(attrName) && CurrentChar == '=')
                {
                    continue;
                }
                else if (string.IsNullOrEmpty(attrName))
                {
                    attrName = ReadAttributeName();
                    _current--;
                }
                else if(!string.IsNullOrEmpty(attrName) && 
                        string.IsNullOrEmpty(attrValue))
                {
                    if(CurrentChar == '{')
                    {
                        var innerParser = new MarkupExtensionParser(_value.Substring(_current));
                        XamlObject innerExtension = innerParser.ParseExtension(context);
                        obj.AddProperty(new ExtendedXamlProperty(attrName, innerExtension));

                        _current += innerParser._current;
                    }
                    else
                    {
                        attrValue = ReadValue();
                        obj.AddProperty(new InlineXamlProperty(attrName, attrValue));
                        _current--;
                    }

                    attrName = attrValue = string.Empty;
                }
            }

            return obj;
        }

        private string ReadValue()
        {
            int nameStart = _current;
            while(!char.IsWhiteSpace(CurrentChar) && MoveNext())
            {}

            return _value.Substring(nameStart, _current - nameStart);
        }

        private string ReadAttributeName()
        {
            int nameStart = _current;
            while(char.IsLetter(CurrentChar) && MoveNext())
            {}

            return _value.Substring(nameStart, _current - nameStart);
        }

        private void HandleRootCreation(ref XamlObject obj, XamlParseContext context)
        {
            int nameStart = _current;
            while((char.IsLetter(CurrentChar) || CurrentChar == ':') && MoveNext())
            {}

            string typeName = _value.Substring(nameStart, _current - nameStart);
            string ns = string.Empty;
            if(typeName.Contains(':'))
            {
                var parts = typeName.Split(':');

                ns = parts[0];
                typeName = parts[1];
            }

            Type extensionType = context.LookupObjectByName(typeName, ns);
            if(extensionType is null)
                throw new Exception();

            obj = new XamlObject(extensionType);
        }

        private bool MoveNext()
        {
            _current++;

            return _value.Length > _current;
        }
    }
}