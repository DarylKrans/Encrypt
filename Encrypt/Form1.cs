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

        void clearstrings()
        {
            InFile = "";
            OutFile = "";
            Key = " ";
            textBox1.Text = "";
            label2.Text = "";
            dec = 0;
        }
        private string GetExtension(long size)
        {
            string hi = "";
            byte[] buff = new byte[8];
            FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
            Source.Seek(size - 8, SeekOrigin.Begin);
            Source.Read(buff, 0, 8);
            Source.Close();
            string conv = Encoding.Default.GetString(buff);
            StringBuilder In = new StringBuilder(conv);
            StringBuilder Out = new StringBuilder(4); //(conv.Length);
            StringBuilder Out2 = new StringBuilder(4); //(conv.Length);
            char cha;
            for (int i = 0; i < conv.Length; i++)
            {
                cha = In[i];
                cha = (char)(cha ^ Key[i % Key.Length]);
                if (i < 4) Out.Append(cha);
                if (i >= 4) Out2.Append(cha);
            }
            if (Out2.ToString() == "true") hi = Out.ToString();
            else hi = "You FAILED miserably!!";
            return hi;
        }
        void checkstatus()
        {
            try
            {
                if ((Key.Length == 0) || (InFile.Length == 0) || (OutFile.Length == 0)) button3.Enabled = false;
                else button3.Enabled = true;
                if (InFile.Length == 0) button2.Enabled = false;
                else button2.Enabled = true;
                if (dec == 1) button2.Enabled = false;
            }
            catch { }
        }
        public Form1()
        {
            InitializeComponent();
            checkstatus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select input file";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
            InFile = openFileDialog1.FileName;
            string ext = Path.GetExtension(InFile);
            if (ext == ".enc") dec = 1;
            if (dec == 1)
            {
                long size = new System.IO.FileInfo(InFile).Length;
                ext = GetExtension(size);
                OutFile = Path.ChangeExtension(InFile, ext);
            }
            checkstatus();
            label2.Text = "Input File : " + InFile;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ext = ".enc";

            saveFileDialog1.Title = "Select output file";
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(InFile) + ext;
            saveFileDialog1.ShowDialog();
            OutFile = (saveFileDialog1.FileName);
            checkstatus();
            label2.Text = "Output File : " + OutFile;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int fail = 0;
            string ext = Path.GetExtension(InFile) + "true";
            long size = new System.IO.FileInfo(InFile).Length;
            long chunk = 1024 * 1024;
            long sections = size / chunk;
            long rest = size - (sections * chunk);
            long chunksize = chunk;
            long p;
            string encdec = "Encrypting File ";
            if (dec == 1)
            {
                ext = GetExtension(size);
                if (ext != "You FAILED miserably!!")
                {
                    OutFile = Path.ChangeExtension(InFile, ext);
                    size = size - 8;
                    sections = size / chunk;
                    rest = size - (sections * chunk);
                    encdec = "Decrypting File ";
                }
                else fail = 1;
            }
            if (fail == 0)
            {
                Thread crypt = new Thread(() =>
                {
                    StringBuilder In = new StringBuilder(ext);
                    StringBuilder Out = new StringBuilder(ext.Length);
                    char cha;
                    for (int i = 0; i < ext.Length; i++)
                    {
                        cha = In[i];
                        cha = (char)(cha ^ Key[i % Key.Length]);
                        Out.Append(cha);
                    }
                    byte[] extt = Encoding.ASCII.GetBytes(Out.ToString());
                    this.Invoke(new Action(() => label2.Text = "Creating " + OutFile));
                    FileStream Source = new(InFile, FileMode.Open, FileAccess.Read);   // Open source file for read only
                    FileStream Dest = new(OutFile, FileMode.Create, FileAccess.Write);  // Open Destination file for append
                                                                                        //Dest.Write(extt, 0, extt.Length);
                    for (long i = 0; i <= sections; i++)
                    {
                        if (i == sections) chunksize = rest;
                        byte[] buff = new byte[chunksize];
                        Source.Seek(i * chunk, SeekOrigin.Begin);           // seeks file location in source file
                        Source.Read(buff, 0, (int)chunksize);
                        byte[] output = new byte[buff.Length];
                        for (int t = 0; t < buff.Length; t++)
                        {
                            output[t] = (byte)(buff[t] ^ Key[t % Key.Length] ^ ((buff.Length - t) * 124));
                        }

                        Dest.Write(output, 0, output.Length);
                        if (sections > 0)
                        {
                            p = i * 100 / sections;
                            this.Invoke(new Action(() => this.Text = encdec + p.ToString() + "%"));
                        }
                    }
                    if (dec == 0) Dest.Write(extt, 0, extt.Length);
                    Source.Close();
                    Dest.Close();
                    this.Invoke(new Action(() => this.Text = "Encrypt"));
                    this.Invoke(new Action(() => clearstrings()));
                })
                { IsBackground = true };
                crypt.Start();
            }
            else label2.Text = "Incorrect Password!";
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Key = " " + textBox1.Text;
            checkstatus();
        }
    }
}
