namespace HiveSpace.Domain.Shared;

public abstract class Entity<TKey> 
    where TKey : struct, IEquatable<TKey>
{
    int? _requestedHashCode;
    TKey _Id;
    public virtual TKey Id
    {
        get
        {
            return _Id;
        }
        protected set
        {
            _Id = value;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null || obj is not Entity<TKey>)
            return false;

        if (Object.ReferenceEquals(this, obj))
            return true;

        if (this.GetType() != obj.GetType())
            return false;

        Entity<TKey> item = (Entity<TKey>)obj;

        return item.Id.ToString() == this.Id.ToString();
    }

    public override int GetHashCode()
    {
        if (!_requestedHashCode.HasValue)
        {
            _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

            return _requestedHashCode.Value;
        }
        else
            return base.GetHashCode();

    }
    public static bool operator == (Entity<TKey> left, Entity<TKey> right)
    {
        if (Object.Equals(left, null))
            return Object.Equals(right, null);
        else
            return left.Equals(right);
    }

    public static bool operator != (Entity<TKey> left, Entity<TKey> right)
    {
        return !(left == right);
    }
}
