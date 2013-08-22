namespace xDev.Data
{
    /// <summary>
    /// Query provider for MS SQL databases.
    /// </summary>
    public sealed class SqlQueryProvider : DbQueryProvider
    {
        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="context">Entity context used by the provider.</param>
        public SqlQueryProvider(EntityContext context)
            : base(context)
        {
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Gets the parameter name in form used by the underlying database.
        /// </summary>
        /// <param name="counter">Order of the parameter in the query.</param>
        /// <returns>Returns the name of the parameter.</returns>
        public override string GetDbParameterName(int counter)
        {
            return "@" + GetParameterName(counter);
        }

        #endregion
    }
}
