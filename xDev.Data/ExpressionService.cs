﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace xDev.Data
{
    /// <summary>
    /// Helper for the expressions.
    /// </summary>
    public sealed class ExpressionService
    {
        #region [ Fields ]

        private readonly Expression _expression;
        private Type _elementType;
        private Expression _whereExpr;
        private Expression _selectExpr;
        private IList<MemberExpression> _selectMembers;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="expression">Expression to work with.</param>
        public ExpressionService(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this._expression = expression;
            this._elementType = null;
            this._whereExpr = null;
            this._selectExpr = null;
            this._selectMembers = null;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets the element type for which the expression is defined.
        /// </summary>
        public Type ElementType
        {
            get
            {
                if (this._elementType == null)
                {
                    GetElementType();
                }
                return this._elementType;
            }
        }


        /// <summary>
        /// Gets the where expression.
        /// </summary>
        public Expression WhereExpr
        {
            get
            {
                if(this._whereExpr == null)
                {
                    FindWhere();
                }
                return this._whereExpr;
            }
        }


        /// <summary>
        /// Gets the select expression.
        /// </summary>
        public Expression SelectExpr
        {
            get
            {
                if (this._selectExpr == null)
                {
                    FindSelect();
                }
                return this._selectExpr;
            }
        }


        /// <summary>
        /// Gets the member access expressions for the select statement.
        /// </summary>
        public IList<MemberExpression> SelectMembers
        {
            get
            {
                if(this._selectMembers == null)
                {
                    FindSelectMembers();
                }
                return this._selectMembers;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Gets element type for which the expression is defined.
        /// </summary>
        /// <returns>Returns instance of an <see cref="T:xDev.Data.ExpressionService"/> object.</returns>
        public ExpressionService GetElementType()
        {
            // If the element type is already found than do not search for it again
            if(this._elementType != null)
            {
                return this;
            }

            var ienum = FindEnumerableType(this._expression.Type);
            this._elementType = (ienum == null) ? this._expression.Type : ienum.GetGenericArguments()[0];

            return this;    
        }


        /// <summary>
        /// Seeks out the expression that represents the innermost call to Where in the expression tree that represents the client query.
        /// </summary>
        /// <returns>Returns instance of an <see cref="T:xDev.Data.ExpressionService"/> object.</returns>
        public ExpressionService FindWhere()
        {
            if(this._whereExpr != null)
            {
                return this;
            }

            // Find where expression
            this._whereExpr = new WhereFinderVisitor(this._expression).FindWhere().GetOperand();
            
            return this;
        }


        /// <summary>
        /// Seeks out the expression that represents the innermost call to Select in the expression tree that represents the client query.
        /// </summary>
        /// <returns>Returns instance of an <see cref="T:xDev.Data.ExpressionService"/> object.</returns>
        public ExpressionService FindSelect()
        {
            if (this._selectExpr != null)
            {
                return this;
            }

            // Find select expression
            this._selectExpr = new SelectFinderVisitor(this._expression).FindSelect().GetOperand();

            return this;
        }


        /// <summary> 
        /// Performs evaluation & replacement of independent sub-trees.
        /// </summary> 
        /// <returns>Returns instance of an <see cref="T:xDev.Data.ExpressionService"/> object.</returns>
        public ExpressionService EvaluateWhere()
        {
            var whereExpr = this.WhereExpr;

            // If there is not any where lambda expression stop processing
            if (whereExpr == null)
            {
                return this;
            }

            // Evaluate where expression
            this._whereExpr = new SubtreeEvaluatorVisitor(whereExpr)
                .Nominate(expr => expr.NodeType != ExpressionType.Parameter)
                .Evaluate()
                .EvaluatedExpr;

            return this;
        }


        /// <summary>
        /// Perfomrs search for the MemberAccess expressions in the select statement.
        /// </summary>
        /// <returns>Returns instance of an <see cref="T:xDev.Data.ExpressionService"/> object.</returns>
        public ExpressionService FindSelectMembers()
        {
            var selectExpr = this.SelectExpr;

            // If there is not any select expression than creates the default one
            if(selectExpr == null)
            {
                // TODO : Create default array of all members for the entity type
                return this;    
            }

            // Find all member access expressions in the select expression
            this._selectMembers = new MemberAccessFinderVisitor(selectExpr)
                .Evaluate()
                .Members;

            return this;
        }

        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Finds the type of objects in the underlying IEnumerable collection/
        /// </summary>
        /// <param name="seqType">Expression type to search for.</param>
        /// <returns>Returns type of the objects in the underlying IEnumerable collection.</returns>
        private Type FindEnumerableType(Type seqType)
        {
            if ((seqType == null) || (seqType == typeof(string)))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (var arg in seqType.GetGenericArguments())
                {
                    var ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    var ienum = FindEnumerableType(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if ((seqType.BaseType != null) && (seqType.BaseType != typeof(object)))
            {
                return FindEnumerableType(seqType.BaseType);
            }

            return null;
        }

        #endregion
    }
}
