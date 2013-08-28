using xDev.Data;

namespace ConsoleApp
{
    public sealed class UserManagementContext : EntityContext
    {
        #region [ Fields ]

        private EntitySet<User> _users; 
        
        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        public UserManagementContext(string connectionString = "SampleDB") 
            : base(connectionString)
        {
            this._users = null;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets user entity set.
        /// </summary>
        public EntitySet<User> Users
        {
            get
            {
                return this._users ?? (this._users = new EntitySet<User>(this.QueryProvider));
            }
        }

        #endregion
    }
}
