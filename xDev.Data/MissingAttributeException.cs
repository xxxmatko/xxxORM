using System;

namespace xDev.Data
{
    /// <summary>
    /// The exception that is thrown when there is an attempt to access a class attribute that does not exist.
    /// </summary>
    public sealed class MissingAttributeException : Exception
    {
        #region [ Fields ]

        private readonly string _message;
        private readonly Type _attributeType;
        private readonly Type _targetType;

        #endregion


        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="T:xDev.Data.MissingAttributeException"/> class.
        /// </summary>
        /// <param name="attributeType">Type of the missing attribute.</param>
        /// <param name="targetType">Type of the target class.</param>
        /// <param name="message">Message that describes the current exception.</param>
        public MissingAttributeException(Type attributeType, Type targetType, string message = null)
        {
            this._attributeType = attributeType;
            this._targetType = targetType;
            this._message = message;
        }

        #endregion


        #region [ Properties ]

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                string msg = string.Format(@"Class {0} is missing {1} attribute.", this._targetType.FullName, this._attributeType.FullName);
                if(string.IsNullOrEmpty(this._message))
                {
                    return msg;
                }

                return string.Format("{0}. {1}", this._message.TrimEnd('.'), msg);
            }
        }

        #endregion
    }
}
