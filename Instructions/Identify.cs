using Hexes;

namespace Instructions
{
    public class Identify : Instruction
    {
        private Hex _identifyHex { get; set; }
        private Hex _rawIdentifyHex { get; set; }

        public Hex IdentifyHex { get => _identifyHex; set => SetIdentify(value); }
        public Hex RawIdenfityHex { get => _rawIdentifyHex; set => SetIdentify(value); }

        public Identify()
        {
            _identifyHex = new Hex();
            _rawIdentifyHex = new Hex();
        }

        public Identify(Hex identify)
        {
            _identifyHex = new Hex();
            _rawIdentifyHex = new Hex();
            SetIdentify(identify);
        }

        private void SetIdentify(Hex identify)
        {
            _rawIdentifyHex = identify;
            _identifyHex = Trim(identify);
        }

        public override bool IsSet()
        {
            return !_identifyHex.IsEmpty && !_rawIdentifyHex.IsEmpty;
        }

        public new string ToString()
        {
            return _identifyHex.ToString();
        }
    }
}
