using System;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace DotX.Xaml.Generation
{
    internal partial class CSharpCodeGenerator
    {
        private void GenerateCodeForOutParameter(CodeOutVariableParameterExpression e,
                                                 TextWriter w,
                                                 CodeGeneratorOptions o)
        {
            Write(w, "out ");
            GenerateCodeForCodeVariableReferenceExpression(e, w, o);
        }

        private void GenerateCodeForCodeIsTypeExpression(CodeIsTypeExpression e,
                                                         TextWriter w,
                                                         CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(e.Target, w, o);
            Write(w, " is ");
            WriteType(w, e.TypeReference);

            if(e.VariableName is not null)
            {
                Write(w);
                Write(w, e.VariableName);
            }
        }

        private void GenerateCodeForCodeDelegateCreateExpression(CodeDelegateCreateExpression delegateCreateExpression, 
                                                                 TextWriter w, 
                                                                 CodeGeneratorOptions o)
        {
            Write(w, "new ");
            WriteType(w, delegateCreateExpression.DelegateType);
            Write(w, "(");
            GenerateCodeFromExpression(delegateCreateExpression.TargetObject, w, o);
            Write(w, ".");
            Write(w, delegateCreateExpression.MethodName);
            Write(w, ")");
        }

        private void GenerateCodeForCodeObjectCreateExpression(CodeObjectCreateExpression objectCreateExpression, 
                                                               TextWriter w, 
                                                               CodeGeneratorOptions o)
        {
            Write(w, "new ");
            WriteType(w, objectCreateExpression.CreateType);
            Write(w, "(");

            CodeExpression par;
            int paramCount = objectCreateExpression.Parameters.Count;
            if (paramCount > 0)
            {
                for (int i = 0; i < paramCount - 1; i++)
                {
                    par = objectCreateExpression.Parameters[i];
                    GenerateCodeFromExpression(par, w, o);
                    Write(w, ",");
                }

                par = objectCreateExpression.Parameters[paramCount - 1];
                GenerateCodeFromExpression(par, w, o);
            }
            Write(w, ")");
        }

        private void GenerateCodeForCodeMethodInvokeExpression(CodeMethodInvokeExpression methodInvokeExpression, 
                                                               TextWriter w, 
                                                               CodeGeneratorOptions o)
        {
            GenerateCodeForCodeMethodReferenceExpression(methodInvokeExpression.Method, w, o);
            Write(w, "(");
            
            CodeExpression param;
            int paramCount = methodInvokeExpression.Parameters.Count;
            if (paramCount > 0)
            {
                for (int i = 0; i < paramCount - 1; i++)
                {
                    param = methodInvokeExpression.Parameters[i];
                    GenerateCodeFromExpression(param, w, o);
                    Write(w, ",");
                }

                param = methodInvokeExpression.Parameters[paramCount - 1];
                GenerateCodeFromExpression(param, w, o);
            }
            Write(w, ")");
        }

        private void GenerateCodeForCodeVariableReferenceExpression(CodeVariableReferenceExpression variableReferenceExpression, 
                                                                    TextWriter w,
                                                                    CodeGeneratorOptions o)
        {
            Write(w, variableReferenceExpression.VariableName);
        }

        private void GenerateCodeForCodeCastExpression(CodeCastExpression castExpression,
                                                       TextWriter w,
                                                       CodeGeneratorOptions o)
        {
            Write(w, "(");
            WriteType(w, castExpression.TargetType);
            Write(w, ")");
            GenerateCodeFromExpression(castExpression.Expression, w, o);
        }

        private void GenerateCodeForCodePropertyReferenceExpression(CodePropertyReferenceExpression propertyReferenceExpression, 
                                                                    TextWriter w,
                                                                    CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(propertyReferenceExpression.TargetObject, w, o);
            Write(w, ".");
            Write(w, propertyReferenceExpression.PropertyName);
        }

        private void GenerateCodeForCodeDefaultValueExpression(CodeDefaultValueExpression defaultValueExpression, 
                                                               TextWriter w,
                                                               CodeGeneratorOptions o)
        {
            Write(w, "default(");
            WriteType(w, defaultValueExpression.Type);
            Write(w, ")");
        }

        private void GenerateCodeForCodeParameterDeclarationExpression(CodeParameterDeclarationExpression parameterDeclarationExpression, 
                                                                       TextWriter w,
                                                                       CodeGeneratorOptions o)
        {
            WriteType(w, parameterDeclarationExpression.Type);
            Write(w);
            Write(w, parameterDeclarationExpression.Name);
        }

        private void GenerateCodeForCodeBaseReferenceExpression(CodeBaseReferenceExpression baseReferenceExpression,
                                                                TextWriter w,
                                                                CodeGeneratorOptions o)
        {
            Write(w, "base");
        }

        private void GenerateCodeForCodeArrayIndexerExpression(CodeArrayIndexerExpression arrayIndexerExpression, 
                                                               TextWriter w,
                                                               CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(arrayIndexerExpression.TargetObject, w, o);
            Write(w, "[");

            CodeExpression index;
            int indexCount = arrayIndexerExpression.Indices.Count;
            for(int i = 0; i < indexCount - 1; i++)
            {
                index = arrayIndexerExpression.Indices[i];
                GenerateCodeFromExpression(index, w, o);
                Write(w, ",");
            }

            index = arrayIndexerExpression.Indices[indexCount - 1];
            GenerateCodeFromExpression(index, w, o);
            Write(w, "]");
        }

        private void GenerateCodeForCodeThisReferenceExpression(CodeThisReferenceExpression thisReferenceExpression,
                                                                TextWriter w,
                                                                CodeGeneratorOptions o)
        {
            Write(w, "this");
        }

        private void GenerateCodeForCodeDelegateInvokeExpression(CodeDelegateInvokeExpression delegateInvokeExpression,
                                                                 TextWriter w,
                                                                 CodeGeneratorOptions o)
        {
            if(delegateInvokeExpression.TargetObject is not null)
            {
                GenerateCodeFromExpression(delegateInvokeExpression.TargetObject, w, o);
            }

            Write(w, "(");
            
            CodeExpression param;
            int paramCount = delegateInvokeExpression.Parameters.Count;
            for(int i = 0; i < paramCount - 1; i++)
            {
                param = delegateInvokeExpression.Parameters[i];
                GenerateCodeFromExpression(param, w, o);
                Write(w, ",");
            }

            param = delegateInvokeExpression.Parameters[paramCount - 1];
            GenerateCodeFromExpression(param, w, o);
            Write(w, ")");
        }

        private void GenerateCodeForCodeTypeReferenceExpression(CodeTypeReferenceExpression typeReferenceExpression, 
                                                                TextWriter w,
                                                                CodeGeneratorOptions o)
        {
            WriteType(w, typeReferenceExpression.Type);
        }

        private void GenerateCodeForCodeMethodReferenceExpression(CodeMethodReferenceExpression methodReferenceExpression, 
                                                                  TextWriter w,
                                                                  CodeGeneratorOptions o)
        {
            if(methodReferenceExpression.TargetObject is not null)
            {
                GenerateCodeFromExpression(methodReferenceExpression.TargetObject, w, o);
                Write(w, ".");
            }

            Write(w, methodReferenceExpression.MethodName);
            if(methodReferenceExpression.TypeArguments?.Count > 0)
            {
                var paramNames = methodReferenceExpression.TypeArguments.OfType<CodeTypeParameter>()
                                                                        .Select(p => p.Name);

                Write(w, "<");
                Write(w, string.Join(',', paramNames));
                Write(w, ">");
            }
        }

        private void GenerateCodeForCodeIndexerExpression(CodeIndexerExpression indexerExpression, 
                                                          TextWriter w,
                                                          CodeGeneratorOptions o)
        {
            if(indexerExpression.TargetObject is not null)
            {
                GenerateCodeFromExpression(indexerExpression.TargetObject, w, o);
            }

            Write(w, "[");
            WriteExpressionCollection(w, indexerExpression.Indices, o);
            Write(w, "]");
        }

        private void GenerateCodeForCodePrimitiveExpression(CodePrimitiveExpression primitiveExpression,
                                                            TextWriter w,
                                                            CodeGeneratorOptions o)
        {
            switch(primitiveExpression.Value)
            {
                case string s:
                    Write(w, $"\"{s}\"");
                    break;

                default:

                    if(primitiveExpression.Value.GetType().IsEnum)
                    {
                        Write(w, primitiveExpression.Value.GetType().FullName);
                        Write(w, ".");
                    }

                    Write(w, primitiveExpression.Value.ToString());
                    break;
            }
        }

        private void GenerateCodeForCodeFieldReferenceExpression(CodeFieldReferenceExpression fieldReferenceExpression, 
                                                                 TextWriter w,
                                                                 CodeGeneratorOptions o)
        {
            if(fieldReferenceExpression.TargetObject is not null)
            {
                GenerateCodeFromExpression(fieldReferenceExpression.TargetObject, w, o);
                Write(w, ".");
            }

            Write(w, fieldReferenceExpression.FieldName);
        }

        private void GenerateCodeForCodeDirectionExpression(CodeDirectionExpression directionExpression, 
                                                            TextWriter w,
                                                            CodeGeneratorOptions o)
        {
            Write(w, GetParamModifier(directionExpression.Direction));
            GenerateCodeFromExpression(directionExpression.Expression, w, o);
        }

        private void GenerateCodeForCodeTypeOfExpression(CodeTypeOfExpression typeOfExpression, 
                                                         TextWriter w,
                                                         CodeGeneratorOptions o)
        {
            Write(w, "typeof (");
            WriteType(w, typeOfExpression.Type);
            Write(w, ")");
        }

        private void GenerateCodeForCodeEventReferenceExpression(CodeEventReferenceExpression eventReferenceExpression, 
                                                                 TextWriter w,
                                                                 CodeGeneratorOptions o)
        {
            if(eventReferenceExpression.TargetObject is not null)
            {
                GenerateCodeFromExpression(eventReferenceExpression.TargetObject, w, o);
                Write(w, ".");
            }

            Write(w, eventReferenceExpression.EventName);
        }

        private void GenerateCodeForCodeArgumentReferenceExpression(CodeArgumentReferenceExpression argumentReferenceExpression, 
                                                                    TextWriter w,
                                                                    CodeGeneratorOptions o)
        {
            Write(w, argumentReferenceExpression.ParameterName);
        }

        private void GenerateCodeForCodePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression propertySetValueReferenceExpression,
                                                                            TextWriter w,
                                                                            CodeGeneratorOptions o)
        {
            Write(w, "value");
        }

        private void GenerateCodeForCodeBinaryOperatorExpression(CodeBinaryOperatorExpression binaryOperatorExpression,
                                                                 TextWriter w,
                                                                 CodeGeneratorOptions o)
        {
            GenerateCodeFromExpression(binaryOperatorExpression.Left, w, o);
            Write(w);

            string val = binaryOperatorExpression.Operator switch
            {
                CodeBinaryOperatorType.Add => "+",
                CodeBinaryOperatorType.Assign => "=",
                CodeBinaryOperatorType.BitwiseAnd => "&",
                CodeBinaryOperatorType.BitwiseOr => "|",
                CodeBinaryOperatorType.BooleanAnd => "&&",
                CodeBinaryOperatorType.BooleanOr => "||",
                CodeBinaryOperatorType.Divide => "/",
                CodeBinaryOperatorType.GreaterThan => ">",
                CodeBinaryOperatorType.GreaterThanOrEqual => ">=",
                CodeBinaryOperatorType.IdentityEquality => "==",
                CodeBinaryOperatorType.IdentityInequality => "!=",
                CodeBinaryOperatorType.LessThan => "<",
                CodeBinaryOperatorType.LessThanOrEqual => "<=",
                CodeBinaryOperatorType.Modulus => "%",
                CodeBinaryOperatorType.Multiply => "*",
                CodeBinaryOperatorType.Subtract => "-",
                CodeBinaryOperatorType.ValueEquality => "==",
                _ => throw new NotImplementedException()
            };

            Write(w, val);
            Write(w);
            GenerateCodeFromExpression(binaryOperatorExpression.Right, w, o);
        }

        private void GenerateCodeForCodeSnippetExpression(CodeSnippetExpression codeSnippetExpression,
                                                          TextWriter w,
                                                          CodeGeneratorOptions o)
        {
            Write(w, codeSnippetExpression.Value);
        }

        private void GenerateCodeForCodeArrayCreateExpression(CodeArrayCreateExpression arrayCreateExpression,
                                                              TextWriter w,
                                                              CodeGeneratorOptions o)
        {
            Write(w, "new ");
            if(arrayCreateExpression.Initializers.Count > 0)
            {
                WriteType(w, arrayCreateExpression.CreateType);
                if (arrayCreateExpression.CreateType.ArrayRank == 0)
                {
                    Write(w, "[]");
                }

                OpenBrace(w);
                WriteExpressionCollection(w, arrayCreateExpression.Initializers, o);
                CloseBrace(w);
            }
            else
            {
                WriteType(w, arrayCreateExpression.CreateType);

                Write(w, "[");
                if (arrayCreateExpression.SizeExpression is not null)
                {
                    GenerateCodeFromExpression(arrayCreateExpression.SizeExpression, w, o);
                }
                else
                {
                    Write(w, arrayCreateExpression.Size.ToString());
                }
                Write(w, "]");

                //ArrayRank here?
            }
        }
    }
}