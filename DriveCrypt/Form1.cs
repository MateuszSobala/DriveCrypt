using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DriveCrypt.Cryptography;
using DriveCrypt.OnlineStores;
using System.Text.RegularExpressions;

namespace DriveCrypt
{
    public partial class Form1 : Form
    {
        private AuthorizationForm _authorizationForm;

        private IEnumerable<Google.Apis.Drive.v3.Data.File> _files;
        private string _directoryPath;
        private FileSystemWatcher _folderWatcher = null;
        private FileSystemWatcher _driveWatcher = null;

        private string[] extensions = { ".dc", ".flkey" };
            

        public Form1(AuthorizationForm authorizationForm)
        {
            InitializeComponent();
            FormClosed += Form1_FormClosed;
            AllowDrop = true;
            DragEnter += new DragEventHandler(dragEnter);
            DragDrop += new DragEventHandler(dragDrop);

            string[] strDrives = Environment.GetLogicalDrives();

            foreach (string strDrive in strDrives)
                MessageBox.Show("Logical Drive: " + strDrive,
                                "Logical Drives",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

            _authorizationForm = authorizationForm;
            //readFolder();
            GetFiles();
            userNameLabel.Text = _authorizationForm._userInfo.Email;
        }

        private async void GetFiles()
        {
            var service = GDriveManager.DriveService;

            var request = service.Files.List();
            request.Q = string.Format("'{0}' in parents", GDriveManager.MainFolderId);

            var response = await request.ExecuteAsync();

            _files = response.Files;
        }

        // Encode File
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = _directoryPath;
            ofd.Filter = "All Files(*.*) | *.*";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileCryptor.EncryptFile(ofd.FileName, _authorizationForm._userCryptor);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = _directoryPath;
            ofd.Filter = "Drive Crypt Files(*" + FileCryptor.DRIVE_CRYPT_EXTENSTION + ") | *" + FileCryptor.DRIVE_CRYPT_EXTENSTION;
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileCryptor.DecryptFile(ofd.FileName, _authorizationForm._userCryptor);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = _directoryPath;
            ofd.Filter = "All Files(*.*) | *.*";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var filenameWithoutPath = ofd.FileName.Remove(0, ofd.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                var file = GDriveManager.UploadFile(ofd.FileName, filenameWithoutPath, GetMimeType(filenameWithoutPath));
            }
        }

        // tries to figure out the mime type of the file.
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
        //-----------------------------------------------------------------------------------------------------------

        public void onChangeEvent(object source, FileSystemEventArgs e)
        {
            MessageBox.Show(" DC Event File: " + e.FullPath + " " + e.ChangeType);
        }

        public void onCreateEvent(object source, FileSystemEventArgs e)
        {
            FileAttributes atributes = File.GetAttributes(e.FullPath);
            if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
            {
                while (IsFileLocked(e.FullPath))
                {
                    Thread.Sleep(100);
                }
            }
            
            var ext = (Path.GetExtension(e.FullPath) ?? string.Empty).ToLower();

            if (!extensions.Any(ext.Equals))
            {
                if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    FileCryptor.EncryptFile(e.FullPath, _authorizationForm._userCryptor);
                    File.Delete(e.FullPath);
                }
            }
            else
            {
                //MessageBox.Show("File: " + e.FullPath);
            }
            refreshDirectoryList();
        }

        public void onDeleteEvent(object source, FileSystemEventArgs e)
        {
            //MessageBox.Show("File: " + e.FullPath + " " + e.ChangeType);
            refreshDirectoryList();
        }

        public void onCreateDcEvent(object source, FileSystemEventArgs e)
        {
            MessageBox.Show("DC Event File: " + e.FullPath +" "+ e.ChangeType);
            refreshDirectoryList();
        }

        private static bool IsFileLocked(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        private void directoryWatcherCreate()
        {
            _folderWatcher = new FileSystemWatcher(this._directoryPath);

            _folderWatcher.Created += new FileSystemEventHandler(onCreateEvent);
            _folderWatcher.Deleted += new FileSystemEventHandler(onDeleteEvent);

            _folderWatcher.EnableRaisingEvents = true;
            _folderWatcher.IncludeSubdirectories = true;
            _folderWatcher.SynchronizingObject = this;

            _driveWatcher = new FileSystemWatcher("C:\\");
            _driveWatcher.Created += new FileSystemEventHandler(onCreateDcEvent);
            _driveWatcher.Changed += new FileSystemEventHandler(onChangeEvent);
            _driveWatcher.Filter = "*.dc";
            _driveWatcher.EnableRaisingEvents = true;
            _driveWatcher.IncludeSubdirectories = true;
            _driveWatcher.SynchronizingObject = this;
        }

        private void chooseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _directoryPath = fbd.SelectedPath;
                GDriveManager.LocalFolderPath = _directoryPath;
                GDriveManager.SyncFiles();

                refreshDirectoryList();
                directoryWatcherCreate();
                choseFolder(fbd.SelectedPath, true);              
            }
        }

        private void choseFolder(string path, bool isExist)
        {
            var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/dirPath.txt");
            System.IO.StreamWriter file = new System.IO.StreamWriter(credPath);
            file.WriteLine(path);
            file.Close();
        }

        private bool readFolder()
        {
            var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/dirPath.txt");
            if (File.Exists(credPath))
            {
                this._directoryPath = System.IO.File.ReadAllText(credPath);
                directoryWatcherCreate();
                refreshDirectoryList();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void refreshDirectoryList()
        {
            FolderList.Items.Clear();
            if (!string.IsNullOrWhiteSpace(_directoryPath))
            {
                string[] files = Directory.GetFiles(_directoryPath);
                string[] dirs = Directory.GetDirectories(_directoryPath);

                textBox2.Text = _directoryPath;

                foreach (var item in dirs)
                {
                    string[] name = item.Split(Path.DirectorySeparatorChar);
                    FolderList.Items.Add(name.Last());
                }

                foreach (var item in files)
                {
                    string[] name = item.Split(Path.DirectorySeparatorChar);
                    FolderList.Items.Add(name.Last());
                }
            }
        }

        private void dragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void dragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                MessageBox.Show(file);
                copyFile(file);
            }
        }

        private void copyFile(string file)
        {
            FileAttributes atributes = File.GetAttributes(file);
            if ((atributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryCopy(file, this._directoryPath, true);
            }
            else
            {
                string[] fileName = file.Split(Path.DirectorySeparatorChar);
                try
                {
                    File.Copy(file, this._directoryPath + Path.DirectorySeparatorChar + fileName.Last());
                }
                catch
                {
                    MessageBox.Show("First choose directory");
                }
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void logout_Click(object sender, EventArgs e)
        {
            _authorizationForm.Show();
            Close();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!_authorizationForm.Visible)
            {
                Application.Exit();
            }
        }

        private void share_Click(object sender, EventArgs e)
        {
            /*
            inputFileName = x
            emailToShare = y
            send inputFileName
            jeśli jesteśmy w posiadaniu klucza publicznego dla tego emaila,
            to
                var shareKeyCryptor = new UserCryptor(Base64Utils.EncodeBase64(emailToShare));
                var keyFilename = FileCryptor.PrepareKeyForSharing(inputFileName, _userCryptor, shareKeyCryptor);
                send keyFilename
            */
        }
    }
}
