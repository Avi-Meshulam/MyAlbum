using System;

namespace MyAlbum.BL
{
    public abstract class Equatable<T> : IEquatable<T>
    {
        public abstract bool Equals(T other);

        // Overrides System.Object GetHashCode method
        public abstract override int GetHashCode();

        // Overrides System.Object Equals method
        public override bool Equals(object obj)
        {
            if (obj is T)
                return Equals((T)obj);
            else
                return false;
        }
    }
}