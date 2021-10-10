using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotX.Interfaces;
using DotX.Attributes;
using Microsoft.CSharp;
using System.CodeDom;
using DotX.Data;
using DotX.PropertySystem;
using DotX.Extensions;

namespace DotX.Xaml.Generation
{
    public class CodeGeneratorForObject
    {
        private readonly string _className;
        private readonly string _ns;
        private readonly ILogger _logger;
        private readonly Generation.CSharpCodeGenerator _generator;

        private readonly Dictionary<Type, int> _nameCounter =
            new Dictionary<Type, int>();

        public CodeGeneratorForObject(string className, string ns, ILogger logger)
        {
            _className = className;
            _ns = ns;
            _generator = new (logger);
            _logger = logger;
        }

        public void GenerateCodeForObject(XamlObject obj, TextWriter output)
        {
            CodeCompileUnit unit = new();
            CodeNamespace codeNamespace = new (_ns);
            
            CodeTypeDeclaration partClassDef = new (_className);
            partClassDef.IsPartial = true;
            
            CodeMemberMethod initializeMethod = new ();
            initializeMethod.UserData.Add("IsPartial", true);
            initializeMethod.Name = "LoadComponent";
            initializeMethod.ReturnType = new ("void");

            GenerateProperties(initializeMethod, obj, "this");

            var childObjects = obj.Children.Select(child => GenerateCodeForObjectCore(child, initializeMethod)).ToArray();

            AssignContent(obj, initializeMethod, "this", childObjects);

            partClassDef.Members.Add(initializeMethod);
            codeNamespace.Types.Add(partClassDef);

            unit.Namespaces.Add(codeNamespace);

            _generator.GenerateCodeFromCompileUnit(unit, output, new ());
        }

        private string GenerateCodeForObjectCore(XamlObject obj, 
                                                 CodeMemberMethod initializeMethod)
        {
            string objName = GetObjectName(obj.ObjType);
            //_provider.CreateValidIdentifier();

            initializeMethod.Statements.Add(GenerateAssignmentStatement(objName, obj.ObjType.FullName));

            GenerateProperties(initializeMethod, obj, objName);

            var childObjects = obj.Children.Select(child => GenerateCodeForObjectCore(child, initializeMethod)).ToArray();

            if(childObjects.Length > 0)
                AssignContent(obj, initializeMethod, objName, childObjects);

            return objName;
        }

