using System;
using System.IO;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Encrypt
{
    public partial class Form1 : Form
    {
        string InFile = "";
        string OutFile = "";
        byte[] Crypt = new byte[0];
        byte[] UInput = new byte[0];
        bool dec;
        bool DelFile;
        bool pass = true;
        const long chunk = 1024 * 1024 * 2;
        const string Phrase = "The test phrase needs to be longer than the potential password to be inputted";
        private byte[] XOR(byte[] DataIn, byte[] EncKey1, byte[] EncKey2)
        {
            int inc = 0;
            for (int t = 0; t < DataIn.Length; t++)
            {
                DataIn[t] = (byte)(DataIn[t] ^ (EncKey1[t % EncKey1.Length] ^ ((t * 123) / 3) + inc)
                    ^ EncKey2[t % EncKey2.Length] ^ EncKey1.Length * EncKey2.Length);
                if (t % 255 == 0) inc++;
            }
            return DataIn;
        }
        private byte[] ClrKey()
        {
            byte[] kIn = { 0x10, 0x80, 0x40, 0x30, 0x60, 0x50, 0x70, 0x20,
                           0x15, 0xFE, 0xC0, 0x12, 0x68, 0x43, 0x14, 0x21 };
            return kIn;
        }
        public static class Prompt // Prompt window configuration
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new()
                {
                    Width = 200,
                    Height = 130,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Button confirmation = new() { Text = "OK", Left = 10, Width = 60, Top = 60, DialogResult = DialogResult.OK };
                Button cancel = new() { Text = "Cancel", Left = 110, Width = 60, Top = 60, DialogResult = DialogResult.Cancel };
                SystemSounds.Beep.Play();
                prompt.ControlBox = false;
                Label textLabel = new() { Left = 60, Top = 20, Text = text };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(cancel);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;
                return prompt.ShowDialog() == DialogResult.OK ? confirmation.ToString() : "no";
            }
        }
        private byte[] MakeKey(byte[] bytes)
        {
            byte[] nKey = new byte[Crypt.Length];
            for (int i = 0; i < Crypt.Length; i++)
            {
                if (i < bytes.Length)
                {
                    if (i % 2 == 0) nKey[i] = (byte)((bytes[i] + 1) / 2);
                    else nKey[i] = (byte)((bytes[i] + 1) * 2);
                }
                else nKey[i] = Crypt[i];
            }
            return nKey;
        }
        void ClearVars()
        {
            InFile = "";
            OutFile = "";
            Crypt = ClrKey();
            UInput = new byte[0];
            textBox1.Text = "";
            label2.Text = "";
            label3.Text = "";
            dec = false;
            pass = true;
            checkBox1.Checked = false;
            DelFile = checkBox1.Checked;
        }
        private static string Trunc(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : Path.GetPathRoot(value) + "..." + value.Substring(value.Length - (maxChars), maxChars);
        }
        private bool GetExtension(long size)
        {
            byte[] buff = new byte[Phrase.Length];
            FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
            Source.Seek(size - Phrase.Length, SeekOrigin.Begin);
            Source.Read(buff, 0, Phrase.Length);
            Source.Close();
            if (Encoding.Default.GetString(XOR(buff, Crypt, UInput)) != Phrase) return false;
            else return true;
        }
        void Checkstatus()
        {
            try
            {
                if ((UInput.Length == 0) || (InFile.Length == 0) || (OutFile.Length == 0)) button3.Enabled = false;
                else button3.Enabled = true;
                if (InFile.Length == 0) checkBox1.Enabled = false;
                else checkBox1.Enabled = true;
                if (InFile.Length > 0) { button2.Visible = true; button3.Visible = true; }
                else { button2.Visible = false; button3.Visible = false; }
            }
            catch { }
        }
        public Form1()
        {
            InitializeComponent();
            ClearVars();
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
                OutFile = Path.ChangeExtension(InFile, null);
                button3.Text = "Decrypt";
            }
            else { OutFile = InFile + ".enc"; button3.Text = "Encrypt"; }
            Checkstatus();
            UpdateLabels();
        }
        void UpdateLabels()
        {
            if (InFile.Length > 0)
            {
                label2.Text = "In File  : " + Trunc(InFile, 50);
                label3.Text = "Out File: " + Trunc(OutFile, 47);
            }
            else { label2.Text = ""; label3.Text = ""; }
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            // Get file Length and set variables to handle file file in 1mb chunks
            long size = new System.IO.FileInfo(InFile).Length;
            string encdec = "Encrypting File ";
            string cont = "";
            if (dec == true)            // -------- Condition check if File is being encrypted [false] or Decrypted [true]
            {
                pass = GetExtension(size);
                if (pass == true)       // -------- Condition check if [decrypt is true] then does the password provided work to decrypt file
                {
                    size -= Phrase.Length;
                    encdec = "Decrypting File ";
                }
                else pass = false;
            }
            long sections = size / chunk;
            long chunksize = chunk;
            if (pass == true)
            {
                if (File.Exists(OutFile)) cont = Prompt.ShowDialog("Overwrite?", "Destination file already exists!");
                if (cont != "no")       // -------- Condition check if destination file exists, overwrite? 
                {
                    byte[] TestPhrase = XOR(Encoding.ASCII.GetBytes(Phrase), Crypt, UInput);
                    FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
                    try
                    {
                        FileStream Dest = new(OutFile, FileMode.Create, FileAccess.Write);  // Open Destination file for append

                        Thread crypt = new(() =>
                        {
                            for (long i = 0; i <= sections; i++)
                            {
                                if (i == sections) chunksize = size - (sections * chunk);
                                byte[] buff = new byte[chunksize];
                                Source.Read(buff, 0, (int)chunksize);
                                Dest.Write(XOR(buff, Crypt, UInput), 0, buff.Length);
                                if (sections > 0) this.Invoke(new Action(() => this.Text = encdec + (i * 100 / sections) + "%"));
                            }
                            if (dec == false) Dest.Write(TestPhrase, 0, TestPhrase.Length);
                            Source.Close();
                            Dest.Close();
                            if (DelFile == true) File.Delete(InFile);
                            this.Invoke(new Action(() => this.Text = "Encrypt"));
                            this.Invoke(new Action(() => ClearVars()));
                        })
                        { IsBackground = false };
                        // End new Thread workload -------------------- //
                        crypt.Start();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("defnied")) Prompt.ShowDialog("Access Denied", "Error");
                        else Prompt.ShowDialog("Unknown", "Error");
                    }
                }
            }
            else  // -------  Condition fails password check, displays this message
            {
                label2.Text = "Incorrect Password!";
                label2.Refresh();
                Thread.Sleep(1000);
                UpdateLabels();
            }
        }
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Crypt = ClrKey();
            if (textBox1.Text != null)
            {
                Crypt = MakeKey(Encoding.ASCII.GetBytes(textBox1.Text)); //Encoding.ASCII.GetBytes(textBox1.Text);
                UInput = Encoding.ASCII.GetBytes(textBox1.Text);
            }
            Checkstatus();
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            DelFile = checkBox1.Checked;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = "Select input file";
            if (dec == false) saveFileDialog1.FileName = Path.GetFileName(openFileDialog1.FileName) + ".enc";//OutFile;
            else saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
            saveFileDialog1.ShowDialog();
            OutFile = saveFileDialog1.FileName;
            UpdateLabels();
        }
    }
}
