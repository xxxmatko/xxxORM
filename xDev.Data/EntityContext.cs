using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace xDev.Data
{
    /// <summary>
    /// Entity context class.
    /// </summary>
    public abstract class EntityContext : IDisposable
    {
        #region [ Fields ]

        private DbQueryProvider _queryProvider;
        private EntityConnection _connection;
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
            : this(new EntityConnection(connectionString), true)
        {
        }


        /// <summary>
        /// Creates new entity context using supplied connection.
        /// </summary>
        /// <param name="connection">Instance of an <see cref="T:System.Data.Common.DbConnection"/> object which represents connection.</param>
        protected EntityContext(DbConnection connection)
            : this(new EntityConnection(connection), false)
        {
        }


        /// <summary>
        /// Creates new entity context using supplied connection.
        /// </summary>
        /// <param name="connection">Instance of an <see cref="T:System.Data.Common.DbConnection"/> object which represents connection.</param>
        /// <param name="isConnectionConstructor">If set to <c>true</c> context is creator of the connection.</param>
        private EntityContext(EntityConnection connection, bool isConnectionConstructor)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection", "Unable to create entity context.");
            }

            this._isConnectionConstructor = isConnectionConstructor;

            this._isConnectionOpened = false;
            this._connection = connection;
            this._connection.StateChange += this.Connection_StateChange;

            this._requestCount = 0;
            this._commandTimeout = null;
            this._queryProvider = null;
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
        public EntityConnection Connection
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
                    // Try to initialize specified queyrprovider
                    if (!string.IsNullOrEmpty(this._connection.ProviderName))
                    {
                        var type = Type.GetType(this._connection.ProviderName, true, true);
                        if (!typeof(DbQueryProvider).IsAssignableFrom(type))
                        {
                            throw new ArgumentException("Query provider must implement xDev.Data.DbQueryProvider class.", "queryProviderName");
                        }

                        // Creation using Activator
                        this._queryProvider = (DbQueryProvider)Activator.CreateInstance(type, new object[] 
                        {
                            this
                        });
                    }

                    // Create and use default query provider
                    this._queryProvider = new SqlQueryProvider(this);
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
            if (e.CurrentState == ConnectionState.Closed)
            {
                this._requestCount = 0;
                this._isConnectionOpened = false;
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
                ReleaseConnection();
            }

            return result;
        }


        /// <summary>
        /// Executes a stored procedure or function that is defined in the data source
        /// </summary>
        /// <param name="functionName">The name of the stored procedure or function.</param>
        /// <param name="parameters">An array of <see cref="T:System.Data.Common.DbParameter" /> objects.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteFunction(string functionName, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Executes a query directly against the data source that returns a sequence of typed results.
        /// </summary>
        /// <typeparam name="T">Type of objects in the result.</typeparam>
        /// <param name="commandText">The command to execute, in the native language of the data source.</param>
        /// <param name="parameters">An array of parameters to pass to the command.</param>
        /// <returns>An enumeration of objects of type <paramref name="T" />.</returns>
        public IEnumerable<T> ExecuteStoreQuery<T>(string commandText, params object[] parameters)
            where T : class, IEntity<T>, new()
        {
            //this.MetadataWorkspace.ImplicitLoadAssemblyForType(typeof(TElement), Assembly.GetCallingAssembly());
            EnsureConnection();

            DbDataReader dbDataReader;
            try
            {
                var dbCommand = CreateStoreCommand(commandText, parameters);
                dbDataReader = dbCommand.ExecuteReader();
            }
            catch
            {
                ReleaseConnection();
                throw;
            }

            List<T> result = null;
            try
            {
                //result = this.InternalTranslate<TElement>(dbDataReader, entitySetName, mergeOption, true);
            }
            catch
            {
                dbDataReader.Dispose();
                ReleaseConnection();
                throw;
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
                // TODO : Implement transactions
                //this.EnsureMetadata();
                //var current = Transaction.Current;
                //if (((current == null) || current.Equals(this._transaction)) && ((this._transaction == null) || this._transaction.Equals(current)))
                //{
                //    this._transaction = current;
                //    return;
                //}

                //if (!this._isConnectionOpened)
                //{
                //    if (current != null)
                //    {
                //        this._connection.EnlistTransaction(current);
                //    }
                //}
                //else
                //{
                //    if (this._requestCount > 1)
                //    {
                //        if (this._transaction == null)
                //        {
                //            this._connection.EnlistTransaction(current);
                //        }
                //        else
                //        {
                //            this._connection.Close();
                //            this._connection.Open();
                //            this._isConnectionOpened = true;
                //            this._requestCount++;
                //        }
                //    }
                //}
                //this._transaction = current;
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

            //// TODO : Implement transaction
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

            // If all parameters are DbParameters than add them to the command
            if(parameters.All(p => p is DbParameter))
            {
                dbCommand.Parameters.AddRange(parameters.Cast<DbParameter>().ToArray());
                return dbCommand;
            }

            // There can not be any DbParameter anymore
            if (parameters.Any(p => p is DbParameter))
            {
                throw new InvalidOperationException("Unable to create store command. Command parameters are mixed with DbParameter instances.");
            }

            // List of db parameters
            var dbParameters = new DbParameter[parameters.Length];
            // List of db parameters names
            var dbParametersNames = new string[parameters.Length];

            // Create DbParameter for each input parameter
            for(int i = 0; i < parameters.Length; i++)
            {
                // Create parameter
                dbParameters[i] = dbCommand.CreateParameter();
                // Set parameter name
                dbParameters[i].ParameterName = this.QueryProvider.GetParameterName(i);
                // Set parameter value
                dbParameters[i].Value = (parameters[i] ?? DBNull.Value);
                // Store the name of the parameter for the command itself
                dbParametersNames[i] = this.QueryProvider.GetDbParameterName(i);
            }
            // Update the command text
            dbCommand.CommandText = string.Format(CultureInfo.InvariantCulture, dbCommand.CommandText, dbParametersNames);

            // Set command parameters
            dbCommand.Parameters.AddRange(dbParameters);

            return dbCommand;
        }

        #endregion


        #region [ Static Methods ]

        ///// <summary>
        ///// Creates new instance of an <see cref="T:System.Data.Common.DbConnection"/> object which represents connection.
        ///// </summary>
        ///// <param name="connectionString">Name of the connection string.</param>
        ///// <returns>Returns instance of an <see cref="T:System.Data.Common.DbConnection"/> object.</returns>
        //protected static DbConnection CreateConnection(string connectionString)
        //{
        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        throw new ArgumentNullException("connectionString", "Unable to create connection.");
        //    }

        //    // Get connection configuration
        //    var cnf = ConfigurationManager.ConnectionStrings[connectionString];

        //    // Check if it is valid
        //    if (cnf == null)
        //    {
        //        throw new ArgumentException("Unable to create connection. Connection string is missing.", "connectionString");
        //    }

        //    // Get inner conenction string settings - there is configuration for the underlying datasource
        //    var storeCnf = ParseConfig(cnf.ConnectionString);
            
        //    // Create temporary connection string for the underlying store
        //    cnf = new ConnectionStringSettings(cnf.Name, storeCnf[EntityContext.ProviderConnectionStringProperty], storeCnf[EntityContext.ProviderNameProperty]);

        //    // Try to create connection
        //    DbConnection dbConnection = null;
        //    try
        //    {
        //        // Get DbFactory
        //        var dbFactory = DbProviderFactories.GetFactory(cnf.ProviderName);

        //        // Try to create connection
        //        dbConnection = dbFactory.CreateConnection();

        //        // Check if the connection was created.
        //        if (dbConnection == null)
        //        {
        //            throw new Exception("Incompatible provider.");
        //        }

        //        // Store the connection string
        //        dbConnection.ConnectionString = cnf.ConnectionString;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidOperationException("Unable to create connection.", ex);
        //    }

        //    return dbConnection;
        //}


        ///// <summary>
        ///// Gets the inner configuration for the connection string.
        ///// </summary>
        ///// <param name="connectionString">Inner text of the original connection string.</param>
        ///// <returns>Returns collection of key/value pairs.</returns>
        //protected static NameValueCollection ParseConfig(string connectionString)
        //{
        //    var cnf = new NameValueCollection();

        //    // Hash table of configuration keys
        //    var cnfKeys = new []
        //    {
        //        new Tuple<string, int>(EntityContext.ProviderNameProperty, connectionString.IndexOf(EntityContext.ProviderNameProperty, StringComparison.InvariantCultureIgnoreCase)),
        //        new Tuple<string, int>(EntityContext.ProviderConnectionStringProperty, connectionString.IndexOf(EntityContext.ProviderConnectionStringProperty, StringComparison.InvariantCultureIgnoreCase))
        //    };

        //    // Sort by the index of the key
        //    Array.Sort(cnfKeys, (x,y) => x.Item2 - y.Item2);

        //    // Get config vlue for each key
        //    for (int i = 0; i < cnfKeys.Length; i++)
        //    {
        //        // Get current cnf key
        //        var cnfKey = cnfKeys[i];
        //        // Get starting index
        //        int start = cnfKey.Item2 + cnfKey.Item1.Length;
        //        // Get ending index, its index of the next key or the length of the configuration
        //        int end = ((i + 1) < cnfKeys.Length) ? cnfKeys[i + 1].Item2 : connectionString.Length;

        //        // Get the actual config value
        //        string value = connectionString.Substring(start, end - start);

        //        // Store the configuration string
        //        cnf.Add(cnfKey.Item1, value.Trim('=', ';', '"', ' '));
        //    }

        //    return cnf;
        //}

        #endregion
    }
}
