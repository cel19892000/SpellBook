﻿namespace SpellBook
{
    class SpellType
    {
        public string primary;
        public string secondary;

        public SpellType(string fullType)
        {
            string[] types = fullType.Split('>');
            primary = types[0];
            if (types.Length > 1)
                secondary = types[1];
        }

    }
}