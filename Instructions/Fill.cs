using Hexes;

namespace Instructions
{
    public class Fill : Replace
    {
        public new Hex BeforeHex { get => _beforeHex; set => _beforeHex = SpaceInverted(value); }
        public new Hex InsideHex { get => _insideHex; set => _insideHex = SpaceInverted(value); }
        public new Hex AfterHex { get => _afterHex; set => _afterHex = SpaceInverted(value); }

        public Fill() : base()
        {

        }

        public Fill(Hex before, Hex inside, Hex after, int index) : base(before, inside, after, index)
        {

        }
    }
}
