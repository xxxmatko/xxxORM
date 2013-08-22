using xDev.Data;

namespace ConsoleApp
{
    public sealed class UserManagementContext : EntityContext
    {
        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        public UserManagementContext(string connectionString = "SampleDB") 
            : base(connectionString)
        {
        }

        #endregion
    }
}
