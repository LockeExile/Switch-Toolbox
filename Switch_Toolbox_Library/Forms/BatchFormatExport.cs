﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Toolbox.Library
{
    public partial class BatchFormatExport : Form
    {
        public int Index = 0;

        public BatchFormatExport(List<string> Formats)
        {
            InitializeComponent();

            foreach (string format in Formats)
                comboBox1.Items.Add(format);

            comboBox1.SelectedIndex = 0;

            Index = 0;
        }

        public string GetSelectedExtension()
        {
            string SelectedExt = comboBox1.GetSelectedText();

            string output = GetSubstringByString("(",")", SelectedExt);
            output = output.Remove(0, 1);
            output.Replace(" ", string.Empty);

            if (output == ".")
                output = ".raw";

            return output;
        }

        public string GetSubstringByString(string a, string b, string c)
        {
            return c.Substring((c.IndexOf(a) + a.Length), (c.IndexOf(b) - c.IndexOf(a) - a.Length));
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Index = comboBox1.SelectedIndex;
        }
    }
}
