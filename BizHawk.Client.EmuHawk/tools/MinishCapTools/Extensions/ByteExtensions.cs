using System;
using System.Collections.Generic;

namespace MinishCapTools.Extensions
{
    public static class ByteExtensions
    {
        public static ushort BytesToUnsignedShortLE(this List<byte> bytes, int index)
        {
            if (index + 1 >= bytes.Count) throw new IndexOutOfRangeException();

            return (ushort)(bytes[index] + (bytes[index + 1] << 8));
        }
        
        public static short BytesToSignedShortLE(this List<byte> bytes, int index)
        {
            if (index + 1 >= bytes.Count) throw new IndexOutOfRangeException();

            return (short)(bytes[index] + (bytes[index + 1] << 8));
        }
        
        public static int BytesToSignedIntLE(this List<byte> bytes, int index)
        {
            if (index + 3 >= bytes.Count) throw new IndexOutOfRangeException();

            return bytes[index] + (bytes[index + 1] << 8) + (bytes[index + 2] << 16) + (bytes[index + 3] << 24);
        }
        
        public static uint BytesToUnsignedIntLE(this List<byte> bytes, int index)
        {
            if (index + 3 >= bytes.Count) throw new IndexOutOfRangeException();

            return (uint)(bytes[index] + (bytes[index + 1] << 8) + (bytes[index + 2] << 16) + ((uint)bytes[index + 3] << 24));
        }
    }
}