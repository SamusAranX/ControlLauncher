using System;
using System.IO;
using System.Text;
using static System.Net.IPAddress;

namespace ControlLauncher.RMDP;

internal sealed class BetterBinaryReader : BinaryReader
{
	public bool IsBigEndian { get; set; }

	public BetterBinaryReader(Stream input, bool isBigEndian = false) : base(input, Encoding.ASCII)
	{
		this.IsBigEndian = isBigEndian;
	}

	public BetterBinaryReader(Stream input, Encoding encoding, bool isBigEndian = false) : base(input, encoding)
	{
		this.IsBigEndian = isBigEndian;
	}

	public BetterBinaryReader(Stream input, Encoding encoding, bool leaveOpen, bool isBigEndian = false) : base(input, encoding, leaveOpen)
	{
		this.IsBigEndian = isBigEndian;
	}

	public override short ReadInt16()
	{
		return this.IsBigEndian ? HostToNetworkOrder(base.ReadInt16()) : base.ReadInt16();
	}

	public override int ReadInt32()
	{
		return this.IsBigEndian ? HostToNetworkOrder(base.ReadInt32()) : base.ReadInt32();
	}

	public override long ReadInt64()
	{
		return this.IsBigEndian ? HostToNetworkOrder(base.ReadInt64()) : base.ReadInt64();
	}

	public override ushort ReadUInt16()
	{
		var buffer = this.ReadBytes(2);
		if (this.IsBigEndian)
			Array.Reverse(buffer);

		return BitConverter.ToUInt16(buffer, 0);
	}

	public override uint ReadUInt32()
	{
		var buffer = this.ReadBytes(4);
		if (this.IsBigEndian)
			Array.Reverse(buffer);

		return BitConverter.ToUInt32(buffer, 0);
	}

	public override ulong ReadUInt64()
	{
		var buffer = this.ReadBytes(8);
		if (this.IsBigEndian)
			Array.Reverse(buffer);

		return BitConverter.ToUInt64(buffer, 0);
	}
}
