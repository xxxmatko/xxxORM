using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using xDev.Data;

namespace ConsoleApp
{
    [Table("DEPARTMENTS")]
    public class Department : IEntity<Department>
    {
        #region [ Variables ]

        private int _id;
        private string _code;
        private int _leaderId;
        private User _leader;
        private ICollection<User> _employees;

        #endregion


        #region [ Constructors ]

        public Department()
        {
            this._id = 0;
            this._code = null;
            this._leaderId = 0;
            this._leader = null;
            this._employees = new List<User>();
        }


        public Department(int id, string code, int leaderId)
            : this()
        {
            this._id = id;
            this._code = code;
            this._leaderId = leaderId;
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

        [Key]
        [Column("CODE", Order = 1, TypeName = "nvarchar(50)")]
        public string Code
        {
            get
            {
                return this._code;
            }
            set
            {
                this._code = value;
            }
        }


        [Column("LEADER_ID", Order = 0, TypeName = "int")]
        public int LeaderId
        {
            get
            {
                return this._leaderId;
            }
            set
            {
                this._leaderId = value;
            }
        }

        #endregion


        #region [ Navigation Properties ]

        public virtual User Leader
        {
            get
            {
                return this._leader;
            }
            set
            {
                this._leader = value;
            }
        }


        public virtual ICollection<User> Employees
        {
            get
            {
                return this._employees;
            }
            set
            {
                this._employees = value;
            }
        }

        #endregion
    }
}
