using System.IO;

namespace Hexes
{
    public class HexIO
    {
        public Hex ReadHex(string path)
        {
            Hex result = new Hex();
            FileStream reader = new FileStream(path, FileMode.Open);
            int value;
            while ((value = reader.ReadByte()) != -1)
            {
                result.Add(value);
            }
            reader.Close();
            return result;
        }

        public void WriteHex(string path, Hex toWrite)
        {

            if (toWrite.IsEmpty)
            {
                throw new IOException("Brak wygenerowanego wyniku.");
            }

            FileStream writer = new FileStream(path, FileMode.Create);

            foreach (int value in toWrite)
            {
                writer.WriteByte((byte)value);
            }
            writer.Close();
        }
    }
}
