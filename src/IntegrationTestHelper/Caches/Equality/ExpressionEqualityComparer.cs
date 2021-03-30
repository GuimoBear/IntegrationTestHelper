using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTestHelper.Caches.Equality
{
	public class ExpressionEqualityComparer : IEqualityComparer<Expression>
	{
		public static ExpressionEqualityComparer Instance = new ExpressionEqualityComparer();

		public bool Equals(Expression a, Expression b)
		{
			return new ExpressionComparison(a, b).AreEqual;
		}

		public int GetHashCode(Expression expression)
		{
			return new HashCodeCalculation(expression).HashCode;
		}
	}
}
