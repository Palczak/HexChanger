using Hexes;

namespace Instructions
{
    public abstract class Instruction
    {
        public abstract bool IsSet();

        protected Hex Trim(Hex raw)
        {
            Hex result = new Hex();

            int counter = 0;
            int pointer = 0;
            foreach (int value in raw)
            {
                if (value != 35)
                {
                    pointer = counter;
                }
                counter++;
            }
            for (int i = 0; i <= pointer; i++)
            {
                result.Add(raw[i]);
            }
            return result;
        }

        protected Hex Space(Hex raw)
        {
            Hex result = new Hex();
            foreach (int value in raw)
            {
                if (value != 35)
                {
                    result.Add(value);
                }
                else
                {
                    result.Add(-1);
                }
            }
            return result;
        }

        protected Hex SpaceInverted(Hex raw)
        {
            Hex result = new Hex();
            foreach (int value in raw)
            {
                if (value != 35)
                {
                    result.Add(-1);
                }
                else
                {
                    result.Add(value);
                }
            }
            return result;
        }
    }
}
