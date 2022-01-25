using System;

namespace SpellBook
{
    public class Filter : IEquatable<Filter>
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }

        public Filter() { }

        public bool Equals(Filter other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Secondary is null ? Primary.Equals(other.Primary) : Secondary.Equals(other.Secondary) && Primary.Equals(other.Primary);
        }

        public override int GetHashCode()
        {
            int hashProductName = Primary == null ? 0 : Primary.GetHashCode();
            int hashProductCode = Secondary == null ? 0 : Secondary.GetHashCode();
            return hashProductName ^ hashProductCode;
        }

    }
}
