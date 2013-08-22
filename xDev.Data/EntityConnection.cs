using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace xDev.Data
{
    /// <summary>
    /// Class which represents the DbConnection.
    /// </summary>
    public sealed class EntityConnection : DbConnection
    {
        #region [ Constants ]

        /// <summary>
        /// Name of the configuration property which specifies database provider.
        /// </summary>
        private const string ProviderNameProperty = "providerName";

        /// <summary>
        /// Name of the configuration property which specifies connection string for the database provider.
        /// </summary>
        private const string ProviderConnectionStringProperty = "providerConnectionString";

        #endregion


        #region [ Fields ]

        private readonly ConnectionStringSettings _connectionCnf;
        private readonly ConnectionStringSettings _storeConnectionCnf;
        private readonly DbConnection _storeConnection;
        private readonly string _providerName;

        #endregion
        
        
        #region [ Constructors ]

        /// <summary>
        /// Basic constructor, which creates DbConnection for the input entity connection string.
        /// </summary>
        /// <param name="connectionString">Name of the entity connectionString.</param>
        public EntityConnection(string connectionString)
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
                throw new ArgumentException(string.Format(@"Unable to create connection. Connection string '{0}' is missing.", connectionString), "connectionString");
            }

            this._connectionCnf = cnf;
            this._providerName = this._connectionCnf.ProviderName;

            // Get inner conenction string settings - there is configuration for the underlying datasource
            this._storeConnectionCnf = GetDbConnectionStringSettings(this._connectionCnf);

            // Create and store connection for the database
            this._storeConnection = CreateDbConnection(this._storeConnectionCnf);
        }


        /// <summary>
        /// Initializes new <see cref="T:xDev.Data.EntityConnection"/> object with supplied <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">Instance of an <see cref="T:System.Data.Common.DbConnection"/> object, which represents connectionfor the underlying database.</param>
        public EntityConnection(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection", "Unable to create entity connection.");
            }

            throw new NotImplementedException();
        }

        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Gets the connection string configuration for the inner connection string.
        /// </summary>
        /// <param name="connectionString">Inner text of the original connection string.</param>
        /// <returns>Returns collection of key/value pairs.</returns>
        private NameValueCollection ParseConnectionString(string connectionString)
        {
            var cnf = new NameValueCollection();

            // Hash table of configuration keys
            var cnfKeys = new[]
            {
                new Tuple<string, int>(EntityConnection.ProviderNameProperty, connectionString.IndexOf(EntityConnection.ProviderNameProperty, StringComparison.InvariantCultureIgnoreCase)),
                new Tuple<string, int>(EntityConnection.ProviderConnectionStringProperty, connectionString.IndexOf(EntityConnection.ProviderConnectionStringProperty, StringComparison.InvariantCultureIgnoreCase))
            };

            // Sort by the index of the key
            Array.Sort(cnfKeys, (x, y) => x.Item2 - y.Item2);

            // Get config vlue for each key
            for (int i = 0; i < cnfKeys.Length; i++)
            {
                // Get current cnf key
                var cnfKey = cnfKeys[i];
                // Get starting index
                int start = cnfKey.Item2 + cnfKey.Item1.Length;
                // Get ending index, its index of the next key or the length of the configuration
                int end = ((i + 1) < cnfKeys.Length) ? cnfKeys[i + 1].Item2 : connectionString.Length;

                // Get the actual config value
                string value = connectionString.Substring(start, end - start);

                // Store the configuration string
                cnf.Add(cnfKey.Item1, value.Trim('=', ';', '"', ' '));
            }

            return cnf;
        }


        /// <summary>
        /// Creates <see cref="T:System.Configuration.ConnectionStringSettings"/> object which represents connection setting for the underlying database.
        /// </summary>
        /// <param name="entityConnectionStringSettings">Connection string settings for the entities.</param>
        /// <returns>Returns <see cref="T:System.Configuration.ConnectionStringSettings"/> object.</returns>
        private ConnectionStringSettings GetDbConnectionStringSettings(ConnectionStringSettings entityConnectionStringSettings)
        {
            // Get inner conenction string settings - there is configuration for the underlying datasource
            var storeCnf = ParseConnectionString(entityConnectionStringSettings.ConnectionString);

            // Create connection string settings for the underlying store
            return new ConnectionStringSettings(entityConnectionStringSettings.Name, 
                storeCnf[EntityConnection.ProviderConnectionStringProperty],
                storeCnf[EntityConnection.ProviderNameProperty]);
        }


        /// <summary>
        /// Creates new database connection for the input <paramref name="connectionStringSettings"/>.
        /// </summary>
        /// <param name="connectionStringSettings">Connection settings.</param>
        /// <returns>Return new <see cref="T:System.Data.Common.DbConnection"/> object.</returns>
        private DbConnection CreateDbConnection(ConnectionStringSettings connectionStringSettings)
        {
            // Try to create connection
            DbConnection dbConnection = null;
            try
            {
                // Get DbFactory
                var dbFactory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

                // Try to create connection
                dbConnection = dbFactory.CreateConnection();

                // Check if the connection was created.
                if (dbConnection == null)
                {
                    throw new Exception("Incompatible provider.");
                }

                // Store the connection string
                dbConnection.ConnectionString = connectionStringSettings.ConnectionString;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to create connection.", ex);
            }

            return dbConnection;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return this._providerName;
            }
        }


        /// <summary>
        /// Gets the DbConnection for the underlying database.
        /// </summary>
        public DbConnection StoreConnection
        {
            get
            {
                return this._storeConnection;
            }
        }


        /// <summary>
        /// Gets the name of the current database after a connection is opened, or the database name specified in the connection string before the connection is opened.
        /// </summary>
        public override string Database
        {
            get
            {
                return (this.StoreConnection == null) ? string.Empty : this.StoreConnection.Database;
            }
        }


        /// <summary>
        /// Gets the name of the database server to which to connect.
        /// </summary>
        public override string DataSource
        {
            get
            {
                if (this.StoreConnection == null)
                {
                    return string.Empty;
                }
                string dataSource;
                try
                {
                    dataSource = this.StoreConnection.DataSource;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unable to get DataSource for the underlying store connection.", ex);
                }
                return dataSource;
            }
        }


        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected.
        /// </summary>
        public override string ServerVersion
        {
            get
            {
                if (this.StoreConnection == null)
                {
                    throw new InvalidOperationException("Store connection is unavailable.");
                }
                if (this.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException("Store connection is closed.");
                }
                string serverVersion;
                try
                {
                    serverVersion = this.StoreConnection.ServerVersion;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unable to get ServerVersion for the underlying store connection.", ex);
                }
                return serverVersion;
            }
        }


        /// <summary>
        /// Gets a string that describes the state of the connection.
        /// </summary>
        public override ConnectionState State
        {
            get
            {
                return this.StoreConnection.State;
            }
        }


        /// <summary>
        /// Gets or sets the string used to open the connection.
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return this._connectionCnf.ConnectionString;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Changes the current database for an open connection.
        /// </summary>
        /// <param name="databaseName">Specifies the name of the database for the connection to use.</param>
        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// </summary>
        public override void Close()
        {
            if (this._storeConnection == null)
            {
                return;
            }
            // Store previous state
            var previousState = this.State;
            if (this._storeConnection.State != ConnectionState.Closed)
            {
                this._storeConnection.Close();
            }
            //this.ClearTransactions();
            if (previousState == ConnectionState.Open)
            {
                OnStateChange(new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed));
            }
        }


        /// <summary>
        /// Opens a database connection with the settings specified by the <see cref="P:System.Data.Common.DbConnection.ConnectionString"/>.
        /// </summary>
        public override void Open()
        {
            if (this._storeConnection == null)
            {
                throw new InvalidOperationException("Unable to open connection. Connection is null.");
            }
            if (this.State != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Unable to open connection. Connection is already opened.");
            }
            if(this._storeConnection.State != ConnectionState.Open)
            {
                this._storeConnection.Open();
            }
            OnStateChange(new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open));
        }


        /// <summary>
        /// Creates and returns a <see cref="T:System.Data.Common.DbCommand"/> object associated with the current connection.
        /// </summary>
        /// <returns>A <see cref="T:System.Data.Common.DbCommand"/> object.</returns>
        protected override DbCommand CreateDbCommand()
        {
            return this.StoreConnection.CreateCommand();
        }

        #endregion




        #region Overrides of DbConnection

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
