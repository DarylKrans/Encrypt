using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Encrypt
{
    public partial class Form1 : Form
    {
        string InFile = "";
        string OutFile = "";
        long Key = 0;
        bool dec;
        bool DelFile;
        bool pass = true;
        const string Phrase = "TestPhrase";
        private byte[] XOR(byte[] DataIn, long EncKey)
        {
            string tKey = textBox1.Text;
            for (int t = 0; t < DataIn.Length; t++) DataIn[t] = (byte)(DataIn[t] ^ (EncKey + (t * 125) / 3) ^ tKey[(t % tKey.Length)]);
            return DataIn;
        }
       
        void Clearstrings()
        {
            InFile = "";
            OutFile = "";
            Key = 0;
            textBox1.Text = "";
            label2.Text = "";
            label3.Text = "";
            dec = false;
            pass = true;
            checkBox1.Checked = false;
            DelFile = checkBox1.Checked;
        }
        private static string Trunc(string value, int maxChars, string fname)
        {
            return value.Length <= maxChars ? value : Path.GetPathRoot(fname) + "..." + value.Substring(value.Length - (maxChars), maxChars);
        }

        private long ConvNum(byte[] c)
        {
            long outnum = 1;
            for (int i = 0; i < c.Length; i++) outnum += Convert.ToInt64(c[i]);
            this.Text = outnum.ToString();
            return outnum;
        }
        private bool GetExtension(long size)
        {
            byte[] buff = new byte[Phrase.Length];
            FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
            Source.Seek(size - Phrase.Length, SeekOrigin.Begin);
            Source.Read(buff, 0, Phrase.Length);
            Source.Close();
            if (Encoding.Default.GetString(XOR(buff, Key)) != Phrase) return false;
            else return true;
        }
        void Checkstatus()
        {
            try
            {
                if ((Key == 1) || (Key == 0) || (InFile.Length == 0) || (OutFile.Length == 0)) button3.Enabled = false;
                else button3.Enabled = true;
                if (InFile.Length == 0) checkBox1.Enabled = false;
                else checkBox1.Enabled = true;
            }
            catch { }
        }
        public Form1()
        {
            InitializeComponent();
            Checkstatus();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            dec = false;
            pass = true;
            openFileDialog1.Title = "Select input file";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
            InFile = openFileDialog1.FileName;
            string ext = Path.GetExtension(InFile);
            if (ext == ".enc")
            {
                dec = true;
                OutFile = Path.GetDirectoryName(InFile) + @"\" + Path.GetFileNameWithoutExtension(InFile);
            }
            else OutFile = InFile + ".enc";
            Checkstatus();
            UpdateLabels();
        }
        void UpdateLabels()
        {
            label2.Text = "In File  : " + Trunc(InFile, 50, InFile);
            label3.Text = "Out File: " + Trunc(OutFile, 50, InFile);
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            // Get file Length and set variables to handle file file in 1mb chunks
            long size = new System.IO.FileInfo(InFile).Length;
            long chunk = 1024 * 1024;
            long sections = size / chunk;
            long chunksize = chunk;
            string encdec = "Encrypting File ";
            if (dec == true)
            {
                pass = GetExtension(size);
                if (pass == true)
                {
                    size -= Phrase.Length;
                    sections = size / chunk;
                    encdec = "Decrypting File ";
                }
                else pass = false;
            }
            if (pass == true)
            {
                byte[] TestPhrase = XOR(Encoding.ASCII.GetBytes(Phrase), Key);
                FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
                FileStream Dest = new(OutFile, FileMode.Create, FileAccess.Write);  // Open Destination file for append
                // Start new Thread  -------------------------------- //
                Thread crypt = new(() =>
                {
                    for (long i = 0; i <= sections; i++)
                    {
                        if (i == sections) chunksize = size - (sections * chunk);
                        byte[] buff = new byte[chunksize];
                        Source.Seek(i * chunk, SeekOrigin.Begin);           // seeks file location in source file
                        Source.Read(buff, 0, (int)chunksize);
                        Dest.Write(XOR(buff, Key), 0, buff.Length);
                        if (sections > 0) this.Invoke(new Action(() => this.Text = encdec + (i * 100 / sections) + "%"));
                    }
                    if (dec == false) Dest.Write(TestPhrase, 0, TestPhrase.Length);
                    Source.Close();
                    Dest.Close();
                    if (DelFile == true) File.Delete(InFile);
                    this.Invoke(new Action(() => this.Text = "Encrypt"));
                    this.Invoke(new Action(() => Clearstrings()));
                })
                { IsBackground = false };
                // End new Thread workload -------------------- //
                crypt.Start();
            }
            else
            {
                label2.Text = "Incorrect Password!";
                label2.Refresh();
                Thread.Sleep(1000);
                UpdateLabels();
            }

        }
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == null) Key = 0;
            else Key = ConvNum(Encoding.ASCII.GetBytes(textBox1.Text));
            Checkstatus();
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            DelFile = checkBox1.Checked;
        }
    }
}
