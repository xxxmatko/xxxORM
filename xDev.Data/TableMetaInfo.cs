using System.Diagnostics;

namespace xDev.Data
{
    /// <summary>
    /// Meta information for the enitity table.
    /// </summary>
    [DebuggerDisplay("[{Schema}].[{Name}]")]
    public sealed class TableMetaInfo
    {
        #region [ Fields ]

        private string _schema;
        private string _name;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basci constructor.
        /// </summary>
        /// <param name="name">The name of the table the entity is mapped to.</param>
        /// <param name="schema">The schema of the table the entity is mapped to.</param>
        public TableMetaInfo(string name, string schema = null)
        {
            this._name = name;
            this._schema = schema;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets the name of the table the entity is mapped to.
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
        /// Gets the schema of the table the entity is mapped to.
        /// </summary>
        public string Schema
        {
            get
            {
                return this._schema;
            }
            internal set
            {
                this._schema = value;
            }
        }

        #endregion
    }
}