        private void GenerateProperties(CodeMemberMethod initializeMethod,
                                        XamlObject target,
                                        string objName)
        {
            foreach(var prop in target.Properties)
            {
                var inlineProp = prop as InlineXamlProperty;
                var fullProp = prop as FullXamlProperty;
                var extendedProp = prop as ExtendedXamlProperty;
                var attachedProp = prop as AttachedXamlProperty;

                if(attachedProp is not null)
                {
                    continue;
                }
                else if(inlineProp is not null)
                {
                    PropertyInfo clrProp = target.ObjType.GetProperty(prop.PropertyName);
                    bool isCollection = clrProp.PropertyType.IsCollection();

                    string converterType = default;
                    if(Converters.Converters.TryGetConverterForType(prop.PropertyType, out var converter))
                        converterType = converter.GetType().FullName;

                    if(!clrProp.CanWrite)
                    {
                        if(!isCollection)
                            throw new InvalidOperationException($"Cannot set {prop.PropertyName} of type {target.ObjType.Name}.");

                        SetReadOnlyCollectionProp(objName,
                                                  clrProp,
                                                  converterType,
                                                  inlineProp.RawValue,
                                                  initializeMethod);

                        return;
                    }

                    object value = inlineProp.RawValue;
                    if(inlineProp.PropertyType.IsEnum &&
                       Enum.TryParse(inlineProp.PropertyType, inlineProp.RawValue, true, out var enumValue))
                    {
                        value = enumValue;
                    }

                    GeneratePrimitiveAssignmentStatement(objName,
                                                         clrProp,
                                                         converterType,
                                                         value,
                                                         initializeMethod);
                }
                else if (fullProp is not null)
                {
                    var clrProp = target.ObjType
                                        .GetProperty(prop.PropertyName);

                    if(clrProp is null)
                    {
                        throw new Exception($"Cannot find property {prop.PropertyName} on type {target.ObjType.Name}");
                    }

                    var children = fullProp.Children.Select(c => (GenerateCodeForObjectCore(c, initializeMethod), c))
                                                    .ToDictionary(c => c.Item1,
                                                                  c => c.c);

                    if(clrProp.CanWrite)
                    {
                        CodeStatement arrAsignment =
                            GenerateCollectionAssignmentStatement(objName,
                                                                  clrProp,
                                                                  children.Keys);

                        initializeMethod.Statements.Add(arrAsignment);
                    }
                    else
                    {
                        var collectionType = clrProp.PropertyType;
                        MethodInfo methodToAdd;
                        
                        if(collectionType == typeof(ResourceCollection))
                        {
                            var valuesToInsert = children.ToDictionary(c => c.Value.Properties.OfType<AttachedXamlProperty>().FirstOrDefault(p => p.Owner == "x").RawValue,
                                                                       c => c.Key);
                            
                            foreach(var addExpr in AddToResourceCollection(objName, clrProp.Name, valuesToInsert))
                                initializeMethod.Statements.Add(addExpr);
                        }
                        else if((methodToAdd = collectionType.GetMethod(nameof(ICollection<object>.Add))) is not null)
                        {
                            foreach(var child in AddToGenericCollection(objName, clrProp.Name, children.Keys))
                                initializeMethod.Statements.Add(child);
                        }
                    }  
                }
                else if (extendedProp is not null)
                {
                    string extObjName = GenerateCodeForObjectCore(extendedProp.Extension, initializeMethod);
                    var clrProp = target.GetType().GetProperty(extendedProp.PropertyName);


                    CodeVariableReferenceExpression extensionVar = new (extObjName);
                    CodeVariableReferenceExpression thisObj = new(objName);
                    CodePrimitiveExpression propName = new (extendedProp.PropertyName);
                    CodeMethodInvokeExpression getValueExpr = new (extensionVar, 
                                                                   nameof(IMarkupExtension.ProvideValue),
                                                                   thisObj,
                                                                   propName);
                                                                   
                    CodeVariableDeclarationStatement valueVar = 
                        new (typeof(object), 
                             $"{objName}_{extendedProp.PropertyName}_valueVar",
                             getValueExpr);

                    initializeMethod.Statements.Add(valueVar);

                    CodeVariableDeclarationStatement cpVar =
                        new(typeof(CompositeObjectProperty),
                            $"{objName}_{extendedProp.PropertyName}_var");

                    initializeMethod.Statements.Add(cpVar);

                    CodePropertyReferenceExpression propRef = new(thisObj, extendedProp.PropertyName);

                    CodeIsTypeExpression isCompositeObject =
                        new ()
                        {
                            Target = thisObj,
                            TypeReference = new (typeof(CompositeObject)),
                            VariableName = $"{objName}_comp"
                        };

                    CodeIsTypeExpression isPropVal =
                        new ()
                        {
                            Target = new CodeVariableReferenceExpression(valueVar.Name),
                            TypeReference = new (typeof(IPropertyValue)),
                            VariableName = $"{objName}_{extendedProp.PropertyName}_propVal"
                        };

                    CodeBinaryOperatorExpression canSetAsCPIsTypesCorrect =
                        new (isCompositeObject, 
                             CodeBinaryOperatorType.BooleanAnd,
                             isPropVal);

                    
                    CodeMethodInvokeExpression tryGetCP =
                        new (new CodeVariableReferenceExpression(isCompositeObject.VariableName),
                             nameof(CompositeObject.TryGetProperty),
                             new CodePrimitiveExpression(extendedProp.PropertyName),
                             new CodeOutVariableParameterExpression(cpVar.Name));

                    CodeBinaryOperatorExpression canSetAsCP =
                        new (canSetAsCPIsTypesCorrect,
                             CodeBinaryOperatorType.BooleanAnd,
                             tryGetCP);

                    CodeExpressionStatement callSetCPMethod = 
                        new(new CodeMethodInvokeExpression(thisObj,
                                                           nameof(CompositeObject.SetValue),
                                                           new CodeVariableReferenceExpression(cpVar.Name),
                                                           new CodeCastExpression(typeof(IPropertyValue),
                                                                                   new CodeVariableReferenceExpression(valueVar.Name))));
                    CodeAssignStatement setPropSt =
                        new(new CodePropertyReferenceExpression(thisObj,
                                                                extendedProp.PropertyName),
                            new CodeCastExpression(new CodeTypeReference(extendedProp.PropertyType),
                                                   new CodeVariableReferenceExpression (valueVar.Name)));

                    CodeConditionStatement conditionalSet = 
                        new (canSetAsCP,
                             new CodeStatement[] { callSetCPMethod },
                             new CodeStatement[] { setPropSt });

                    initializeMethod.Statements.Add(conditionalSet);
                }
            }
        }

