using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace xDev.Data
{
    /// <summary>
    /// The query provider code in this class implements the four methods that are required to implement the <see cref="T:System.Linq.IQueryProvider"/> interface. 
    /// </summary>
    public abstract class DbQueryProvider : IQueryProvider
    {
        #region [ Fields ]

        private EntityContext _context;
        
        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="context">Entity context used by the provider.</param>
        protected DbQueryProvider(EntityContext context)
        {
            this._context = context;
        }

        #endregion


        #region [ Abstract Methods ]

        /// <summary>
        /// Gets the parameter name in form used by the underlying database.
        /// </summary>
        /// <param name="counter">Order of the parameter in the query.</param>
        /// <returns>Returns the name of the parameter.</returns>
        public abstract string GetDbParameterName(int counter);


        //public abstract string GetQueryText(Expression expression);

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        /// <param name="counter">Order of the parameter in the query.</param>
        /// <returns>Returns the name of the parameter.</returns>
        public virtual string GetParameterName(int counter)
        {
            return string.Format(CultureInfo.InvariantCulture, "p{0}", counter);
        }


        /// <summary>
        /// Constructs an System.Linq.IQueryable object that can evaluate the query represented
        /// by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>An System.Linq.IQueryable that can evaluate the query represented by the specified expression tree.</returns>
        public IQueryable CreateQuery(Expression expression)
        {
            //Type elementType = TypeSystem.GetElementType(expression.Type);
            //try
            //{
            //    return (IQueryable)Activator.CreateInstance(typeof(EntitySet<>).MakeGenericType(elementType), new object[] { this, expression });
            //}
            //catch (TargetInvocationException tie)
            //{
            //    throw tie.InnerException;
            //}
            throw new NotImplementedException();
        }


        /// <summary>
        /// Constructs an <see cref="System.Linq.IQueryable{T}"/> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the <see cref="System.Linq.IQueryable{T}"/> that is returned.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>An <see cref="System.Linq.IQueryable{T}"/> that can evaluate the query represented by the specified expression tree.</returns>
        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return new EntitySet<T>(this, expression);
        }
                

        /// <summary>
        /// Executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="T">The type of the value that results from executing the query.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public T Execute<T>(Expression expression)
        {
            bool isEnumerable = (typeof(T).Name == "IEnumerable`1");
            bool isQueryOverDataSource = (expression is MethodCallExpression);

            // Create expression service
            var exprService = new ExpressionService(expression)
                .FindWhere()
                .EvaluateWhere();


            throw new NotImplementedException();
            //return (T)this._context.Execute(expression, isEnumerable);
            //return (TResult)TerraServerQueryContext.Execute(expression, IsEnumerable);
        }


        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public object Execute(Expression expression)
        {
            //return TerraServerQueryContext.Execute(expression, false);
            throw new NotImplementedException();
        }

        #endregion
    }
}
