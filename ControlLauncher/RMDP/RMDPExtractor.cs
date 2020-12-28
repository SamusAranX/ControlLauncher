using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Force.Crc32;

namespace ControlLauncher.RMDP {
	internal enum RMDPVersion {
		AlanWake = 2,
		AlanWakesAmericanNightmare = 7,
		QuantumBreak = 8,
		Control = 9,
		Unknown = int.MinValue,
	}

	internal class RMDPExtractor {
		public static readonly string[] FontPaths = {
			"data/uiresources/p7/fonts/AkzidGrtskProBol.otf",
			"data/uiresources/p7/fonts/AkzidGrtskProReg.otf",
		};

		private static string GetName(BinaryReader reader, ulong tableSize, ulong nameOffset) {
			// save current stream position
			var position = reader.BaseStream.Position;

			// go to name offset

			reader.BaseStream.Seek(-(long)tableSize + (long)nameOffset, SeekOrigin.End);

			// suck all chars into the name one by one
			var name = "";
			while (reader.PeekChar() != 0) {
				name += reader.ReadChar();
			}

			// reset stream position
			reader.BaseStream.Seek(position, SeekOrigin.Begin);

			return name;
		}

		public static bool ExtractGameFiles(string gameDir, string packFileName, string[] filesToExtract) {
			var binFile = Path.Combine(gameDir, "data_packfiles", $"{packFileName}.bin");
			var rmdpFile = Path.Combine(gameDir, "data_packfiles", $"{packFileName}.rmdp");

			if (!File.Exists(binFile) || !File.Exists(rmdpFile))
				return false; // no exception here because the launcher itself already checks whether it is in the game folder

			var numExtractedFiles = 0;

			using (var binStream = File.Open(binFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var binReader = new BetterBinaryReader(binStream, Encoding.ASCII))
			using (var rmdpStream = File.Open(rmdpFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var rmdpReader = new BinaryReader(rmdpStream)) {
				binReader.IsBigEndian = binReader.ReadByte() == 1;

				RMDPVersion version;
				try {
					version = (RMDPVersion)binReader.ReadInt32();
				} catch (Exception) {
					version = RMDPVersion.Unknown;
					throw new Exception("The game files are in an unexpected format.");
				}

				var numFolders = binReader.ReadUInt32();
				var numFiles = binReader.ReadUInt32();

				binReader.ReadBytes(8);

				var tableSize = binReader.ReadUInt32();

				binReader.ReadBytes(128); // magic number taken from AWTools

				var folderNames = new List<string>();
				var folderPaths = new List<string>();
				var folderIDs = new List<ulong>();

				// loop to read out folder names
				for (var i = 0; i < numFolders; i++) {
					var crc = binReader.ReadUInt32();
					var nextNeighborFolderID = binReader.ReadUInt64();
					var prevNeighborFolderID = binReader.ReadUInt64();

					binReader.ReadBytes(4);

					var nameOffset = binReader.ReadUInt64();
					var nextLowerFolderID = binReader.ReadUInt64();
					var nextFileID = binReader.ReadUInt64();

					var folderName = "";
					if (nameOffset != ulong.MaxValue)
						folderName = GetName(binReader, tableSize, nameOffset);

					if (Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(folderName.ToLowerInvariant())) != crc)
						throw new Exception("The game files are corrupted.");

					folderNames.Add(folderName);
					folderIDs.Add(prevNeighborFolderID);
				}

				// loop to construct full folder paths
				ulong prevID;
				for (var i = 0; i < numFolders; i++) {
					var name = folderNames[i];
					prevID = folderIDs[i];

					while (prevID != ulong.MaxValue) {
						var folderParent = folderNames[(int)prevID];
						if (folderParent != "")
							name = folderParent + "/" + name;
						prevID = folderIDs[(int)prevID];
					}

					folderPaths.Add(name);
				}

				// loop to extract files
				for (var i = 0; i < numFiles; i++) {
					var crc = binReader.ReadUInt32();

					var nextNeighborFileID = binReader.ReadUInt64();
					prevID = binReader.ReadUInt64();
					var fileFlags = binReader.ReadUInt32();
					var nameOffset = binReader.ReadUInt64();
					var fileOffset = binReader.ReadUInt64();
					var fileSize = binReader.ReadUInt64();
					var fileDataCRC = binReader.ReadUInt32(); // should be little-endian. make this explicit when running on something other than x86
					var fileDate = binReader.ReadUInt64();

					var fileName = GetName(binReader, tableSize, nameOffset);
					var filePath = folderPaths[(int)prevID] + "/" + fileName;
					var fileDateTime = DateTime.FromFileTimeUtc((long)fileDate);

					if (!filesToExtract.Contains(filePath))
						continue;

					if (Crc32Algorithm.Compute(Encoding.ASCII.GetBytes(fileName.ToLowerInvariant())) != crc)
						throw new Exception("The game files are corrupted.");

					rmdpReader.BaseStream.Seek((long)fileOffset, SeekOrigin.Begin);
					var fileData = rmdpReader.ReadBytes((int)fileSize);
					if (Crc32Algorithm.Compute(fileData) != fileDataCRC)
						throw new Exception("The game files are corrupted.");

					using (var fileStream = File.Open(Path.Combine(gameDir, fileName), FileMode.Create))
					using (var fileWriter = new BinaryWriter(fileStream)) {
						fileWriter.Write(fileData);
					}

					numExtractedFiles++;
					if (numExtractedFiles == filesToExtract.Length)
						return true;
				}
			}

			return false;
		}
	}
}