        private void SetReadOnlyCollectionProp(string objName, 
                                               PropertyInfo clrPropInfo, 
                                               string converterType, 
                                               string rawValue,
                                               CodeMemberMethod initializeMethod)
        {
            CodeTypeReference converterTypeRef = 
                new(converterType);

            CodeTypeReference targetTypeRef =
                new(clrPropInfo.PropertyType);

            CodeVariableReferenceExpression currentObjectVar =
                new (objName);

            CodeVariableDeclarationStatement converterVar =
                new (converterTypeRef,
                     $"{objName}_{clrPropInfo.Name}_converter",
                     new CodeObjectCreateExpression(converterTypeRef));

            CodeVariableReferenceExpression covnverterVarRef = 
                new(converterVar.Name);

            initializeMethod.Statements.Add(converterVar);

            CodeMethodInvokeExpression convertValueCall =
                new (covnverterVarRef,
                     nameof(IValueConverter.Convert),
                     new CodePrimitiveExpression(rawValue),
                     new CodeTypeOfExpression(clrPropInfo.PropertyType.FullName));

            Type elemType = 
                typeof(IEnumerable<>).MakeGenericType(clrPropInfo.PropertyType.GetGenericArguments());
            
            CodeTypeReference enumerableTypeRef =
                new (elemType);

            CodeVariableDeclarationStatement convertedValueVar =
                new (enumerableTypeRef,
                     $"{objName}_{clrPropInfo.Name}_converted",
                     new CodeCastExpression(enumerableTypeRef,
                                            convertValueCall));

            initializeMethod.Statements.Add(convertedValueVar);

            CodePropertyReferenceExpression collectionToAddItems =
                new (currentObjectVar, 
                     clrPropInfo.Name);

            CodeVariableReferenceExpression iterator =
                new ("elem");

            CodeForeachStatement foreachStatement =
                new(new CodeVariableReferenceExpression(convertedValueVar.Name),
                    iterator,
                    new CodeStatement[]
                    {
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(collectionToAddItems,
                                                           nameof(ICollection<object>.Add),
                                                           iterator))
                    });

            initializeMethod.Statements.Add(foreachStatement);
        }

        private void AssignContent(XamlObject obj,
                                   CodeMemberMethod method,
                                   string objName,
                                   IReadOnlyCollection<string> children)
        {
            var attr = obj.ObjType.GetCustomAttributes<ContentMemberAttribute>(true)
                                  .First();

            CodeVariableReferenceExpression currentObject =
                new(objName);

            if(attr.IsMethod)
            {
                foreach(var childName in children)
                {
                    CodeVariableReferenceExpression childExpression =
                        new (childName);

                    CodeMethodInvokeExpression invokeMethod =
                        new (currentObject, 
                             attr.MemberName, 
                             childExpression);

                    method.Statements.Add(new CodeExpressionStatement(invokeMethod));
                }

                return;
            }
            
            CodePropertyReferenceExpression contentProp =
                new (currentObject, attr.MemberName);

            CodeVariableReferenceExpression singleChildExpression =
                new (children.SingleOrDefault() ??
                        throw new Exception($"No children for content for {objName} of type {obj.ObjType.Name}."));

            CodeAssignStatement assignStatement = 
                new (contentProp,
                     singleChildExpression);

            method.Statements.Add(assignStatement);
        }

