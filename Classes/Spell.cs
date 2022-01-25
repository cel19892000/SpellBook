using System;
using System.Globalization;
using System.Xml.Serialization;

namespace SpellBook
{
    public class DefaultSpell : Spell
    {
        public DefaultSpell() { }
        
        private string _level = string.Empty;
        public override string Level
        {
            get => "Unknown";
            set => _level = value;
        }

        private double _percent = double.NaN;
        public override double Percent 
        {
            get
            {
                return 0.0;
            }
            set
            {
                _percent = value;
            }
        }
    }

    [XmlRoot("NewSpell")]
    [XmlInclude(typeof(DefaultSpell))]
    public class Spell : IEquatable<Spell> , IComparable<Spell>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Primary { get; set; }
        public string Secondary { get; set; }
        public string Movements { get; set; }

        public Spell() { }

        private string _level = string.Empty;

        public virtual string Level
        {
            get => new SpellProgress(Name, MainWindow.data.KnockturnData).level;
            set => _level = value;
        }

        private double _percent = double.NaN;
        public virtual double Percent
        {
            get
            {
                SpellProgress thisSpell = new SpellProgress(Name, MainWindow.data.KnockturnData);
                double percentage = Convert.ToDouble(thisSpell.percentage, new CultureInfo("en-US"));
                return percentage;
            }
            set
            {
                _percent = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Spell objAsSpell = obj as Spell;
            if (objAsSpell == null) return false;
            else return Equals(objAsSpell);
        }

        public int CompareTo(Spell compareSpell)
        {
            if (compareSpell == null)
                return 1;
            else
                return this.Name.CompareTo(compareSpell.Name);
        }

        public override int GetHashCode() => Name.GetHashCode();

        public bool Equals(Spell other)
        {
            if (other == null) return false;
            return (this.Name.Equals(other.Name));
        }

    }
}
