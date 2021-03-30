using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTestHelper.Caches.Equality
{
	internal class ExpressionEnumeration : ExpressionVisitor, IEnumerable<Expression>
	{
		private List<Expression> _expressions = new List<Expression>();

		public ExpressionEnumeration(Expression expression)
		{
			Visit(expression);
		}

		protected override void Visit(Expression expression)
		{
			if (expression == null) return;

			_expressions.Add(expression);
			base.Visit(expression);
		}

		public IEnumerator<Expression> GetEnumerator()
		{
			return _expressions.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
