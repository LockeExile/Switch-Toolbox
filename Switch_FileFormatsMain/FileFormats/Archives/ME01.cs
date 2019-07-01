﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Switch_Toolbox;
using System.Windows.Forms;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.IO;

namespace FirstPlugin
{
    public class ME01 : IArchiveFile, IFileFormat
    {
        public FileType FileType { get; set; } = FileType.Archive;

        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "ME01" };
        public string[] Extension { get; set; } = new string[] { "*.bin" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool CanAddFiles { get; set; }
        public bool CanRenameFiles { get; set; }
        public bool CanReplaceFiles { get; set; } = true;
        public bool CanDeleteFiles { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Switch_Toolbox.Library.IO.FileReader(stream, true))
            {
                return reader.CheckSignature(4, "ME01");
            }
        }

        public Type[] Types
        {
            get
            {
                List<Type> types = new List<Type>();
                return types.ToArray();
            }
        }

        public List<FileEntry> files = new List<FileEntry>();

        public IEnumerable<ArchiveFileInfo> Files => files;

        private uint Alignment;
        public void Load(System.IO.Stream stream)
        {
            CanSave = true;

            using (var reader = new FileReader(stream))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                reader.ReadSignature(4, "ME01");
                uint FileCount = reader.ReadUInt32();
                Alignment = reader.ReadUInt32();
                uint[] DataOffsets = reader.ReadUInt32s((int)FileCount);
                uint[] Sizes = reader.ReadUInt32s((int)FileCount);

                string[] FileNames = new string[FileCount];
                for (int i = 0; i < FileCount; i++)
                {
                    FileNames[i] = reader.ReadZeroTerminatedString();
                    while (true)
                    {
                        if (reader.ReadByte() != 0x30)
                        {
                            reader.Seek(-1);
                            break;
                        }
                    }
                }

                long DataPosition = reader.Position;
                for (int i = 0; i < FileCount; i++)
                {
                   //reader.SeekBegin(DataPosition + DataOffsets[i]);
                    var file = new FileEntry();
                    file.FileName = FileNames[i];
                    file.FileData = reader.ReadBytes((int)Sizes[i]);
                    files.Add(file);

                    while (true)
                    {
                        if (reader.EndOfStream)
                            break;

                        if (reader.ReadByte() != 0x30)
                        {
                            reader.Seek(-1);
                            break;
                        }
                    }
                }
            }
        }

        public void Unload()
        {

        }

        public byte[] Save()
        {
            var mem = new System.IO.MemoryStream();
            using (var writer = new FileWriter(mem))
            {
                writer.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;

                writer.WriteSignature("ME01");
                writer.Write(files.Count);
                writer.Write(Alignment);
                writer.Write(new uint[files.Count]); //Save space for offsets
                for (int i = 0; i < files.Count; i++)
                    writer.Write(files[i].FileData.Length);

                for (int i = 0; i < files.Count; i++)
                {
                    writer.WriteString(files[i].FileName);

                    //Fixed 128 string length
                    if (i != files.Count - 1)
                    {
                        int PaddingCount = 128 - files[i].FileName.Length - 1;
                        for (int p = 0; p < PaddingCount; p++)
                            writer.Write((byte)0x30);
                    }
                    else //Else align normally
                    {
                        Align(writer, (int)Alignment);
                    }
                }

                uint[] Offsets = new uint[files.Count];

                long DataStart = writer.Position;
                for (int i = 0; i < files.Count; i++)
                {
                    Offsets[i] = (uint)(writer.Position - DataStart);

                    files[i].SaveFileFormat();
                    writer.Write(files[i].FileData);
                    Align(writer, (int)Alignment);
                }

                writer.Seek(12, System.IO.SeekOrigin.Begin);
                writer.Write(Offsets);
            }

            return mem.ToArray();
        }

        private void Align(FileWriter writer, int alignment)
        {
            var startPos = writer.Position;
            long position = writer.Seek((-writer.Position % alignment + alignment) % alignment, System.IO.SeekOrigin.Current);

            writer.Seek(startPos, System.IO.SeekOrigin.Begin);
            while (writer.Position != position)
            {
                writer.Write((byte)0x30);
            }
        }

        public bool AddFile(ArchiveFileInfo archiveFileInfo)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo archiveFileInfo)
        {
            return false;
        }

        public class FileEntry : ArchiveFileInfo
        {

        }
    }
}
