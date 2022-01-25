using System;

namespace SpellBook
{
    public class Filter : IEquatable<Filter>
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }

        public Filter() { }

        public Filter(SpellType type)
        {
            Primary = type.primary;
            Secondary = type.secondary;
        }

        public bool Equals(Filter other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Secondary is null ? Primary.Equals(other.Primary) : Secondary.Equals(other.Secondary) && Primary.Equals(other.Primary);
        }

        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            int hashProductName = Primary == null ? 0 : Primary.GetHashCode();

            //Get hash code for the Code field.
            int hashProductCode = Secondary == null ? 0 : Secondary.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName ^ hashProductCode;
        }

    }
}
