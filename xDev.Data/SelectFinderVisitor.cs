using System;
using System.Linq.Expressions;

namespace xDev.Data
{
    /// <summary>
    /// Visitor that finds the innermost Select method call expression.
    /// </summary>
    internal sealed class SelectFinderVisitor : ExpressionVisitor
    {
        #region [ Fields ]

        private readonly Expression _expression;
        private MethodCallExpression _select;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="expression">Original expression which contains Select expression.</param>
        public SelectFinderVisitor(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this._expression = expression;
            this._select = null;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets select expression.
        /// </summary>
        public MethodCallExpression Select
        {
            get
            {
                return this._select;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Seeks out the expression that represents the innermost call to Select in the expression tree that represents the client query.
        /// </summary>
        /// <returns>Returns expression that represents the innermost call to Where.</returns>
        public SelectFinderVisitor FindSelect()
        {
            if (this._select != null)
            {
                return this;
            }

            Visit(this._expression);
            return this;
        }


        /// <summary>
        /// Gets the operand of Select method call.
        /// </summary>
        /// <returns>Returns LambdaExpression which represents operand of Select method call.</returns>
        public LambdaExpression GetOperand()
        {
            if (this._select == null)
            {
                return null;
            }

            return (LambdaExpression)((UnaryExpression)(this._select.Arguments[1])).Operand;
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
            if (expression.Method.Name == "Select")
            {
                this._select = expression;
            }

            Visit(expression.Arguments[0]);

            return expression;
        }

        #endregion
    }
}

