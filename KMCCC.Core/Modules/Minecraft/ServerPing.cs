#pragma warning disable S2696

using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KMCCC.Modules.Minecraft.Pinging;

namespace KMCCC.Modules.Minecraft;

/// <summary>
/// Minecraft Ping based off https://gist.github.com/Fireflies/2480d14fbbb33b4bbae3
/// </summary>
public class ServerPing
{
    private static NetworkStream? _stream;
    private static List<byte>? _buffer;
    private static int _offset;

    private readonly string _server;
    private readonly int _port;

    public ServerPing(string server, int port)
    {
        _server = server;
        _port = port;
    }

    public PingPayload Ping()
    {
        using (var client = new TcpClient())
        {
            client.Connect(_server, _port);
            if (!client.Connected)
            {
                client.Client.Disconnect(false);
                throw new IOException("Failed to connect to the server.");
            }

            _buffer = new List<byte>();
            _stream = client.GetStream();

            /*
             * Send a "Handshake" packet
             * http://wiki.vg/Server_List_Ping#Ping_Process
             */
            WriteVarInt(47);
            WriteString(_server);
            WriteShort((short)_port);
            WriteVarInt(1);
            Flush(0);

            /*
             * Send a "Status Request" packet
             * http://wiki.vg/Server_List_Ping#Ping_Process
             */
            Flush(0);

            var buffer = new byte[8192];
            _stream.Read(buffer, 0, buffer.Length);

            try
            {
                var json = ReadString(buffer, buffer.Length);
                var safejson = string.Concat("{", json.AsSpan(json.IndexOf('{') + 1));
                client.Client.Disconnect(false);

                if (!safejson.EndsWith("\"}"))
                    safejson += "\"}";

                try
                {
                    return JsonMapper.ToObject<PingPayload>(safejson);
                }
                catch
                {
                    return new PingPayload { Description = new Description { text = safejson } };
                }
            }
            catch (IOException)
            {
                client.Client.Disconnect(false);
                throw;
            }
        }
    }

    public Task<PingPayload> PingAsync(CancellationToken token = default(CancellationToken))
    {
        return Task<PingPayload>.Factory.StartNew(Ping, token);
    }

    #region 读取/写入 methods
    internal static byte ReadByte(byte[] buffer)
    {
        var b = buffer[_offset];
        _offset += 1;
        return b;
    }

    internal static int ReadVarInt(byte[] buffer)
    {
        var value = 0;
        var size = 0;
        int b;
        while (((b = ReadByte(buffer)) & 0x80) == 0x80)
        {
            value |= (b & 0x7F) << (size++ * 7);
            if (size > 5)
            {
                throw new IOException("This VarInt is an imposter!");
            }
        }
        return value | ((b & 0x7F) << (size * 7));
    }

    internal static string ReadString(byte[] buffer, int length)
    {
        return Encoding.UTF8.GetString(buffer, 3, length - 3).TrimEnd('\0');
    }

    internal static void WriteVarInt(int value)
    {
        while ((value & 128) != 0)
        {
            _buffer.Add((byte)(value & 127 | 128));
            value = (int)((uint)value) >> 7;
        }
        _buffer.Add((byte)value);
    }

    internal static void WriteShort(short value)
    {
        _buffer.AddRange(BitConverter.GetBytes(value));
    }

    internal static void WriteString(string data)
    {
        var buffer = Encoding.UTF8.GetBytes(data);
        WriteVarInt(buffer.Length);
        _buffer.AddRange(buffer);
    }

    internal static void Write(byte b)
    {
        _stream.WriteByte(b);
    }

    internal static void Flush(int id = -1)
    {
        var buffer = _buffer.ToArray();
        _buffer.Clear();

        var add = 0;
        var packetData = new[] { (byte)0x00 };
        if (id >= 0)
        {
            WriteVarInt(id);
            packetData = _buffer.ToArray();
            add = packetData.Length;
            _buffer.Clear();
        }

        WriteVarInt(buffer.Length + add);
        var bufferLength = _buffer.ToArray();
        _buffer.Clear();

        _stream.Write(bufferLength, 0, bufferLength.Length);
        _stream.Write(packetData, 0, packetData.Length);
        _stream.Write(buffer, 0, buffer.Length);
    }
    #endregion
}

public class Players
{
    [JsonPropertyName("max")]
    public int Max { get; set; }

    [JsonPropertyName("online")]
    public int OnlineCount { get; set; }

    [JsonPropertyName("sample")]
    public List<object>? Sample { get; set; }
}

public class Extra
{
    public string color { get; set; }
    public string text { get; set; }
    public bool? strikethrough { get; set; }
    public bool? bold { get; set; }
}

public class Description
{
    public List<Extra> extra { get; set; }
    public string text { get; set; }
}

public class Modinfo
{
    public string type { get; set; }
    public List<object> modList { get; set; }
}
