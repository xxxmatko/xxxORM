using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace xDev.Data
{
    /// <summary>
    /// Service class for objects of type <see cref="T:System.Data.Common.DbDataReader"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DataReaderService<T>
        where T : class, IEntity<T>, new()
    {
        #region [ Fields ]

        private DbDataReader _reader;
        private readonly bool _hasRows;
        private readonly int _fieldCount;
        private bool _isClosed;
        private List<T> _result;
        private List<string> _columns;
        private MetaInfo<T> _metaInfo; 

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="reader">Instance of an <see cref="T:System.Data.Common.DbDataReader"/> object.</param>
        public DataReaderService(DbDataReader reader)
        {
            if(reader == null)
            {
                throw new ArgumentNullException("reader", "Unable to create DataReaderService for an empty reader.");
            }
            this._reader = reader;
            this._hasRows = this._reader.HasRows;
            this._fieldCount = this._reader.FieldCount;
            this._isClosed = false;
            this._columns = null;
            this._metaInfo = null;
            this._result = new List<T>();
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets a value that indicates whether the undelrying data reader contains one or more rows.
        /// </summary>
        public bool HasRows
        {
            get
            {
                return this._hasRows;
            }
        }


        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int FieldCount
        {
            get
            {
                return this._fieldCount;
            }
        }


        /// <summary>
        /// Gets column names for the underlying data reader.
        /// </summary>
        public IList<string> Columns
        {
            get
            {
                if(this._columns == null)
                {
                    GetColumns();
                }
                return this._columns;
            }
        }


        /// <summary>
        /// Gets meta info for the entity.
        /// </summary>
        public MetaInfo<T> MetaInfo
        {
            get
            {
                if (this._metaInfo == null)
                {
                    GetMetaInfo();
                }
                return this._metaInfo;
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Gets all columns for the underlying <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </summary>
        /// <returns>Returns instance of an <see cref="xDev.Data.DataReaderService{T}"/>.</returns>
        public DataReaderService<T> GetColumns()
        {
            // TODO : Check if the _columns is initialized than do not it again
            CheckIsClosed();
            
            // Initialize the column list
            this._columns = new List<string>(this.FieldCount);

            // Read all columns from the datareader
            for(int i = 0; i < this.FieldCount; i++)
            {
                this._columns.Add(this._reader.GetName(i));
            }

            return this;    
        }


        /// <summary>
        /// Gets meta info for the entity.
        /// </summary>
        /// <returns>Returns instance of an <see cref="xDev.Data.DataReaderService{T}"/>.</returns>
        public DataReaderService<T> GetMetaInfo()
        {
            // TODO : Check if the _metainfo is initialized than do not it again
            CheckIsClosed();

            // Get meta info for the entity type
            this._metaInfo = MetaInfo<T>.GetMetaInfo();

            return this;
        }


        /// <summary>
        /// Gets value indicating whether the underlying reader contains specified <paramref name="columnName"/>.
        /// </summary>
        /// <param name="columnName">Name of the column to check.</param>
        /// <returns>Returns <c>true</c> if the reader contains column, <c>false</c> otherwise.</returns>
        public bool HasColumn(string columnName)
        {
            return this.Columns.Contains(columnName);
        }


        /// <summary>
        /// Gets the resulting entity collection.
        /// </summary>
        /// <returns>Returns an entity collection.</returns>
        public List<T> ToList()
        {
            if(this._isClosed)
            {
                return this._result;
            }

            try
            {
                ReadData();
            }
            catch(Exception ex)
            {
                throw new Exception("Error occured while reading data from the data reader.", ex);
            }
            finally
            {
                ReleaseReader();    
            }
            
            return this._result;
        }

        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Checks if the udneryling data reader is closed. If it is closed than an exception occures.
        /// </summary>
        private void CheckIsClosed()
        {
            if (this._isClosed)
            {
                throw new InvalidOperationException("Unable to continue. Underlying reader is already closed.");
            }
        }


        /// <summary>
        /// Releases underlying reader.
        /// </summary>
        private void ReleaseReader()
        {
            if((this._reader == null) || (this._reader.IsClosed))
            {
                return;
            }
            this._isClosed = true;

            this._reader.Close();
            this._reader.Dispose();
            this._reader = null;
        }


        /// <summary>
        /// Reads data from the undelrying reader.
        /// </summary>
        private void ReadData()
        {
            if (!this.HasRows)
            {
                return;
            }

            // Get entity meta info
            var mi = this.MetaInfo;

            // Get list of entity properties
            var properties = mi.Properties;

            // Get column names and types for properties
            var columnNames = mi.ColumnInfos.ToDictionary(ci => ci.Key, ci => ci.Value.Name);
            var columnTypes = mi.ColumnInfos.ToDictionary(ci => ci.Key, ci => ci.Value.DataType);

            // Get property converters
            var propertyConverters = mi.PropertyConverters;

            // Read each record
            while (this._reader.Read())
            {
                T entity = ReadEntity(this._reader, properties, columnNames, columnTypes, propertyConverters);
                if(entity == null)
                {
                    throw new InvalidOperationException("Unable to read data for the entity.");
                }
                this._result.Add(entity);
            }
        }


        /// <summary>
        /// Reads data from the supplied <see cref="T:System.Data.IDataReader"/> object.
        /// </summary>
        /// <param name="data">Instance of an <see cref="T:System.Data.IDataReader"/> object.</param>
        /// <param name="properties">List of entity properties.</param>
        /// <param name="columnNames">List of entity column names.</param>
        /// <param name="columnTypes">List of entity column types.</param>
        /// <param name="propertyConverters">List of property converters.</param>
        /// <returns>Returns new instance of an <typeparamref name="T"/> entity object filled with supplied data.</returns>
        private T ReadEntity(IDataReader data, IList<string> properties, IDictionary<string, string> columnNames, IDictionary<string, DbType> columnTypes, IDictionary<string, TypeConverter> propertyConverters)
        {
            // Arguments for the entity constructor
            var @params = new object[properties.Count];

            // Read value for each property
            for (int i = 0; i < properties.Count; i++)
            {
                string property = properties[i];
                string column = columnNames[property];
                DbType columnType = columnTypes[property];

                // Check if reader contains property column
                if (!HasColumn(column))
                {
                    @params[i] = null;
                    continue;
                }

                // Get column index
                int columnIdx = data.GetOrdinal(column);

                // If the valu is null than stop processing for the column
                if (data.IsDBNull(columnIdx))
                {
                    @params[i] = null;
                    continue;
                }

                // Store the column value for the constructor
                switch (columnType)
                {
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.String:
                    case DbType.StringFixedLength:
                        @params[i] = data.GetString(columnIdx);
                        break;
                    case DbType.Binary:
                        throw new NotImplementedException();
                        break;
                    case DbType.Byte:
                    case DbType.SByte:
                        @params[i] = data.GetByte(columnIdx);
                        break;
                    case DbType.Boolean:
                        @params[i] = data.GetBoolean(columnIdx);
                        break;
                    case DbType.Currency:
                    case DbType.Decimal:
                        @params[i] = data.GetDecimal(columnIdx);
                        break;
                    case DbType.Double:
                        @params[i] = data.GetDouble(columnIdx);
                        break;
                    case DbType.Guid:
                        @params[i] = data.GetGuid(columnIdx);
                        break;
                    case DbType.Int16:
                    case DbType.UInt16:
                        @params[i] = data.GetInt16(columnIdx);
                        break;
                    case DbType.Int32:
                    case DbType.UInt32:
                        @params[i] = data.GetInt32(columnIdx);
                        break;
                    case DbType.Int64:
                    case DbType.UInt64:
                        @params[i] = data.GetInt64(columnIdx);
                        break;
                    case DbType.Single:
                        @params[i] = data.GetFloat(columnIdx);
                        break;
                    case DbType.VarNumeric:
                        throw new NotImplementedException();
                        break;
                    case DbType.Xml:
                        throw new NotImplementedException();
                        break;
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.DateTimeOffset:
                    case DbType.Time:
                        @params[i] = data.GetDateTime(columnIdx);
                        break;
                    case DbType.Object:
                    default:
                        @params[i] = data.GetValue(columnIdx);
                        break;
                }

                // Apply converters if needed
                if(propertyConverters.ContainsKey(property))
                {
                    @params[i] = propertyConverters[property].ConvertFrom(@params[i]);
                }
            }

            // Create entity
            return this.MetaInfo.Create(@params);
        }

        #endregion
    }
}