        private string GetObjectName(Type objType)
        {
            if(!_nameCounter.TryGetValue(objType, out int counter))
            {
                _nameCounter.Add(objType, 1);
                
                return $"{objType.Name}_1";
            }

            counter++;
            var name = $"{objType.Name}_{counter}";
            _nameCounter[objType] = counter;

            return name;
        }

        private CodeStatement GenerateAssignmentStatement(string objName, string objTypeName)
        {
            CodeTypeReference reference = new (objTypeName);
            CodeVariableDeclarationStatement definition = new (reference, objName);
            definition.InitExpression = new CodeObjectCreateExpression (objTypeName);

            return definition;
        }

        private void GeneratePrimitiveAssignmentStatement(string objName,
                                                          PropertyInfo propInfo,
                                                          string converterType,
                                                          object value,
                                                          CodeMemberMethod method)
        {
            _logger.LogWarning($"Converter for {propInfo.Name} of type {propInfo.PropertyType.Name} is {converterType}.");
            CodeVariableReferenceExpression obj = new (objName);
            CodePropertyReferenceExpression prop = new (obj, propInfo.Name);

            CodeExpression expressionToSet;
            if (string.IsNullOrEmpty(converterType))
            {
                expressionToSet = new CodePrimitiveExpression(value);
            }
            else
            {
                string convVarName = $"{objName}_{propInfo.Name}_converter";
                CodeObjectCreateExpression convCreate = new(converterType);
                CodeVariableDeclarationStatement conv = new(converterType,
                                                             convVarName,
                                                             convCreate);

                method.Statements.Add(conv);

                CodeVariableReferenceExpression convVar = new(convVarName);
                CodePrimitiveExpression valueExpr = new(value);
                CodeTypeOfExpression typeOfExpression = new(propInfo.PropertyType);
                expressionToSet = new CodeMethodInvokeExpression(convVar,
                                                                 nameof(IValueConverter.Convert),
                                                                 valueExpr,
                                                                 typeOfExpression);

                expressionToSet = new CodeCastExpression(new CodeTypeReference(propInfo.PropertyType),
                                                         expressionToSet);
            }

            method.Statements.Add(new CodeAssignStatement(prop, expressionToSet));
        }

        private CodeStatement GenerateCollectionAssignmentStatement(string objName,
                                                                    PropertyInfo propInfo,
                                                                    IEnumerable<string> objects)
        {
            CodeVariableReferenceExpression obj = new (objName);
            CodePropertyReferenceExpression prop = new (obj, propInfo.Name);

            CodeArrayCreateExpression arr = new (propInfo.PropertyType.FullName,
                                                 objects.Select(obj => new CodeVariableReferenceExpression(obj))
                                                        .ToArray());

            return new CodeAssignStatement(prop, arr);
        }

        private IEnumerable<CodeStatement> AddToResourceCollection(string objectName,
                                                                   string propName,
                                                                   IDictionary<string, string> values)
        {
            CodeVariableReferenceExpression obj = new(objectName);
            CodePropertyReferenceExpression prop = new (obj, propName);
            foreach(var val in values)
            {
                CodeMethodInvokeExpression methodInvokation = 
                    new (prop, 
                         nameof(ResourceCollection.Add),
                         new CodePrimitiveExpression(val.Key),
                         new CodeVariableReferenceExpression(val.Value));

                yield return new CodeExpressionStatement(methodInvokation);
            }
        }

        private IEnumerable<CodeStatement> AddToGenericCollection(string objectName,
                                                                  string propName,
                                                                  IEnumerable<string> values)
        {
            CodeVariableReferenceExpression obj = new(objectName);
            CodePropertyReferenceExpression prop = new (obj, propName);
            foreach(var val in values)
            {
                CodeMethodInvokeExpression methodInvokation = 
                    new (prop, 
                         nameof(ICollection<object>.Add),
                         new CodeVariableReferenceExpression(val));

                yield return new CodeExpressionStatement(methodInvokation);
            }
        }
    }
}