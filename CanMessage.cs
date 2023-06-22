// Based on ECAN-W01-UserManual_EN_V1.0.pdf manual from https://www.cdebyte.com/pdf-down.aspx?id=1949 

using System.Text.Json;

public enum IDLengthEnum { Normal, Extended };
public enum FrameTypeEnum { Data, Remote };

public class CanMessage
{
    public IDLengthEnum IDLength {get; set;}
    public FrameTypeEnum FrameType {get; set;}
    public uint ID {get; set;}
    public byte[]? Data {get; set;}
    
    public CanMessage()
    {     
    }

    public CanMessage(byte[] bytes)
    {
        if ((bytes[0] & (1<<7)) != 0)
        {
            IDLength = IDLengthEnum.Extended;
        }
        else
        {
            IDLength = IDLengthEnum.Normal;
        }
        if ((bytes[0] & (1<<6)) != 0)
        {
            FrameType = FrameTypeEnum.Remote;
        }
        else
        {
            FrameType = FrameTypeEnum.Data;
        }

        int dataLength =  bytes[0] & 0xF;
        if (dataLength > 8)
        {
            dataLength = 8;
        }

        Data = new Byte[dataLength];
        ID = BitConverter.ToUInt32(bytes, 1);
        Array.Copy(bytes, 5, Data, 0, dataLength);
    }

    public byte[] ToBinary()
    {
        var bytes = new byte[13];
        if (IDLength == IDLengthEnum.Extended)
        {
            bytes[0] |= (1<<7);
        }
        if (FrameType == FrameTypeEnum.Remote)
        {
            bytes[0] |= (1<<6);
        }
        if (Data ==null || Data.Length >8)
        {
            throw new Exception($"Invalid can message: {ToString()}");
        }
        bytes[0] |= (byte)Data.Length;
        var idSpan = new Span<byte>(bytes, 1, 4);
        BitConverter.TryWriteBytes(idSpan, ID);
        Array.Copy(Data, 0, bytes, 5, 8);

        return bytes;
    }

    public String Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static CanMessage Deserialize(string serializedMessage)
    {
        var canMessage = JsonSerializer.Deserialize<CanMessage>(serializedMessage);
        if (canMessage != null)
        {
            return canMessage;
        }
        throw new Exception($"Could not parse: {serializedMessage}");
    }

    public override string ToString()
    {
        if (Data == null)
        {
            return "Empty CanMessage";
        }
        return $"Can message: {IDLength},{FrameType},{Data.Length},{ID}:{BitConverter.ToString(Data)}:{Serialize()}";
    }
}