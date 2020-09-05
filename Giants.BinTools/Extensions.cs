namespace Giants.BinTools
{
    using System.IO;
    using System.Text;

    public static class Extensions
    {
        /// <summary>
        /// Reads a null-terminated C-string from the binary reader.
        /// </summary>
        public static string ReadCString(this BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();

            while (true)
            {
                char c = reader.ReadChar();
                if (c == '\0')
                {
                    break;
                }

                stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Writes a null-terminated C-string to the binary writer.
        /// </summary>
        public static void WriteCString(this BinaryWriter writer, string value)
        {
            writer.Write(Encoding.UTF8.GetBytes(value));
            writer.Write('\0');
        }
    }
}
