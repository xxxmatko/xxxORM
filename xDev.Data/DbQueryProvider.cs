﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xDev.Data
{
    /// <summary>
    /// The query provider code in this class implements the four methods that are required to implement the <see cref="T:System.Linq.IQueryProvider"/> interface. 
    /// </summary>
    public class DbQueryProvider : IQueryProvider
    {
        #region [ Public Methods ]

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
        /// Constructs an System.Linq.IQueryable<T> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the System.Linq.IQueryable<T> that is returned.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>An System.Linq.IQueryable<T> that can evaluate the query represented by the specified expression tree.</returns>
        public IQueryable<T> CreateQuery<T>(Expression expression)
        {
            //return new QueryableTerraServerData<TResult>(this, expression);
            throw new NotImplementedException();
        }
                

        /// <summary>
        /// Executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="T">The type of the value that results from executing the query.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>The value that results from executing the specified query.</returns>
        public T Execute<T>(Expression expression)
        {
            //bool IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");

            //return (TResult)TerraServerQueryContext.Execute(expression, IsEnumerable);
            throw new NotImplementedException();
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