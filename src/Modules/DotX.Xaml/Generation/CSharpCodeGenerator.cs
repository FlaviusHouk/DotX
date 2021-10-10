using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using DotX.Extensions;
using DotX.Interfaces;
using Microsoft.CSharp;

namespace DotX.Xaml.Generation
{
    internal partial class CSharpCodeGenerator : ICodeGenerator
    {
        private readonly ILogger _logger;
        private int _level;
        private bool _intentWritten;

        public object GenerateCodeForCodeDelegateCreateExpressiondelegateCreateExpression { get; private set; }

        public CSharpCodeGenerator(ILogger logger)
        {
            _logger = logger;
        }

        public string CreateEscapedIdentifier(string value) => value;

        public string CreateValidIdentifier(string value) => value;

        public void GenerateCodeFromCompileUnit(CodeCompileUnit e, TextWriter w, CodeGeneratorOptions o)
        {
            foreach (var ns in e.Namespaces.OfType<CodeNamespace>())
            {
                GenerateCodeFromNamespace(ns, w, o);
                WriteLine(w);
            }
        }

        public void GenerateCodeFromExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            switch (e)
            {
                case CodeArrayCreateExpression arrayCreateExpression:
                    GenerateCodeForCodeArrayCreateExpression(arrayCreateExpression, w, o);
                    break;
                case CodeSnippetExpression codeSnippetExpression:
                    GenerateCodeForCodeSnippetExpression(codeSnippetExpression, w, o);
                    break;
                case CodeBinaryOperatorExpression binaryOperatorExpression:
                    GenerateCodeForCodeBinaryOperatorExpression(binaryOperatorExpression, w, o);
                    break;
                case CodePropertySetValueReferenceExpression propertySetValueReferenceExpression:
                    GenerateCodeForCodePropertySetValueReferenceExpression(propertySetValueReferenceExpression, w, o);
                    break;
                case CodeArgumentReferenceExpression argumentReferenceExpression:
                    GenerateCodeForCodeArgumentReferenceExpression(argumentReferenceExpression, w, o);
                    break;
                case CodeEventReferenceExpression eventReferenceExpression:
                    GenerateCodeForCodeEventReferenceExpression(eventReferenceExpression, w, o);
                    break;
                case CodeTypeOfExpression typeOfExpression:
                    GenerateCodeForCodeTypeOfExpression(typeOfExpression, w, o);
                    break;
                case CodeDirectionExpression directionExpression:
                    GenerateCodeForCodeDirectionExpression(directionExpression, w, o);
                    break;
                case CodeFieldReferenceExpression fieldReferenceExpression:
                    GenerateCodeForCodeFieldReferenceExpression(fieldReferenceExpression, w, o);
                    break;
                case CodePrimitiveExpression primitiveExpression:
                    GenerateCodeForCodePrimitiveExpression(primitiveExpression, w, o);
                    break;
                case CodeIndexerExpression indexerExpression:
                    GenerateCodeForCodeIndexerExpression(indexerExpression, w, o);
                    break;
                case CodeMethodReferenceExpression methodReferenceExpression:
                    GenerateCodeForCodeMethodReferenceExpression(methodReferenceExpression, w, o);
                    break;
                case CodeTypeReferenceExpression typeReferenceExpression:
                    GenerateCodeForCodeTypeReferenceExpression(typeReferenceExpression, w, o);
                    break;
                case CodeDelegateInvokeExpression delegateInvokeExpression:
                    GenerateCodeForCodeDelegateInvokeExpression(delegateInvokeExpression, w, o);
                    break;
                case CodeThisReferenceExpression thisReferenceExpression:
                    GenerateCodeForCodeThisReferenceExpression(thisReferenceExpression, w, o);
                    break;
                case CodeDelegateCreateExpression delegateCreateExpression:
                    GenerateCodeForCodeDelegateCreateExpression(delegateCreateExpression, w, o);
                    break;
                case CodeArrayIndexerExpression arrayIndexerExpression:
                    GenerateCodeForCodeArrayIndexerExpression(arrayIndexerExpression, w, o);
                    break;
                case CodeBaseReferenceExpression baseReferenceExpression:
                    GenerateCodeForCodeBaseReferenceExpression(baseReferenceExpression, w, o);
                    break;
                case CodeParameterDeclarationExpression parameterDeclarationExpression:
                    GenerateCodeForCodeParameterDeclarationExpression(parameterDeclarationExpression, w, o);
                    break;
                case CodeDefaultValueExpression defaultValueExpression:
                    GenerateCodeForCodeDefaultValueExpression(defaultValueExpression, w, o);
                    break;
                case CodePropertyReferenceExpression propertyReferenceExpression:
                    GenerateCodeForCodePropertyReferenceExpression(propertyReferenceExpression, w, o);
                    break;
                case CodeCastExpression castExpression:
                    GenerateCodeForCodeCastExpression(castExpression, w, o);
                    break;
                case CodeOutVariableParameterExpression outReference:
                    GenerateCodeForOutParameter(outReference, w, o);
                    break;
                case CodeVariableReferenceExpression variableReferenceExpression:
                    GenerateCodeForCodeVariableReferenceExpression(variableReferenceExpression, w, o);
                    break;
                case CodeMethodInvokeExpression methodInvokeExpression:
                    GenerateCodeForCodeMethodInvokeExpression(methodInvokeExpression, w, o);
                    break;
                case CodeObjectCreateExpression objectCreateExpression:
                    GenerateCodeForCodeObjectCreateExpression(objectCreateExpression, w, o);
                    break;
                case CodeIsTypeExpression isTypeExpression:
                    GenerateCodeForCodeIsTypeExpression(isTypeExpression, w, o);
                    break;
                default:
                    if (!TryToHandleExpression(e, w, o))
                    {
                        throw new NotImplementedException();
                    }
                    break;
            }
        }

