using System;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace xDev.Data
{
    /// <summary>
    /// Meta information for the enitity table column.
    /// </summary>
    [DebuggerDisplay("[{Name} ({DataType})]")]
    public sealed class ColumnMetaInfo
    {
        #region [ Constants ]

        /// <summary>
        /// Regular expression for parsing column data type.
        /// </summary>
        private static readonly Regex DataTypeRegex = new Regex(@"(?<type>\w+)(?:\((?<size>\w+)\))?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        #endregion


        #region [ Fields ]

        private string _name;
        private DbType _dataType;
        private int _size;
        private bool _isNullable;
        private bool _isKey;
        private int _order;
        private bool _allowEmptyStrings;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="T:xDev.Data.ColumnMetaInfo"/> class.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <param name="dataType">Name of the column data type.</param>
        /// <param name="order">Order of the column.</param>
        public ColumnMetaInfo(string name, string dataType, int order)
        {
            this._name = name;
            this._dataType = DbType.String;
            this._size = 0;
            this._isNullable = true;
            this._isKey = false;
            this._order = order;
            this._allowEmptyStrings = false;

            ParseDataType(dataType);
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            internal set
            {
                this._name = value;
            }
        }


        /// <summary>
        /// Gets the database data type of the column.
        /// </summary>
        public DbType DataType
        {
            get
            {
                return this._dataType;
            }
            internal set
            {
                this._dataType = value;
            }
        }


        /// <summary>
        /// Gets the database column size.
        /// </summary>
        public int Size
        {
            get
            {
                return this._size;
            }
            internal set
            {
                this._size = value;
            }
        }


        /// <summary>
        /// Gets value which indicates whether the column is nullable or not.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                return this._isNullable;
            }
            internal set
            {
                this._isNullable = value;
            }
        }


        /// <summary>
        /// Gets value which indicates whether the column is key column.
        /// </summary>
        public bool IsKey
        {
            internal get
            {
                return this._isKey;
            }
            set
            {
                this._isKey = value;
            }
        }


        /// <summary>
        /// Gets the order of the column.
        /// </summary>
        public int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates whether an empty string is allowed.
        /// </summary>
        public bool AllowEmptyStrings
        {
            get
            {
                return this._allowEmptyStrings;
            }
            set
            {
                this._allowEmptyStrings = value;
            }
        }

        #endregion


        #region [ Private Methods ]

        /// <summary>
        /// Parse data type string and sets the DbType and columne size according to it.
        /// </summary>
        /// <param name="dataType">String which describes the column data type.</param>
        private void ParseDataType(string dataType)
        {
            if(string.IsNullOrEmpty(dataType))
            {
                return;
            }

            var match = ColumnMetaInfo.DataTypeRegex.Match(dataType);
            if(!match.Success)
            {
                return;
            }

            // Check and get the DbType
            if(match.Groups["type"].Success)
            {
                this._dataType = GetDbTypeForString(match.Groups["type"].Value.Trim().ToLower());                
            }

            // Check and get the column size
            if (match.Groups["size"].Success)
            {
                this._size = GetDbSizeForString(match.Groups["size"].Value.Trim().ToLower());
            }
        }


        /// <summary>
        /// Gets the DbType for the input string.
        /// </summary>
        /// <param name="dataType">String which describes DbType.</param>
        /// <returns>Returns one of the <see cref="T:System.Data.DbType"/>.</returns>
        private DbType GetDbTypeForString(string dataType)
        {
            switch(dataType)
            {
                case "int":
                    return DbType.Int32;
                case "nvarchar":
                    return DbType.String;
                case "bit":
                    return DbType.Boolean;
                default:
                    throw new NotImplementedException(string.Format("Unable to get DbType. Data type '{0}' is not supported.", dataType));
                // TODO: Handler all other db data types
            }
        }


        /// <summary>
        /// Gets the size of the database columne.
        /// </summary>
        /// <param name="size">String which represents size of the database column.</param>
        /// <returns>Returns size of the database column.</returns>
        private int GetDbSizeForString(string size)
        {
            if(string.IsNullOrEmpty(size))
            {
                return 0;
            }

            int result = 0;

            // Try to parse the size
            if(!int.TryParse(size, out result))
            {
                // TODO: It could be MAX - then resolve MAX for other db types
            }

            return result;
        }

        #endregion
    }
}
