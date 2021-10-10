using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace DotX.Xaml.Generation
{
    internal partial class CSharpCodeGenerator
    {
        private void GenerateCodeForCodeConditionStatement(CodeConditionStatement conditionStatement, 
                                                           TextWriter w, 
                                                           CodeGeneratorOptions o)
        {
            Write(w, "if (");
            GenerateCodeFromExpression(conditionStatement.Condition, w, o);
            WriteLine(w, ")");
            OpenBrace(w);
            foreach(CodeStatement s in conditionStatement.TrueStatements)
            {
                GenerateCodeFromStatement(s, w, o);
            }
            CloseBrace(w);

            if (conditionStatement.FalseStatements?.Count > 0)
            {
                Write(w, "else");
                OpenBrace(w);
                foreach (CodeStatement s in conditionStatement.FalseStatements)
                {
                    GenerateCodeFromStatement(s, w, o);
                }
                CloseBrace(w);
            }
        }

        private void GenerateCodeForCodeVariableDeclarationStatement(CodeVariableDeclarationStatement variableDeclarationStatement, 
                                                                     TextWriter w,
                                                                     CodeGeneratorOptions o)
        {
            WriteType(w, variableDeclarationStatement.Type);
            Write(w);
            Write(w, variableDeclarationStatement.Name);
            
            if(variableDeclarationStatement.InitExpression is not null)
            {
                Write(w, " = ");
                GenerateCodeFromExpression(variableDeclarationStatement.InitExpression, w, o);
            }

            WriteLine(w, ";");
        }

        private void GenerateCodeForCodeGotoStatement(CodeGotoStatement gotoStatement, 
                                                      TextWriter w,
                                                      CodeGeneratorOptions o)
        {
            throw new NotImplementedException();
        }

        private void GenerateCodeForCodeExpressionStatement(CodeExpressionStatement expressionStatement,
                                                            TextWriter w,
                                                            CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(expressionStatement.Expression, w, o);
            WriteLine(w, ";");
        }

        private void GenerateCodeForCodeTryCatchFinallyStatement(CodeTryCatchFinallyStatement tryCatchFinallyStatement,
                                                                 TextWriter w,
                                                                 CodeGeneratorOptions o)
        {
            WriteLine(w, "try");
            OpenBrace(w);
            foreach(CodeStatement s in tryCatchFinallyStatement.TryStatements)
            {
                GenerateCodeFromStatement(s, w, o);
            }
            CloseBrace(w);
            foreach(CodeCatchClause cc in tryCatchFinallyStatement.CatchClauses)
            {
                Write(w, "catch");

                if(cc.CatchExceptionType is not null)
                {
                    Write(w, " (");
                    WriteType(w, cc.CatchExceptionType);
                    
                    if(cc.LocalName is not null)
                    {
                        Write(w);
                        Write(w, cc.LocalName);
                    }

                    WriteLine(w, ")");
                }

                OpenBrace(w);

                foreach(CodeStatement s in cc.Statements)
                {
                    GenerateCodeFromStatement(s, w, o);
                }

                CloseBrace(w);
            }

            Write(w, "finally");
            OpenBrace(w);
            foreach(CodeStatement s in tryCatchFinallyStatement.FinallyStatements)
            {
                GenerateCodeFromStatement(s, w, o);
            }
            CloseBrace(w);
        }

        private void GenerateCodeForForeachStatement(CodeForeachStatement foreachStatement,
                                                     TextWriter w,
                                                     CodeGeneratorOptions o)
        {
            Write(w, "foreach (var ");
            Write(w, foreachStatement.Var.VariableName);
            Write(w, " in ");
            GenerateCodeFromExpression(foreachStatement.CollectionSource, w, o);
            Write(w, ")");
            OpenBrace(w);

            foreach(CodeStatement s in foreachStatement.Statements)
                GenerateCodeFromStatement(s, w, o);

            CloseBrace(w);
        }

        private void GenerateCodeForCodeIterationStatement(CodeIterationStatement iterationStatement,
                                                           TextWriter w,
                                                           CodeGeneratorOptions o)
        {
            Write(w, "for (");
            GenerateCodeFromStatement(iterationStatement.InitStatement, w, o);
            Write(w, "; ");
            GenerateCodeFromExpression(iterationStatement.TestExpression, w, o);
            Write(w, "; ");
            GenerateCodeFromStatement(iterationStatement.IncrementStatement, w, o);
            WriteLine(w, ")");
            OpenBrace(w);

            foreach(CodeStatement s in iterationStatement.Statements)
            {
                GenerateCodeFromStatement(s, w, o);
            }

            CloseBrace(w);
        }

        private void GenerateCodeForCodeMethodReturnStatement(CodeMethodReturnStatement returnStatement,
                                                              TextWriter w,
                                                              CodeGeneratorOptions o)
        {
            Write(w, "return ");
            GenerateCodeFromExpression(returnStatement.Expression, w, o);
            WriteLine(w, ";");
        }

        private void GenerateCodeForCodeLabeledStatement(CodeLabeledStatement labeledStatement,
                                                         TextWriter w,
                                                         CodeGeneratorOptions o)
        {
            Write(w, labeledStatement.Label);
            WriteLine(w, ":");

            if(labeledStatement.Statement is not null)
            {
                GenerateCodeFromStatement(labeledStatement.Statement, w, o);
            }
        }

        private void GenerateCodeForCodeThrowExceptionStatement(CodeThrowExceptionStatement throwExceptionStatement,
                                                                TextWriter w,
                                                                CodeGeneratorOptions o)
        {
            Write(w, "throw ");
            GenerateCodeFromExpression(throwExceptionStatement.ToThrow, w, o);
            WriteLine(w, ";");
        }

        private void GenerateCodeForCodeCommentStatement(CodeCommentStatement commentStatement,
                                                         TextWriter w,
                                                         CodeGeneratorOptions o)
        {
            Write(w, commentStatement.Comment.Text);
        }

        private void GenerateCodeForCodeRemoveEventStatement(CodeRemoveEventStatement removeEventStatement,
                                                             TextWriter w,
                                                             CodeGeneratorOptions o)
        {
            if(removeEventStatement.Event.TargetObject is not null)
            {
                GenerateCodeFromExpression(removeEventStatement.Event.TargetObject, w, o);
                Write(w, ".");
            }

            Write(w, removeEventStatement.Event.EventName);
            Write(w, " -= ");
            GenerateCodeFromExpression(removeEventStatement.Listener, w, o);
            WriteLine(w, ";");
        }

        private void GenerateCodeForCodeSnippetStatement(CodeSnippetStatement snippetStatement,
                                                         TextWriter w,
                                                         CodeGeneratorOptions o)
        {
            Write(w, snippetStatement.Value);
        }

        private void GenerateCodeForAttachEventStatement(CodeAttachEventStatement attachEventStatement,
                                                         TextWriter w,
                                                         CodeGeneratorOptions o)
        {
            if(attachEventStatement.Event.TargetObject is not null)
            {
                GenerateCodeFromExpression(attachEventStatement.Event.TargetObject, w, o);
                Write(w, ".");
            }

            Write(w, attachEventStatement.Event.EventName);
            Write(w, " += ");
            GenerateCodeFromExpression(attachEventStatement.Listener, w, o);
            WriteLine(w, ";");
        }

        private void GenerateCodeForAssignmentStatement(CodeAssignStatement assignStatement, 
                                                        TextWriter w,
                                                        CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(assignStatement.Left, w, o);
            Write(w, " = ");
            GenerateCodeFromExpression(assignStatement.Right, w, o);
            WriteLine(w, ";");
        }
    }
}