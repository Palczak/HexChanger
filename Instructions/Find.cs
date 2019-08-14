using Hexes;

namespace Instructions
{
    public class Find : Instruction
    {
        private Hex _findHex;
        public Hex FindHex { get => _findHex; set => _findHex = Space(value); }
        public int FileIndex { get; set; }

        public Find()
        {
            _findHex = new Hex();
            FileIndex = -1;
        }

        public Find(Hex find, int fileIndex)
        {
            _findHex = Space(find);
            FileIndex = fileIndex;
        }

        public new string ToString()
        {
            return FileIndex + "\n" + _findHex.ToString();
        }

        public override bool IsSet()
        {
            return !_findHex.IsEmpty && FileIndex >= 0;
        }
    }
}
