using Hexes;

namespace Instructions
{
    public class Replace : Instruction
    {
        protected Hex _beforeHex { get; set; }
        protected Hex _insideHex { get; set; }
        protected Hex _afterHex { get; set; }
        public int FileIndex { get; set; }

        public Hex BeforeHex { get => _beforeHex; set => _beforeHex = Space(value); }
        public Hex InsideHex { get => _insideHex; set => _insideHex = Space(value); }
        public Hex AfterHex { get => _afterHex; set => _afterHex = Space(value); }

        public Replace()
        {
            _beforeHex = new Hex();
            _insideHex = new Hex();
            _afterHex = new Hex();
            FileIndex = -1;
        }

        public Replace(Hex before, Hex inside, Hex after, int fileIndex)
        {
            BeforeHex = before;
            InsideHex = inside;
            AfterHex = after;
            FileIndex = fileIndex;
        }

        public new string ToString()
        {
            return FileIndex + "\n before \n" + _beforeHex.ToString() + "\n inside \n" + _insideHex.ToString() + "\n after \n" + _afterHex.ToString();
        }

        public override bool IsSet()
        {
            return !_beforeHex.IsEmpty && !_insideHex.IsEmpty && !_afterHex.IsEmpty && FileIndex >= 0;
        }
    }
}
