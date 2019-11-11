using Hexes;
using System;
using System.Collections.Generic;

namespace Managers
{
    public class GlobalManager
    {
        public FileManager FileManager { get; set; }
        public FixManager FixManager { get; set; }

        public GlobalManager()
        {
            FileManager = new FileManager();
            FixManager = new FixManager();
        }

        public void LoadInstruction(string patch)
        {
            FileManager.InstructionDir = patch;
            FixManager.InstructionSet = FileManager.ReadInstructions();
        }

        public void LoadCorruptedHex(string path)
        {
            FixManager.CorruptedHex = FileManager.HexIO.ReadHex(path);
        }

        public Hex Fix(Dictionary<int, List<int>> positionsFound)
        {
            FixManager.Fix(positionsFound);
            return FixManager.FixedHex;
        }

        public bool Identify()
        {
            if(FixManager.IsSet())
            {
                return FixManager.Identify();
            }
            return false;
        } 

        public Dictionary<int, List<int>> Find()
        {
            if (FixManager.IsSet())
            {
                var positionsFound = FixManager.Find();
                if (positionsFound.Count == 0)
                {
                    return null;
                }
                else
                {
                    return positionsFound;
                }
            }
            return null;
        }
    }
}
