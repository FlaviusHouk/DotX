using System;

namespace DotX.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentMemberAttribute : Attribute
    {
        public string MemberName { get; }
        public bool IsMethod { get; }

        public ContentMemberAttribute(string name, bool isMethod = false)
        {
            MemberName = name;
            IsMethod = isMethod;
        }
    }
}