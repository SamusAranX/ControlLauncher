using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.IPAddress;

namespace RMDPExtractor {
	internal class BetterBinaryReader : BinaryReader {
		public bool IsBigEndian { get; set; }

		public BetterBinaryReader(Stream input, bool isBigEndian = false) : base(input, Encoding.ASCII) {
			this.IsBigEndian = isBigEndian;
		}
		public BetterBinaryReader(Stream input, Encoding encoding, bool isBigEndian = false) : base(input, encoding) {
			this.IsBigEndian = isBigEndian;
		}
		public BetterBinaryReader(Stream input, Encoding encoding, bool leaveOpen, bool isBigEndian = false) : base(input, encoding, leaveOpen) {
			this.IsBigEndian = isBigEndian;
		}

		public override short ReadInt16() => this.IsBigEndian ? HostToNetworkOrder(base.ReadInt16()) : base.ReadInt16();
		public override int ReadInt32() => this.IsBigEndian ? HostToNetworkOrder(base.ReadInt32()) : base.ReadInt32();
		public override long ReadInt64() => this.IsBigEndian ? HostToNetworkOrder(base.ReadInt64()) : base.ReadInt64();

		public override ushort ReadUInt16() {
			var buffer = base.ReadBytes(2);
			if (this.IsBigEndian)
				Array.Reverse(buffer);

			return BitConverter.ToUInt16(buffer, 0);
		}

		public override uint ReadUInt32() {
			var buffer = base.ReadBytes(4);
			if (this.IsBigEndian)
				Array.Reverse(buffer);

			return BitConverter.ToUInt32(buffer, 0);
		}

		public override ulong ReadUInt64() {
			var buffer = base.ReadBytes(8);
			if (this.IsBigEndian)
				Array.Reverse(buffer);

			return BitConverter.ToUInt64(buffer, 0);
		}
	}
}
