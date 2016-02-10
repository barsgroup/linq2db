using System;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToDB.Expressions
{
	using LinqToDB.Extensions;

	public static class MemberHelper
	{
	    public static MemberInfo GetMemeberInfo(LambdaExpression func)
	    {
	        var ex = func.Body;

	        var unaryExpression = ex as UnaryExpression;
	        if (unaryExpression != null)
	            ex = unaryExpression.Operand;

	        var newExpression = ex as NewExpression;
	        if (newExpression != null)
	            return newExpression.Constructor;

	        return (ex as MemberExpression)?.Member ?? (ex as MethodCallExpression)?.Method;
	    }

	    public static MemberInfo MemberOf<T>(Expression<Func<T,object>> func)
		{
			return GetMemeberInfo(func);
		}

		public static FieldInfo FieldOf<T>(Expression<Func<T,object>> func)
		{
			return (FieldInfo)GetMemeberInfo(func);
		}

		public static PropertyInfo PropertyOf<T>(Expression<Func<T,object>> func)
		{
			return (PropertyInfo)GetMemeberInfo(func);
		}

		public static MethodInfo MethodOf<T>(Expression<Func<T,object>> func)
		{
			var mi = GetMemeberInfo(func);
		    var propertyInfo = mi as PropertyInfo;
		    return propertyInfo != null ? propertyInfo.GetGetMethodEx() : (MethodInfo)mi;
		}

		public static MethodInfo MethodOf(Expression<Func<object>> func)
		{
			var mi = GetMemeberInfo(func);
		    var propertyInfo = mi as PropertyInfo;
		    return propertyInfo != null ? propertyInfo.GetGetMethodEx() : (MethodInfo)mi;
		}

		public static ConstructorInfo ConstructorOf<T>(Expression<Func<T,object>> func)
		{
			return (ConstructorInfo)GetMemeberInfo(func);
		}

		public static ConstructorInfo ConstructorOf(Expression<Func<object>> func)
		{
			return (ConstructorInfo)GetMemeberInfo(func);
		}
	}
}
