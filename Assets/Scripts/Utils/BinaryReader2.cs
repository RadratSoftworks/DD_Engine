using System;
using System.IO;
using System.Text;

public class BinaryReader2 : BinaryReader
{
    public BinaryReader2(Stream input) : base(input)
    {
    }

    public BinaryReader2(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public BinaryReader2(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
    }

    public string ReadWordLengthString()
    {
        ushort len = ReadUInt16BE();
        byte[] data = new byte[len];

        if (base.BaseStream.Read(data) != len)
        {
            return null;
        }

        return Encoding.UTF8.GetString(data);
    }

    public String ReadZeroTerminatedString()
    {
        string result = "";
        do
        {
            char c = (char)base.PeekChar();
            if (c == 0)
            {
                break;
            } else
            {
                base.ReadByte();
                result += c;
            }
        } while (true);

        return result;
    }

    public short ReadInt16BE()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }
    public int ReadInt32BE()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public ushort ReadUInt16BE()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToUInt16(data, 0);
    }

    public void RewindByte()
    {
        BaseStream.Seek(-1, SeekOrigin.Current);
    }
}
