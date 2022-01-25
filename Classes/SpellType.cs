namespace SpellBook
{
    public class SpellType
    {
        public string primary;
        public string secondary;

        public SpellType(string fullType)
        {
            if (fullType != null)
            {
                string[] types = fullType.Split('>');
                primary = types[0];
                if (types.Length > 1)
                    secondary = types[1];
                else
                    secondary = "";
            }
            
            
        }

    }
}
