﻿#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WampSharp.V2.Rpc;

namespace WampSharp.CodeGeneration
{
    public class CalleeProxyCodeGenerator
    {
        private readonly string mNamespaceName;

        private readonly string mClassDeclarationTemplate =
@"using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using WampSharp.V2;
using WampSharp.V2.CalleeProxy;
using WampSharp.V2.Client;

namespace {$namespace}
{
    //------------------------------------------------------------------------------
    // <auto-generated>
    //     This code was generated by a tool.
    //
    //     Changes to this file may cause incorrect behavior and will be lost if
    //     the code is regenerated.
    // </auto-generated>
    //------------------------------------------------------------------------------
    public class {$proxyName}Proxy : CalleeProxyBase, {$implementedInterface}
    {
{$fields}

        public {$proxyName}Proxy
                (IWampRealmProxy realmProxy,
                 ICalleeProxyInterceptor interceptor)
            : base(realmProxy, interceptor)
        {
        }
{$implementedMethods}
    }
}";
        public CalleeProxyCodeGenerator(string namespaceName)
        {
            mNamespaceName = namespaceName;
        }

        public string GenerateCode(Type interfaceType)
        {
            List<string> methods = new List<string>();
            List<string> fields = new List<string>();

            int methodIndex = 0;

            foreach (MethodInfo method in GetMethods(interfaceType))
            {
                IProxyMethodWriter writer = GetWriter(method);

                string methodCode = writer.WriteMethod(methodIndex, method);
                string methodField = writer.WriteField(methodIndex, method);
                methods.Add(methodCode);
                fields.Add(methodField);
                methodIndex++;
            }

            string joinedMethods = JoinLines(methods);
            string joinedFields = JoinLines(fields);

            IDictionary<string, string> dictionary = 
                new Dictionary<string, string>();

            dictionary["namespace"] = mNamespaceName;
            dictionary["fields"] = joinedFields;
            dictionary["implementedMethods"] = joinedMethods;
            dictionary["implementedInterface"] = FormatTypeExtensions.FormatType(interfaceType);
            dictionary["proxyName"] = GetInterfaceName(interfaceType);

            string processed = CodeGenerationHelper.ProcessTemplate(mClassDeclarationTemplate, dictionary);

            return processed;
        }

        private IEnumerable<MethodInfo> GetMethods(Type interfaceType)
        {
            foreach (Type type in GetTypesToExplore(interfaceType))
            {
                foreach (MethodInfo method in type.GetPublicInstanceMethods())
                {
                    yield return method;
                }
            }
        }

        private IEnumerable<Type> GetTypesToExplore(Type interfaceType)
        {
            yield return interfaceType;

            foreach (Type implementedInterface in interfaceType.GetInterfaces())
            {
                yield return implementedInterface;
            }
        }

        private IProxyMethodWriter GetWriter(MethodInfo method)
        {
            ValidateMethod(method);

            if (method.GetParameters().Any(x => x.IsOut || x.ParameterType.IsByRef))
            {
                return new OutRefProxyMethodWriter();
            }

            return new SimpleProxyMethodWriter();
        }

        private static void ValidateMethod(MethodInfo method)
        {
            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                MethodInfoValidation.ValidateSyncMethod(method);
            }
            else
            {
                if (method.IsDefined(typeof(WampProgressiveResultProcedureAttribute)))
                {
                    MethodInfoValidation.ValidateProgressiveMethod(method);
                }
                else
                {
                    MethodInfoValidation.ValidateAsyncMethod(method);
                }
            }
        }

        private static string JoinLines(List<string> lines)
        {
            string joinedLines =
                string.Join(Environment.NewLine, lines);

            joinedLines =
                string.Join(Environment.NewLine,
                            joinedLines.Split(new[] {Environment.NewLine}, StringSplitOptions.None)
                                         .Select(x => "        " + x));
            return joinedLines;
        }

        private string GetInterfaceName(Type interfaceType)
        {
            string result = interfaceType.Name;

            if (result.StartsWith("I"))
            {
                return result.Substring(1);
            }
            else
            {
                return result;
            }
        }
    }
}