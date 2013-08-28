using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace xDev.Data
{
    /// <summary>
    /// Performs bottom-up analysis to determine which nodes can possibly 
    /// be part of an evaluated sub-tree. 
    /// </summary>
    public sealed class NominatorVisitor : ExpressionVisitor
    {
        #region [ Fields ]

        private readonly Expression _expression;
        private Predicate<Expression> _isExpressionEvaluable;
        private HashSet<Expression> _candidates;
        private bool _cannotBeEvaluated;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor which sets delegate which decides whether the expression can be evaluated or not.
        /// </summary>
        /// <param name="expression">Expression to proccess.</param>
        public NominatorVisitor(Expression expression)
        {
            if(expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            
            this._expression = expression;
            this._isExpressionEvaluable = null;
            this._candidates = null;
            this._cannotBeEvaluated = false;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets evaluation candidates.
        /// </summary>
        public HashSet<Expression> Candidates
        {
            get
            {
                return this._candidates;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly be part of an evaluated sub-tree. 
        /// </summary>
        /// <param name="isExpressionEvaluable">Delegate which decides whether the expression can be evaluated or not.</param>
        /// <returns>Returns hash set of candidates.</returns>
        public NominatorVisitor Nominate(Predicate<Expression> isExpressionEvaluable)
        {
            if (isExpressionEvaluable == null)
            {
                throw new ArgumentNullException("isExpressionEvaluable");
            }
            this._isExpressionEvaluable = isExpressionEvaluable;

            this._candidates = new HashSet<Expression>();
            Visit(this._expression);
            return this;
        }

        #endregion


        #region [ Ooverriden Methods ]

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = this._cannotBeEvaluated;
                this._cannotBeEvaluated = false;
                base.Visit(expression);
                if (!this._cannotBeEvaluated)
                {
                    if (this._isExpressionEvaluable(expression))
                    {
                        this._candidates.Add(expression);
                    }
                    else
                    {
                        this._cannotBeEvaluated = true;
                    }
                }
                this._cannotBeEvaluated |= saveCannotBeEvaluated;
            }
            return expression;
        }

        #endregion
    }
}
