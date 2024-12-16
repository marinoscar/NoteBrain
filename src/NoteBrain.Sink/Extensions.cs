using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteBrain.Sink
{
    public static class Extensions
    {
        /// <summary>
        /// Converts the input string to a Base64 encoded string.
        /// </summary>
        /// <param name="input">The input string to encode.</param>
        /// <returns>A Base64 encoded string.</returns>
        public static string ToBase64(this string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Reads all bytes from the given FileStream.
        /// </summary>
        /// <param name="fileStream">The FileStream to read from.</param>
        /// <returns>An array of bytes read from the FileStream.</returns>
        public static byte[] ReadAllBytes(this FileStream fileStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
