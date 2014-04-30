namespace Idecom.Bus.Utility
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class Reflect<T>
    {
        public static PropertyInfo GetProperty(Expression<Func<T, object>> property)
        {
            var info = GetMemberInfo(property, false) as PropertyInfo;
            if (info == null) throw new ArgumentException("Member is not a property");
            return info;
        }

        public static PropertyInfo GetProperty(Expression<Func<T, object>> property, bool checkForSingleDot)
        {
            return GetMemberInfo(property, checkForSingleDot) as PropertyInfo;
        }

        private static MemberInfo GetMemberInfo(Expression member, bool checkForSingleDot)
        {
            if (member == null) throw new ArgumentNullException("member");
            var lambda = member as LambdaExpression;
            if (lambda == null) throw new ArgumentException("Not a lambda expression", "member");

            MemberExpression memberExpr = null;
            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpr = ((UnaryExpression) lambda.Body).Operand as MemberExpression;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpr = lambda.Body as MemberExpression;
                    break;
            }

            if (memberExpr == null) throw new ArgumentException("Not a member access", "member");
            if (!checkForSingleDot) return memberExpr.Member;
            if (memberExpr.Expression is ParameterExpression)
                return memberExpr.Member;
            throw new ArgumentException("Argument passed contains more than a single dot which is not allowed: " + member, "member");
        }
    }
}