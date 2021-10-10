using System.CodeDom;

namespace DotX.Xaml.Generation
{
    internal class CodeIsTypeExpression : CodeExpression
    {
        public CodeExpression Target { get; set; }
        public CodeTypeReference TypeReference { get; set; }

        public string VariableName { get; set; }
    }
}