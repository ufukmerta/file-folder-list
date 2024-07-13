using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileFolderList
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }
        string newPath(string fPath)
        {
            return Path.Combine(currentDirectory,
                Path.GetFileNameWithoutExtension(fPath) + String.Format("{0:_MM_dd_yyyy__h_mm_ss}",
                File.GetLastWriteTime(fPath)) + ".txt");
        }
        string currentDirectory = "";
        Thread th;

        void list()
        {
            try
            {
                currentDirectory = Directory.GetCurrentDirectory();
                string[] files = Directory.GetFiles(currentDirectory, ".", SearchOption.AllDirectories);
                string[] directories = Directory.GetDirectories(currentDirectory,"*", SearchOption.AllDirectories);
                string fPath = Path.Combine(currentDirectory, "filepaths.txt");
                if (File.Exists(fPath))
                {
                    File.Move(fPath, newPath(fPath));
                }
                fPath = Path.Combine(currentDirectory, "documentsname.txt");
                if (File.Exists(fPath))
                {
                    File.Move(fPath, newPath(fPath));
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(currentDirectory, "filepaths.txt"), FileMode.CreateNew), Encoding.UTF8))
                {
                    foreach (string value in files)
                    {
                        sw.WriteLine(value);
                    }
                }
                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(currentDirectory, "documentsname.txt"), FileMode.CreateNew), Encoding.UTF8))
                {
                    sw.WriteLine("ROOT:" + currentDirectory + "\nSubfolders:");
                    foreach (string value in directories)
                    {
                        sw.WriteLine(value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occured: " + ex.Message);
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            th.Abort();
        }

        private void formMain_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            th = new Thread(list);
            th.Start();
            while (th.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(500);
            }
            Close();
            Environment.Exit(0);
        }
    }
}
