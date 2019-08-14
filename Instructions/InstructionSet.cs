using System.Collections.Generic;

namespace Instructions
{
    public class InstructionSet
    {
        public Identify Identify { get; set; }
        public List<Find> FindList { get; set; }
        public List<Replace> ReplaceList { get; set; }
        public List<Fill> FillList { get; set; }

        public InstructionSet()
        {
            Identify = new Identify();
            FindList = new List<Find>();
            ReplaceList = new List<Replace>();
            FillList = new List<Fill>();
        }

        public bool IsSet()
        {
            return Identify.IsSet();
        }

        public new string ToString()
        {
            string result = "";
            result += "idenfity \n" + Identify.ToString() + "\n\n";
            result += "find\n";
            foreach (var find in FindList)
            {
                result += find.ToString() + "\n";
            }
            result += "\n\n replace\n";
            foreach (var replace in ReplaceList)
            {
                result += replace.ToString() + "\n";
            }
            result += "\n\n fill\n";
            foreach (var fill in FillList)
            {
                result += fill.ToString() + "\n";
            }
            return result;
        }
    }
}
