#pragma warning disable CS1591
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace WampSharp.Core.Utilities
{
    public static class Method
    {
        public static MethodInfo Get(Expression<Action> methodCall)
        {
            MethodCallExpression callExpression = methodCall.Body as MethodCallExpression;

            return callExpression.Method;
        }

        public static MethodInfo Get<T>(Expression<Action<T>> methodCall)
        {
            MethodCallExpression callExpression = methodCall.Body as MethodCallExpression;

            return callExpression.Method;
        }
    }
}