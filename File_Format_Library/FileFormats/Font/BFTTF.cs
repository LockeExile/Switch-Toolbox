﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox;
using System.Windows.Forms;
using Toolbox.Library;
using Toolbox.Library.IO;
using Toolbox.Library.Forms;

namespace FirstPlugin
{
    public class BFTTF : TreeNodeFile, IFileFormat, IContextMenuNode
    {
        public FileType FileType { get; set; } = FileType.Font;

        public bool CanSave { get; set; }
        public string[] Description { get; set; } = new string[] { "Binary Cafe True Type Font" };
        public string[] Extension { get; set; } = new string[] { "*.bfttf" };
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public bool Identify(System.IO.Stream stream)
        {
            using (var reader = new Toolbox.Library.IO.FileReader(stream, true))
            {
                uint magic = reader.ReadUInt32();
                reader.Position = 0;
                return magic == 0x1A879BD9 || magic == 0x1E1AF836 || magic == 0xC1DE68F3;
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

        public byte[] DecryptedFont { get; set; }

        //Decryption process from https://github.com/TheFearsomeDzeraora/BFTTFutil/blob/master/Program.cs
        public void Load(System.IO.Stream stream)
        {
            Text = FileName;

            using (var reader = new FileReader(stream))
            {
                uint decryptionKey = 0;

                uint magic = reader.ReadUInt32();
                switch (magic)
                {
                    case 0x1A879BD9: decryptionKey = 2785117442U; break;
                    case 0x1E1AF836: decryptionKey = 1231165446U; break;
                    case 0xC1DE68F3: decryptionKey = 2364726489U; break;
                    default:
                        Console.WriteLine("Err 0x2: Input file isn't a BFTTF\\BFOTF");
                        break;
                }

                byte[] inFile = reader.getSection(0, (int)reader.BaseStream.Length);
                if (inFile.Length <= 8) return;
                uint value = GetUInt32(inFile, 4) ^ decryptionKey;
                if (inFile.Length < value) return;
                byte[] outFile = new byte[inFile.Length - 8];
                int pos = 8;
                while (pos < inFile.Length)
                {
                    SetToUInt32(GetUInt32(inFile, pos) ^ decryptionKey, outFile, pos - 8);
                    pos += 4;
                }

                DecryptedFont = outFile;
            }
        }

        public ToolStripItem[] GetContextMenuItems()
        {
            List<ToolStripItem> Items = new List<ToolStripItem>();
            Items.Add(new STToolStipMenuItem("Export", null, ExportAction, Keys.Control | Keys.E));
            return Items.ToArray();
        }

        private void ExportAction(object sender, EventArgs args)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Text;
            sfd.DefaultExt = System.IO.Path.GetExtension(Text);
            sfd.Filter = "Open Type Font |*.otf;";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllBytes(sfd.FileName, DecryptedFont);
            }
        }

        public override void OnClick(TreeView treeview)
        {
            HexEditor editor = (HexEditor)LibraryGUI.GetActiveContent(typeof(HexEditor));
            if (editor == null)
            {
                editor = new HexEditor();
                LibraryGUI.LoadEditor(editor);
            }
            editor.Text = Text;
            editor.Dock = DockStyle.Fill;
            editor.LoadData(DecryptedFont);
        }

        private static UInt32 GetUInt32(byte[] data, int pos)
        {
            return (UInt32)(data[pos + 3] | data[pos + 2] << 8 | data[pos + 1] << 16 | data[pos] << 24);
        }

        private static void SetToUInt32(uint val, byte[] data, int pos)
        {
            data[pos + 3] = (byte)(val & (uint)byte.MaxValue);
            data[pos + 2] = (byte)(val >> 8 & (uint)byte.MaxValue);
            data[pos + 1] = (byte)(val >> 16 & (uint)byte.MaxValue);
            data[pos] = (byte)(val >> 24 & (uint)byte.MaxValue);
        }

        public void Unload()
        {

        }

        public void Save(System.IO.Stream stream)
        {
        }
    }
}
