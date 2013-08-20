using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using xDev.Data;

namespace ConsoleApp
{
    [Table("RIGHTS")]
    public class Right : IEntity<Right>
    {
        #region [ Variables ]

        private int _id;
        private string _name;
        private ICollection<Role> _roles;

        #endregion


        #region [ Constructors ]

        public Right()
        {
            this._id = 0;
            this._name = null;
            this._roles = new List<Role>();
        }


        public Right(int id, string name)
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

        public virtual ICollection<Role> Roles
        {
            get
            {
                return this._roles;
            }
            set
            {
                this._roles = value;
            }
        }

        #endregion
    }
}
