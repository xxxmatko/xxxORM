using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace xDev.Data
{
    /// <summary>
    /// Finds all MemberAccess epxressions.
    /// </summary>
    public sealed class MemberAccessFinderVisitor : ExpressionVisitor
    {
        #region [ Fields ]

        private readonly Expression _expression;
        private List<MemberExpression> _members;
        private ParameterExpression _parameter;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expression">Expression which should be evaluated.</param>
        public MemberAccessFinderVisitor(Expression expression)
        {
            if(expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this._expression = expression;
            this._members = null;
            this._parameter = null;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets all members.
        /// </summary>
        public IList<MemberExpression> Members
        {
            get
            {
                return this._members;
            }
        }


        /// <summary>
        /// Gets the parameter expression.
        /// </summary>
        public ParameterExpression Parameter
        {
            get
            {
                return this._parameter;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        public MemberAccessFinderVisitor Evaluate()
        {
            if (this._members != null)
            {
                return this;
            }

            Visit(this._expression);

            return this;
        }

        #endregion


        #region [ Overriden Methods ]

        /// <summary>
        /// Visits the children of the <see cref="System.Linq.Expressions.Expression{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            this._parameter = (node as LambdaExpression).Parameters[0];
            return base.VisitLambda<T>(node);
        }


        /// <summary>
        /// Visits the children of the System.Linq.Expressions.MemberExpression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if((node.Expression.NodeType != ExpressionType.Parameter) || !node.Expression.Equals(this._parameter))
            {
                return base.VisitMember(node);    
            }
            if(!this._members.Contains(node))
            {
                this._members.Add(node);
            }
            return base.VisitMember(node);
        }


        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.NewExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitNew(NewExpression node)
        {
            if (this._members == null)
            {
                this._members = new List<MemberExpression>();
            }

            return base.VisitNew(node);
        }

        #endregion
    }
}
