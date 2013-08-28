using System;
using System.Linq.Expressions;


namespace xDev.Data
{
    /// <summary>
    /// Visitor that finds the innermost Where method call expression.
    /// </summary>
    internal sealed class WhereFinderVisitor : ExpressionVisitor
    {
        #region [ Fields ]

        private readonly Expression _expression;
        private MethodCallExpression _where;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="expression">Original expression which contains Where expression.</param>
        public WhereFinderVisitor(Expression expression)
        {
            if(expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this._expression = expression;
            this._where = null;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets where expression.
        /// </summary>
        public MethodCallExpression Where
        {
            get
            {
                return this._where;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Seeks out the expression that represents the innermost call to Where in the expression tree that represents the client query.
        /// </summary>
        /// <returns>Returns expression that represents the innermost call to Where.</returns>
        public WhereFinderVisitor FindWhere()
        {
            if(this._where != null)
            {
                return this;
            }

            Visit(this._expression);
            return this;
        }


        /// <summary>
        /// Gets the operand of Where method call.
        /// </summary>
        /// <returns>Returns LambdaExpression which represents operand of Where method call.</returns>
        public LambdaExpression GetWhereOperand()
        {
            if(this._where == null)
            {
                return null;
            }

            return (LambdaExpression)((UnaryExpression)(this._where.Arguments[1])).Operand;
        }

        #endregion


        #region [ Overriden Methods ]

        /// <summary>
        /// Visits the children of the System.Linq.Expressions.MethodCallExpression.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name == "Where")
            {
                this._where = expression;
            }

            Visit(expression.Arguments[0]);

            return expression;
        }

        #endregion
    }
}
