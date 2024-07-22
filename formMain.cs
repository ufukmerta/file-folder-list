using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FileFolderList
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }

        string currentDirectory;
        string NewPath(string fPath)
        {
            return Path.Combine(currentDirectory,
                Path.GetFileNameWithoutExtension(fPath) + String.Format("{0:_MM_dd_yyyy__h_mm_ss}",
                File.GetLastWriteTime(fPath)) + ".txt");
        }

        ArrayList filesArr = new ArrayList();
        ArrayList errorList = new ArrayList();

        void ListFiles(string currentDirectory)
        {
            try
            {
                string[] directoriesForFiles = Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
                foreach (string directory in directoriesForFiles)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);
                        filesArr.AddRange(files);
                        ListFiles(directory);
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occured: " + ex.Message);
            }
        }

        ArrayList foldersArr = new ArrayList();
        void ListFolders(string currentDirectory)
        {
            try
            {
                string[] directoriesForDir = Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
                foreach (string directory in directoriesForDir)
                {
                    try
                    {
                        string[] subDir = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);
                        foldersArr.AddRange(subDir);
                        ListFolders(directory);
                    }
                    catch (Exception ex)
                    {
                        errorList.Add(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Occured: " + ex.Message);
            }
        }

        void WriteToFiles()
        {
            foldersArr.Sort();

            CheckFileAndMove(Path.Combine(currentDirectory, "filepaths.txt"));
            WriteToFile(Path.Combine(currentDirectory, "filepaths.txt"), filesArr);

            CheckFileAndMove(Path.Combine(currentDirectory, "folderpaths.txt"));
            WriteToFile(Path.Combine(currentDirectory, "folderpaths.txt"), foldersArr);

            CheckFileAndMove(Path.Combine(currentDirectory, Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "_error.txt"));
            WriteToFile(Path.Combine(currentDirectory, Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "_error.txt"), errorList);            
        }
        void WriteToFile(string fPath, ArrayList arr)
        {
            if (arr.Count > 0)
            {
                using (StreamWriter sw = new StreamWriter(new FileStream(fPath, FileMode.CreateNew), Encoding.UTF8))
                {
                    foreach (string value in arr)
                    {
                        sw.WriteLine(value);
                    }
                }
            }
        }

        void CheckFileAndMove(string fPath)
        {
            if (File.Exists(fPath))
            {
                File.Move(fPath, NewPath(fPath));
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            thFiles.Abort();
            thFolders.Abort();
            Close();
            Environment.Exit(0);
        }

        Thread thFiles;
        Thread thFolders;
        private void formMain_Shown(object sender, EventArgs e)
        {
            //The Application won't add the top directories from the startup path to the folder list.
            //This is a good way to avoid if condition for every recursion in the listFolders method like: if (currentDirectory == Application.StartupPath)
            try
            {
                foldersArr.AddRange(Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly));
            }
            catch (Exception ex)
            {
                errorList.Add(ex.Message);
            }
            thFiles = new Thread(() => ListFiles(Application.StartupPath));
            thFolders = new Thread(() => ListFolders(Application.StartupPath));
            thFiles.Start();
            thFolders.Start();
            while (thFiles.IsAlive || thFolders.IsAlive)
            {
                Application.DoEvents();
                Thread.Sleep(1000);
            }
            WriteToFiles();
            Close();
            Environment.Exit(0);
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            //Tries to create a file in the current directory. If access denied, then asks to run application as administrator.
            currentDirectory = Application.StartupPath;
            try
            {
                string fPath = Path.Combine(currentDirectory, "_" + String.Format("{0:_MM_dd_yyyy__h_mm_ss}", DateTime.Now) + ".txt");
                using (StreamWriter sw = new StreamWriter(new FileStream(fPath, FileMode.CreateNew), Encoding.UTF8))
                {
                    foreach (string value in errorList)
                    {
                        sw.WriteLine(value);
                    }
                }
                File.Delete(fPath);
            }
            catch (UnauthorizedAccessException)
            {
                DialogResult dr =
                    MessageBox.Show("The application needs permission to create files in the current directory. Do you want to run application as administrator?",
                    "Application Needs Permission",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo { FileName = Application.ExecutablePath, UseShellExecute = true, Verb = "runas" });
                }
                Close();
            }
        }
    }
}
