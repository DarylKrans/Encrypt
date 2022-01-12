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
        string Key = " ";
        int dec = 0;
        bool DelFile;
        private byte[] XOR(byte[] DataIn, string EncKey)
        {
            byte[] DataOut = new byte[DataIn.Length];
            for (int t = 0; t < DataIn.Length; t++)
            {
                DataOut[t] = (byte)(DataIn[t] ^ EncKey[t % EncKey.Length] ^ ((DataIn.Length - t) * 124));
            }
            return DataOut;
        }
        void Clearstrings()
        {
            InFile = "";
            OutFile = "";
            Key = " ";
            textBox1.Text = "";
            label2.Text = "";
            label3.Text = "";
            dec = 0;
            checkBox1.Checked = false;
            DelFile = checkBox1.Checked;
            Truncate(Key, 1);
        }
        private string Truncate(string value, int maxChars)
        {
            byte[] trunc = Encoding.ASCII.GetBytes(value);
            byte[] dat = new byte[maxChars];
            byte[] o = Encoding.ASCII.GetBytes("...");
            string newvalue;
            if (value.Length > maxChars)
            {
                for (int i = 0; i < 3; i++) dat[i] = o[i];
                int t = 3;
                {
                    for (int i = value.Length - (maxChars) + 3; t < (maxChars); i++)
                    {
                        dat[t] = trunc[i];
                        t++;
                    }
                }
                newvalue = Encoding.ASCII.GetString(dat);
            }
            else newvalue = value;
            return newvalue;
        }
        private string GetExtension(long size)
        {
            byte[] buff = new byte[108];
            FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
            Source.Seek(size - 108, SeekOrigin.Begin);
            Source.Read(buff, 0, 108);
            Source.Close();
            string test = Encoding.Default.GetString(XOR(buff, Key));
            if (test != "This is a test to see if the password entered is correct. If it is correct, this will be readable in english") test = "denied!";
            return test;
        }
        void Checkstatus()
        {
            try
            {
                if ((Key == " ") || (InFile.Length == 0) || (OutFile.Length == 0)) button3.Enabled = false;
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
            openFileDialog1.Title = "Select input file";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
            InFile = openFileDialog1.FileName;
            string ext = Path.GetExtension(InFile);
            if (ext == ".enc")
            {
                dec = 1;
                OutFile = Path.GetDirectoryName(InFile) + @"\" + Path.GetFileNameWithoutExtension(InFile);
            }
            else OutFile = InFile + ".enc";
            Checkstatus();
            UpdateLabels();
        }
        void UpdateLabels()
        {
            label2.Text = "Input File : " + Truncate(InFile, 55);
            label3.Text = "Output File: " + Truncate(OutFile, 55);
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            // Get file Length and set variables to handle file file in 1mb chunks
            string ext = "This is a test to see if the password entered is correct. If it is correct, this will be readable in english";
            long size = new System.IO.FileInfo(InFile).Length;
            long chunk = 1024 * 1024;
            long sections = size / chunk;
            long chunksize = chunk;
            string encdec = "Encrypting File ";
            long p;
            int fail = 0;
            if (dec == 1)
            {
                ext = GetExtension(size);
                if (ext != "denied!")
                {
                    size -= 108;
                    sections = size / chunk;
                    encdec = "Decrypting File ";
                }
                else fail = 1;
            }
            if (fail == 0)
            {
                byte[] TestPhrase = XOR(Encoding.ASCII.GetBytes(ext), Key);
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
                        byte[] output = XOR(buff, Key);
                        Dest.Write(output, 0, output.Length);
                        if (sections > 0)
                        {
                            p = i * 100 / sections;
                            this.Invoke(new Action(() => this.Text = encdec + p.ToString() + "%"));
                        }
                    }
                    if (dec == 0) Dest.Write(TestPhrase, 0, TestPhrase.Length);
                    Source.Close();
                    Dest.Close();
                    if (DelFile == true) File.Delete(InFile);
                    this.Invoke(new Action(() => this.Text = "Encrypt"));
                    this.Invoke(new Action(() => Clearstrings()));
                })
                { IsBackground = true };
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
            if (textBox1.Text == "") Key = " ";
            else Key = textBox1.Text;
            Checkstatus();
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            DelFile = checkBox1.Checked;
        }
    }
}
