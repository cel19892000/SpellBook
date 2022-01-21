namespace SpellBook
{
    public class Spell
    {
        public string name;
        public string description;
        public string type;
        public string movements;

        public Spell() { }

        public Spell(string spellName, string spellDescription, string spellType, string spellMovements)
        {
            name = spellName;
            description = spellDescription;
            type = spellType;
            movements = spellMovements;
        }

    }
}
