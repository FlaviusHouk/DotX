using System.CodeDom;
using System.Collections.Generic;

namespace DotX.Xaml.Generation
{
    internal class CodeForeachStatement : CodeStatement
    {
        public CodeExpression CollectionSource { get; }
        public CodeVariableReferenceExpression Var { get; }

        public IReadOnlyCollection<CodeStatement> Statements { get; }

        public CodeForeachStatement(CodeExpression collectionSource,
                                    CodeVariableReferenceExpression iterateVar,
                                    IReadOnlyCollection<CodeStatement> statements)
        {
            CollectionSource = collectionSource;
            Var = iterateVar;
            Statements = statements;
        }
    }
}