        public void GenerateCodeFromNamespace(CodeNamespace e, TextWriter w, CodeGeneratorOptions o)
        {
            foreach (var c in e.Comments.OfType<CodeCommentStatement>())
            {
                Write(w, "/*");
                WriteLine(w, c.Comment.Text);
                WriteLine(w, "*/");
            }

            Write(w, "namespace ");
            WriteLine(w, e.Name);
            OpenBrace(w);

            foreach (var t in e.Types.OfType<CodeTypeDeclaration>())
            {
                GenerateCodeFromType(t, w, o);
                WriteLine(w);
            }

            CloseBrace(w);
        }

        public void GenerateCodeFromStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            switch(e)
            {
                case CodeAssignStatement assignStatement:
                    GenerateCodeForAssignmentStatement(assignStatement, w, o);
                    break;
                case CodeAttachEventStatement attachEventStatement:
                    GenerateCodeForAttachEventStatement(attachEventStatement, w, o);
                    break;
                case CodeSnippetStatement snippetStatement:
                    GenerateCodeForCodeSnippetStatement(snippetStatement, w, o);
                    break;
                case CodeRemoveEventStatement removeEventStatement:
                    GenerateCodeForCodeRemoveEventStatement(removeEventStatement, w, o);
                    break;
                case CodeCommentStatement commentStatement:
                    GenerateCodeForCodeCommentStatement(commentStatement, w, o);
                    break;
                case CodeThrowExceptionStatement throwExceptionStatement:
                    GenerateCodeForCodeThrowExceptionStatement(throwExceptionStatement, w, o);
                    break;
                case CodeLabeledStatement labeledStatement:
                    GenerateCodeForCodeLabeledStatement(labeledStatement, w, o);
                    break;
                case CodeMethodReturnStatement returnStatement:
                    GenerateCodeForCodeMethodReturnStatement(returnStatement, w, o);
                    break;
                case CodeForeachStatement foreachStatement:
                    GenerateCodeForForeachStatement(foreachStatement, w, o);
                    break;
                case CodeIterationStatement iterationStatement:
                    GenerateCodeForCodeIterationStatement(iterationStatement, w, o);
                    break;
                case CodeTryCatchFinallyStatement tryCatchFinallyStatement:
                    GenerateCodeForCodeTryCatchFinallyStatement(tryCatchFinallyStatement, w, o);
                    break;
                case CodeExpressionStatement expressionStatement:
                    GenerateCodeForCodeExpressionStatement(expressionStatement, w, o);
                    break;
                case CodeGotoStatement gotoStatement:
                    GenerateCodeForCodeGotoStatement(gotoStatement, w, o);
                    break;
                case CodeVariableDeclarationStatement variableDeclarationStatement:
                    GenerateCodeForCodeVariableDeclarationStatement(variableDeclarationStatement, w, o);
                    break;
                case CodeConditionStatement conditionStatement:
                    GenerateCodeForCodeConditionStatement(conditionStatement, w, o);
                    break;
                default:
                    if(!TryToHandleStatement(e, w, o))
                    {
                        throw new NotImplementedException();
                    }
                    break;
            }
        }

        public void GenerateCodeFromType(CodeTypeDeclaration e, TextWriter w, CodeGeneratorOptions o)
        {
            if (e.IsClass)
            {
                Write(w, "public ");
                if (e.IsPartial)
                    Write(w, "partial ");

                Write(w, "class ");
            }
            else if (e.IsStruct)
            {
                Write(w, "public struct ");
            }

            WriteLine(w, e.Name);
            OpenBrace(w);

            _logger.LogWarning($"Having {e.Members.Count} members.");
            foreach (CodeTypeMember m in e.Members)
            {                
                GenerateCodeForMember(m, w, o);
                WriteLine(w);
            }

            CloseBrace(w);
        }

        public string GetTypeOutput(CodeTypeReference type)
        {
            throw new System.NotImplementedException();
        }

        public bool IsValidIdentifier(string value) => true;

        public bool Supports(GeneratorSupport supports) => true;

        public void ValidateIdentifier(string value)
        { }

        protected virtual bool TryToHandleExpression(CodeExpression e, TextWriter w, CodeGeneratorOptions o)
        {
            return false;
        }

        protected virtual bool TryToHandleStatement(CodeStatement e, TextWriter w, CodeGeneratorOptions o)
        {
            throw new NotImplementedException();
        }

        private void GenerateCodeForMember(CodeTypeMember member, TextWriter w, CodeGeneratorOptions o)
        {
            switch (member)
            {
                case CodeMemberMethod meth:
                    GenerateCodeForMemberMethod(meth, w, o);
                    break;
                case CodeMemberField field:
                    GenerateCodeForField(field, w, o);
                    break;
                case CodeMemberProperty prop:
                    GenerateCodeForProperty(prop, w, o);
                    break;
                case CodeTypeDeclaration typeDeclaration:
                    _logger.LogWarning("Skipping type declaration.");
                    break;
                default:
                    throw new NotImplementedException($"Cannot process {member.GetType().Name}.");
            }
        }

        private void GenerateCodeForProperty(CodeMemberProperty prop, TextWriter w, CodeGeneratorOptions o)
        {
            Write(w, GetAccessForAttribute(prop.Attributes));
            Write(w);
            Write(w, prop.Type.BaseType);
            Write(w, " ");
            WriteLine(w, prop.Name);
            OpenBrace(w);

            if (prop.HasGet)
            {
                Write(w, "get");

                if (prop.GetStatements.Count == 0)
                {
                    WriteLine(w, ";");
                }
                else
                {
                    foreach (var statement in prop.GetStatements.OfType<CodeStatement>())
                    {
                        GenerateCodeFromStatement(statement, w, o);
                    }

                    WriteLine(w, ";");
                }
            }

            if (prop.HasSet)
            {
                Write(w, "set");

                if (prop.SetStatements.Count == 0)
                {
                    WriteLine(w, ";");
                }
                else
                {
                    foreach (var statement in prop.SetStatements.OfType<CodeStatement>())
                    {
                        GenerateCodeFromStatement(statement, w, o);
                    }

                    WriteLine(w, ";");
                }
            }

            CloseBrace(w);
        }

        private void GenerateCodeForField(CodeMemberField field, TextWriter w, CodeGeneratorOptions o)
        {
            Write(w, GetAccessForAttribute(field.Attributes));
            Write(w);
            Write(w, field.Type.BaseType);
            Write(w);
            if (field.InitExpression is not null)
            {
                Write(w, field.Name);
                WriteLine(w, " = ");

                GenerateCodeFromExpression(field.InitExpression, w, o);

                WriteLine(w, ";");
                return;
            }

            Write(w, field.Name);
            WriteLine(w, ";");
        }

        private void GenerateCodeForMemberMethod(CodeMemberMethod meth, TextWriter w, CodeGeneratorOptions o)
        {
            Write(w, GetAccessForAttribute(meth.Attributes));
            Write(w);

            if(meth.UserData.Contains("IsPartial") &&
               (bool)meth.UserData["IsPartial"])
            {
                Write(w, "partial ");
            }

            Write(w, meth.ReturnType.BaseType);
            Write(w);

            Write(w, meth.Name);
            Write(w, "(");

            int paramCount = meth.Parameters.Count;

            if(paramCount > 0)
            {
                CodeParameterDeclarationExpression par;
                for(int i = 0; i < paramCount - 1; i++)
                {
                    par = meth.Parameters[i];
                
                    Write(w, ", ");
                }

                par = meth.Parameters[paramCount - 1];
                GenerateCodeForCodeParameterDeclarationExpression(par, w, o);
            }

            WriteLine(w, ")");

            OpenBrace(w);
            for(int i = 0; i < meth.Statements.Count; i++)
            {
                GenerateCodeFromStatement(meth.Statements[i], w, o);
            }
            CloseBrace(w);
        }

        private string GetAccessForAttribute(MemberAttributes attributes)
        {
            if (attributes.HasFlag(MemberAttributes.Public))
                return "public";
            else if (attributes.HasFlag(MemberAttributes.Family))
                return "protected";

            return "private";
        }

        private string GetParamModifier(FieldDirection direction)
        {
            return direction switch
            {
                FieldDirection.Out => "out ",
                FieldDirection.Ref => "ref ",
                _ => string.Empty
            };
        }

        private void OpenBrace(TextWriter w)
        {
            WriteLine(w, "{");
            _level++;
        }

        private void CloseBrace(TextWriter w)
        {
             _level--;
            WriteLine(w, "}");
        }

        private void Write(TextWriter w, string content = " ")
        {
            if (!_intentWritten)
            {
                string intent = new string(' ', _level * 4);
                w.Write(intent);
            }

            _intentWritten = true;
            w.Write(content);
        }

        private void WriteLine(TextWriter w, string content = "")
        {
            if (!_intentWritten)
            {
                string intent = new string(' ', _level * 4);
                w.Write(intent);
            }

            w.WriteLine(content);
            _intentWritten = false;
        }

        private void WriteType(TextWriter w, CodeTypeReference typeRef)
        {
            Write(w, typeRef.BaseType.Split('`')[0]);
            
            if(typeRef.TypeArguments.Count > 0)
            {
                Write(w, "<");

                int argCount = typeRef.TypeArguments.Count;
                for(int i = 0; i < argCount - 1; i++)
                {
                    WriteType(w, typeRef.TypeArguments[i]);
                    Write(w, ", ");
                }

                WriteType(w, typeRef.TypeArguments[argCount - 1]);
                Write(w, ">");
            }
        }

        private void WriteExpressionCollection(TextWriter w, 
                                               CodeExpressionCollection collection,
                                               CodeGeneratorOptions o)
        {
            CodeExpression elem;
            int indexCount = collection.Count;
            for(int i = 0; i < indexCount - 1; i++)
            {
                elem = collection[i];
                GenerateCodeFromExpression(elem, w, o);
                Write(w, ",");
            }

            elem = collection[indexCount - 1];
            GenerateCodeFromExpression(elem, w, o);
        }
    }
}