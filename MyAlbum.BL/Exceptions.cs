using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlbum.BL
{
    #region Albums Manager
    [Serializable]
    public class DuplicateObjectException : Exception
    {
        public DuplicateObjectException()
        { }

        public DuplicateObjectException(string message) : base(message)
        { }

        public DuplicateObjectException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException()
        { }

        public UniqueConstraintViolationException(string message)
            : base(message)
        { }

        public UniqueConstraintViolationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class RequiredConstraintViolationException : Exception
    {
        public RequiredConstraintViolationException()
        { }

        public RequiredConstraintViolationException(string message) : base(message)
        { }

        public RequiredConstraintViolationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
    #endregion // Albums Manager
}
