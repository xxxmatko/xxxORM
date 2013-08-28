using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace xDev.Data
{
    /// <summary>
    /// Evaluates & replaces sub-trees when first candidate is reached (top-down).
    /// </summary>
    public sealed class SubtreeEvaluatorVisitor : ExpressionVisitor
    {
        #region [ Fields ]

        private readonly Expression _expression;
        private Expression _evaluatedExpr;
        private HashSet<Expression> _candidates;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expression">Expression which should be evaluated.</param>
        public SubtreeEvaluatorVisitor(Expression expression)
        {
            if(expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this._expression = expression;
            this._evaluatedExpr = null;
            this._candidates = null;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets evaluated expression.
        /// </summary>
        public Expression EvaluatedExpr
        {
            get
            {
                return this._evaluatedExpr;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly be part of an evaluated sub-tree. 
        /// </summary>
        /// <param name="isExpressionEvaluable">Delegate which decides whether the sub-expression can be evaluated or not.</param>
        public SubtreeEvaluatorVisitor Nominate(Predicate<Expression> isExpressionEvaluable)
        {
            if(isExpressionEvaluable == null)
            {
                throw new ArgumentNullException("isExpressionEvaluable");
            }

            if(this._candidates != null)
            {
                return this;
            }

            // Get candidates
            this._candidates = new NominatorVisitor(this._expression).Nominate(isExpressionEvaluable).Candidates;

            return this;
        }



        /// <summary>
        /// Evaluates the expression.
        /// </summary>
        public SubtreeEvaluatorVisitor Evaluate()
        {
            if(this._evaluatedExpr != null)
            {
                return this;
            }

            this._evaluatedExpr = Visit(this._expression);

            return this;
        }

        #endregion


        #region [ Overriden Methods ]

        /// <summary>
        /// Traverse expression tree and evaluates expression that can be evaluated.
        /// </summary>
        /// <param name="exp">Expression to visit.</param>
        /// <returns>Returns modified expression or the original one.</returns>
        public override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            if (this._candidates.Contains(exp))
            {
                return this.EvaluateInternal(exp);
            }
            return base.Visit(exp);
        }
        
        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Internaly evaluates the input <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <returns>Returns evaluated expression.</returns>
        private Expression EvaluateInternal(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return expression;
            }
            var lambda = Expression.Lambda(expression);
            var fn = lambda.Compile();
            return Expression.Constant(fn.DynamicInvoke(null), expression.Type);
        }

        #endregion
    }
}
