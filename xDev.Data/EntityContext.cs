using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace xDev.Data
{
    /// <summary>
    /// Entity context class.
    /// </summary>
    public abstract class EntityContext : IDisposable
    {
        #region [ Fields ]

        // TODO : How and when initialize query provider?
        private DbQueryProvider _queryProvider;
        private DbConnection _connection;
        private readonly bool _isConnectionConstructor;
        private bool _isConnectionOpened;
        private int _requestCount;
        private int? _commandTimeout;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Creates new entity context for the supplied connection string and initiate new connection.
        /// </summary>
        /// <param name="connectionString">Name of the connection string.</param>
        protected EntityContext(string connectionString)
            : this(EntityContext.CreateConnection(connectionString), true)
        {
        }


        /// <summary>
        /// Creates new entity context using supplied connection.
        /// </summary>
        /// <param name="connection">Instance of an <see cref="T:System.Data.Common.DbConnection"/> object which represents connection.</param>
        protected EntityContext(DbConnection connection)
            : this(connection, false)
        {
        }


        /// <summary>
        /// Creates new entity context using supplied connection.
        /// </summary>
        /// <param name="connection">Instance of an <see cref="T:System.Data.Common.DbConnection"/> object which represents connection.</param>
        /// <param name="isConnectionConstructor">If set to <c>true</c> context is creator of the connection.</param>
        private EntityContext(DbConnection connection, bool isConnectionConstructor)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection", "Unable to create entity context.");
            }

            this._isConnectionOpened = false;
            this._requestCount = 0;
            this._isConnectionConstructor = isConnectionConstructor;
            this._commandTimeout = null;
            this._queryProvider = null;
            this._connection = connection;
            this._connection.StateChange += this.Connection_StateChange;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets or sets the timeout value, in seconds, for all object context operations. A null value indicates that the default value of the underlying provider will be used.
        /// </summary>
        public int? CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                if (value.HasValue && (value < 0))
                {
                    throw new ArgumentOutOfRangeException("CommandTimeout", "Unable to set command timeout. The value must be greater than zero.");
                }
                this._commandTimeout = value;
            }
        }


        /// <summary>
        /// Gets the connection used by the object context.
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                if (this._connection == null)
                {
                    throw new ObjectDisposedException("connection", "Database connection has been disposed.");
                }
                return this._connection;
            }
        }


        /// <summary>
        /// Gets the LINQ query provider associated with this object context.
        /// </summary>
        protected DbQueryProvider QueryProvider
        {
            get
            {
                if (this._queryProvider == null)
                {
                    // TODO : If it was not set use and create defautl SqlQueryProvider
                    throw new ArgumentNullException("QueryProvider", "DbQueryProvider was not specified.");
                }
                return this._queryProvider;
            }
        }

        #endregion


        #region [ Protected Methods ]

        /// <summary>
        /// Releases the resources used by the object context.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> both managed and unmanaged resources will be released; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (this._connection != null)
            {
                this._connection.StateChange -= this.Connection_StateChange;
                if (this._isConnectionConstructor)
                {
                    this._connection.Close();
                    this._connection.Dispose();
                }
            }
            this._connection = null;
        }

        #endregion


        #region [ Event Handlers ]

        /// <summary>
        /// Event handler for the connection state change event.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            // TODO : Handler connection state changes
            if (e.CurrentState == ConnectionState.Closed)
            {
                //this._connectionRequestCount = 0;
                //this._openedConnection = false;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Executes an arbitrary command directly against the data source using the existing connection.
        /// </summary>
        /// <param name="commandText">The command to execute, in the native language of the data source.</param>
        /// <param name="parameters">An array of parameters to pass to the command.</param>
        /// <returns>Returns the number of rows affected.</returns>
        public int ExecuteStoreCommand(string commandText, params object[] parameters)
        {
            EnsureConnection();
            
            int result;
            try
            {
                var dbCommand = CreateStoreCommand(commandText, parameters);
                result = dbCommand.ExecuteNonQuery();
            }
            finally
            {
                this.ReleaseConnection();
            }

            return result;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Ensures that the database connection is opened.
        /// </summary>
        private void EnsureConnection()
        {
            if (this._connection == null)
            {
                throw new ObjectDisposedException("connection", "Unable to open connection while it has been disposed.");
            }

            // Reopen connection if needed
            if (this.Connection.State == ConnectionState.Closed)
            {
                this.Connection.Open();
                this._isConnectionOpened = true;
            }

            // Remember the request count
            if (this._isConnectionOpened)
            {
                this._requestCount++;
            }

            // Check the connection state again
            if ((this._connection.State == ConnectionState.Closed) || (this._connection.State == ConnectionState.Broken))
            {
                throw new InvalidOperationException("Unable to execute command on closed connection.");
            }

            try
            {
                // TODO : What about transactions?
                //this.EnsureMetadata();
                //Transaction current = Transaction.Current;
                //bool flag = (null != current && !current.Equals(this._lastTransaction)) || (null != this._lastTransaction && !this._lastTransaction.Equals(current));
                //if (flag)
                //{
                //    if (!this._openedConnection)
                //    {
                //        if (current != null)
                //        {
                //            this._connection.EnlistTransaction(current);
                //        }
                //    }
                //    else
                //    {
                //        if (this._requestCount > 1)
                //        {
                //            if (null == this._lastTransaction)
                //            {
                //                this._connection.EnlistTransaction(current);
                //            }
                //            else
                //            {
                //                this._connection.Close();
                //                this._connection.Open();
                //                this._openedConnection = true;
                //                this._requestCount++;
                //            }
                //        }
                //    }
                //}
                //this._lastTransaction = current;
            }
            catch (Exception)
            {
                ReleaseConnection();
                throw;
            }
        }


        /// <summary>
        /// Release connection to the pool.
        /// </summary>
        private void ReleaseConnection()
        {
            if (this._connection == null)
            {
                throw new ObjectDisposedException("connection", "Unable to release connection while it has been disposed.");
            }

            // If connectione is opened try to close it
            if (this._isConnectionOpened)
            {
                // Check the current request count
                if (this._requestCount > 0)
                {
                    this._requestCount--;
                }

                // If there are not any other requests than release connection
                if (this._requestCount == 0)
                {
                    this.Connection.Close();
                    this._isConnectionOpened = false;
                }
            }
        }


        /// <summary>
        /// Creates <see cref="T:System.Data.Common.DbCommand"/> for the input <paramref name="commandText"/> and <paramref name="parameters"/>.
        /// </summary>
        /// <param name="commandText">Text of the command.</param>
        /// <param name="parameters">List of command parameters.</param>
        /// <returns>Returns instance of an <see cref="T:System.Data.Common.DbCommand"/> object.</returns>
        private DbCommand CreateStoreCommand(string commandText, params object[] parameters)
        {
            // Create command for the connection
            var dbCommand = this._connection.CreateCommand();
            dbCommand.CommandText = commandText;

            // Set command timeout if specified
            if (this.CommandTimeout.HasValue)
            {
                dbCommand.CommandTimeout = this.CommandTimeout.Value;
            }

            // TODO : Implement transaction
            //EntityTransaction currentTransaction = this._connection.CurrentTransaction;
            //if (currentTransaction != null)
            //{
            //    dbCommand.Transaction = currentTransaction.StoreTransaction;
            //}

            // If there are not any parameters than exit execution
            if ((parameters == null) || (parameters.Length <= 0))
            {
                return dbCommand;
            }

            // Process parameters
            var array = new DbParameter[parameters.Length];
            //if (parameters.All((object p) => p is DbParameter))
            //{
            //    for (int i = 0; i < parameters.Length; i++)
            //    {
            //        array[i] = (DbParameter)parameters[i];
            //    }
            //}
            //else
            //{
            //    if (parameters.Any((object p) => p is DbParameter))
            //    {
            //        throw EntityUtil.InvalidOperation(Strings.ObjectContext_ExecuteCommandWithMixOfDbParameterAndValues);
            //    }
            //    string[] array2 = new string[parameters.Length];
            //    string[] array3 = new string[parameters.Length];
            //    for (int j = 0; j < parameters.Length; j++)
            //    {
            //        array2[j] = string.Format(CultureInfo.InvariantCulture, "p{0}", new object[]
            //{
            //    j
            //});
            //        array[j] = dbCommand.CreateParameter();
            //        array[j].ParameterName = array2[j];
            //        array[j].Value = (parameters[j] ?? DBNull.Value);
            //        array3[j] = "@" + array2[j];
            //    }
            //    dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, dbCommand.CommandText, array3);
            //}
            dbCommand.Parameters.AddRange(array);

            return dbCommand;
        }

        #endregion


        #region [ Static Methods ]

        /// <summary>
        /// Creates new instance of an <see cref="T:System.Data.Common.DbConnection"/> object which represents connection.
        /// </summary>
        /// <param name="connectionString">Name of the connection string.</param>
        /// <returns>Returns instance of an <see cref="T:System.Data.Common.DbConnection"/> object.</returns>
        protected static DbConnection CreateConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", "Unable to create connection.");
            }

            // Get connection configuration
            var cnf = ConfigurationManager.ConnectionStrings[connectionString];

            // Check if it is valid
            if (cnf == null)
            {
                throw new ArgumentException("Unable to create connection. Connection string is missing.", "connectionString");
            }

            // Try to create connection
            DbConnection dbConnection = null;
            try
            {
                // Get DbFactory
                var dbFactory = DbProviderFactories.GetFactory(cnf.ProviderName);

                // Try to create connection
                dbConnection = dbFactory.CreateConnection();

                // Check if the connection was created.
                if (dbConnection == null)
	            {
                    throw new Exception("Incompatible provider.");
	            }

                // Store the connection string
                dbConnection.ConnectionString = cnf.ConnectionString;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to create connection.", ex);
            }

            return dbConnection;
        }

        #endregion
    }
}
