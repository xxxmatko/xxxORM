using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace xDev.Data
{
    /// <summary>
    /// Represents a typed entity set that is used to perform create, read, update, and delete operations.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public class EntitySet<T> : IOrderedQueryable<T>
        where T : class, new()
    {
        #region [ Fields ]

        private IQueryProvider _provider;
        private Expression _expression;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Base constructor that is called by the client to create the data source. 
        /// </summary>
        public EntitySet()
        {
            throw new NotImplementedException();
            // TODO : What to do with the default provider implementation ?
            //this._provider = new TerraServerQueryProvider();
            //this._expression = Expression.Constant(this);
        }


        /// <summary> 
        /// This constructor is called by Provider.CreateQuery(). 
        /// </summary> 
        /// <param name="provider">Object which implements <see cref="T:System.Linq.IQueryProvider"/> interface.</param>
        /// <param name="expression">Query expression.</param>
        public EntitySet(IQueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            this._provider = provider;
            this._expression = expression;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets the query provider.
        /// </summary>
        public IQueryProvider Provider
        {
            get
            {
                return this._provider;
            }
            private set
            {
                this._provider = value;
            }
        }


        /// <summary>
        /// Gets the query expression.
        /// </summary>
        public Expression Expression 
        { 
            get
            {
                return this._expression;
            }
            private set
            {
                this._expression = value;
            }
        }


        /// <summary>
        /// Gets the type of the entities in the set.
        /// </summary>
        public Type ElementType
        {
            get 
            {
                return typeof(T);
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator<T> that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (this.Provider.Execute<IEnumerable<T>>(Expression)).GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator<T> that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this.Provider.Execute<System.Collections.IEnumerable>(Expression)).GetEnumerator();
        }

        #endregion
    }
}
