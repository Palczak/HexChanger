using Hexes;
using Instructions;
using System.Collections.Generic;

namespace Managers
{
    public class FixManager
    {
        public InstructionSet InstructionSet { get; set; }
        public Hex CorruptedHex { get; set; }
        public Hex FixedHex { get; set; }

        public FixManager()
        {
            InstructionSet = new InstructionSet();
            CorruptedHex = new Hex();
            FixedHex = new Hex();
        }

        public bool IsSet()
        {
            return InstructionSet.IsSet() && !CorruptedHex.IsEmpty;
        }

        public bool IsResultSet() { return !FixedHex.IsEmpty; }

        public bool Identify()
        {
            Hex identifyRaw = InstructionSet.Identify.RawIdenfityHex;
            Hex identify = InstructionSet.Identify.IdentifyHex;

            if (identifyRaw.Count != CorruptedHex.Count)
            {
                return false;
            }

            List<int> match = CorruptedHex.AllMatches(identify);
            if (match.Count == 0)
            {
                return false;
            }

            return true;
        }

        //First int is number of instruction, second ints are indexes where to put instruction.
        public Dictionary<int, List<int>> Find()
        {
            var result = new Dictionary<int, List<int>>();
            foreach (Find find in InstructionSet.FindList)
            {
                List<int> positionList = CorruptedHex.AllMatches(find.FindHex);
                if (positionList.Count > 0)
                {
                    result.Add(find.FileIndex, new List<int>(positionList));
                }
            }
            return result;
        }

        public void Fix(Dictionary<int, List<int>> placesFound)
        {
            FixedHex = new Hex(CorruptedHex);

            foreach (var position in placesFound)
            {
                //Loop to find matching replace instruction
                foreach (Replace replace in InstructionSet.ReplaceList)
                {
                    if (replace.FileIndex == position.Key)
                    {
                        //Using replace on all found indexes
                        foreach (int hexIndex in position.Value)
                        {
                            ReplaceAll(hexIndex, replace);
                        }
                    }
                }

                foreach (Fill fill in InstructionSet.FillList)
                {
                    if (fill.FileIndex == position.Key)
                    {
                        foreach (int hexIndex in position.Value)
                        {
                            ReplaceAll(hexIndex, fill);
                        }
                    }
                }
            }
        }

        private void ReplaceAll(int startIndex, Replace instruction)
        {
            //index  - lenght
            FixedHex.Replace(startIndex - instruction.BeforeHex.Count, instruction.BeforeHex);
            FixedHex.Replace(startIndex, instruction.InsideHex);
            FixedHex.Replace(startIndex + instruction.InsideHex.Count, instruction.AfterHex);
        }
    }
}
