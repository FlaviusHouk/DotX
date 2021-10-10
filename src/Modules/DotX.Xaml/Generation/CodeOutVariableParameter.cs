using System.CodeDom;

namespace DotX.Xaml.Generation
{
    internal class CodeOutVariableParameterExpression : CodeVariableReferenceExpression
    {
        public CodeOutVariableParameterExpression(string variableName) : 
            base(variableName)
        {}
    }
}