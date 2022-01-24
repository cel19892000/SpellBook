using System;
using System.Globalization;

namespace SpellBook
{
    public class Spell : IEquatable<Spell> , IComparable<Spell>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Movements { get; set; }

        private string _level = string.Empty;

        public string Level
        {
            get => new SpellProgress(Name, MainWindow.doc).level;
            set => _level = value;
        }

        private double _percent = double.NaN;
        public double Percent
        {
            get
            {
                SpellProgress thisSpell = new SpellProgress(Name, MainWindow.doc);
                double percentage = Convert.ToDouble(thisSpell.percentage, new CultureInfo("en-US"));
                return percentage;
            }
            set
            {
                _percent = value;
            }
        }

        public Spell() { }

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
