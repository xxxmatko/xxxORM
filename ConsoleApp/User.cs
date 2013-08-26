using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using xDev.Data;

namespace ConsoleApp
{
    [Table("USERS")]
    public class User : IEntity<User>
    {
        #region [ Variables ]

        private int _id;
        private string _login;
        private string _password;
        private int _departmentId;
        private string _isLocked;
        private ICollection<Role> _roles;
        private Department _department;
        private ICollection<Department> _departments;

        #endregion


        #region [ Constructors ]

        public User()
        {
            this._id = 0;
            this._login = null;
            this._password = null;
            this._departmentId = 0;
            this._isLocked = null;
            this._roles = new List<Role>();
            this._department = null;
            this._departments = new List<Department>();
        }


        public User(int id, string login, string password, int departmentId)
            : this()
        {
            this._id = id;
            this._login = login;
            this._password = password;
            this._departmentId = departmentId;
        }

        #endregion


        #region [ Properties ]

        [Key]
        [Column("ID", Order = 0, TypeName = "int")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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


        [Column("LOGIN", Order = 0, TypeName = "nvarchar(100)")]
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Login
        {
            get
            {
                return this._login;
            }
            set
            {
                this._login = value;
            }
        }


        [Column("PASSWORD", Order = 0, TypeName = "nvarchar(100)")]
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }


        [Column("DEPARTMENT_ID", Order = 0, TypeName = "int")]
        [Range(1, int.MaxValue)]
        public int DepartmentId
        {
            get
            {
                return this._departmentId;
            }
            set
            {
                this._departmentId = value;
            }
        }


        [Column("IS_LOCKED", Order = 0, TypeName = "bit")]
        [TypeConverter(typeof(BoolToStringConverter))]
        public string IsLocked    
        {
            get
            {
                return this._isLocked;
            }
            set
            {
                this._isLocked = value;
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


        public virtual Department Department
        {
            get
            {
                return this._department;
            }
            set
            {
                this._department = value;
            }
        }


        public virtual ICollection<Department> Departments
        {
            get
            {
                return this._departments;
            }
            set
            {
                this._departments = value;
            }
        }

        #endregion
    }
}
