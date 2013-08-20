using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using xDev.Data;

namespace ConsoleApp
{
    [Table("ROLES")]
    public class Role : IEntity<Role>
    {
        #region [ Variables ]

        private int _id;
        private string _name;
        private ICollection<User> _users;
        private ICollection<Right> _rights;

        #endregion


        #region [ Constructors ]

        public Role()
        {
            this._id = 0;
            this._name = null;
            this._users = new List<User>();
            this._rights = new List<Right>();
        }


        public Role(int id, string name)
            : this()
        {
            this._id = id;
            this._name = name;
        }

        #endregion


        #region [ Properties ]

        [Key]
        [Column("ID", Order = 0, TypeName = "int")]
        public int Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        [Column("NAME", Order = 0, TypeName = "nvarchar(100)")]
        [Required]
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        #endregion


        #region [ Navigation Properties ]

        public virtual ICollection<User> Users
        {
            get
            {
                return this._users;
            }
            set
            {
                this._users = value;
            }
        }


        public virtual ICollection<Right> Rights
        {
            get
            {
                return this._rights;
            }
            set
            {
                this._rights = value;
            }
        }

        #endregion
    }
}
