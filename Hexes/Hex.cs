using System.Collections.Generic;
using System.Text;

namespace Hexes
{
    public class Hex : List<int>
    {
        public Hex() : base() { }

        public Hex(IEnumerable<int> collection) : base(collection) { }

        public Hex(string text)
        {
            text = text.Replace(" ", "");
            text = text.Replace("\n", "");
            for (int i = 0; i < text.Length - 2; i += 2)
            {
                string hex = text.Substring(i, 2);
                int intValue = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                Add(intValue);
            }
        }

        public bool IsEmpty { get { return Count == 0; } }

        public List<int> AllMatches(Hex hex)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == hex[0])
                {
                    int counter = 0;
                    for (int j = 0; j < hex.Count; j++)
                    {
                        if (i + j >= this.Count)
                            break;
                        else if (this[i + j] == hex[j] || hex[j] == -1)
                            counter++;
                        else
                            break;
                    }
                    if (hex.Count == counter)
                        indexes.Add(i);
                }
            }
            return indexes;
        }

        public void Replace(int startIndex, Hex hex)
        {
            if (startIndex < 0 || startIndex + hex.Count > this.Count)
                return;
            foreach (int value in hex)
            {
                if (value != -1)
                {
                    this[startIndex] = value;
                }
                startIndex++;
            }
        }

        public new string ToString()
        {
            if (IsEmpty)
            {
                return "empty";
            }
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i != 0 && i % 16 == 0)
                {
                    stringBuilder.Append("\n");
                }
                stringBuilder.Append(this[i].ToString("X2") + " ");
            }
            return stringBuilder.ToString();
        }
    }
}
