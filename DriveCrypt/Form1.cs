using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DriveCrypt.Cryptography;
using DriveCrypt.OnlineStores;
using System.Text.RegularExpressions;
using DriveCrypt.Utils;

namespace DriveCrypt
{
    public partial class Form1 : Form
    {
        private AuthorizationForm _authorizationForm;

        private IEnumerable<Google.Apis.Drive.v3.Data.File> _files;
        private string _directoryPath;
        private FileSystemWatcher _folderWatcher = null;
        private FileSystemWatcher _driveWatcher = null;

        private string[] _extensionsToBeIgnoredByWatcher = { FileCryptor.DRIVE_CRYPT_EXTENSTION, FileCryptor.FILE_KEY_EXTENSION, UserCryptor.PUB_KEY_EXTENSION };
            

        public Form1(AuthorizationForm authorizationForm)
        {
            InitializeComponent();
            FormClosed += Form1_FormClosed;
            AllowDrop = true;
            DragEnter += new DragEventHandler(dragEnter);
            DragDrop += new DragEventHandler(dragDrop);

            string[] strDrives = Environment.GetLogicalDrives();

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
            var ofd = new OpenFileDialog();
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
            var ofd = new OpenFileDialog();
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
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = _directoryPath;
            ofd.Filter = "All Files(*.*) | *.*";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var filenameWithoutPath = ofd.FileName.Remove(0, ofd.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                var file = GDriveManager.UploadFile(ofd.FileName, filenameWithoutPath);
            }
        }

        public void onChangeEvent(object source, FileSystemEventArgs e)
        {
            MessageBox.Show(" DC Event File: " + e.FullPath + " " + e.ChangeType);
        }

        public void onCreateEvent(object source, FileSystemEventArgs e)
        {
            FileAttributes atributes = File.GetAttributes(e.FullPath);
            if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
            {
                if (IsFileLocked(e.FullPath))
                {
                    MessageBox.Show("The requested file " + Path.GetFileName(e.FullPath) + " already exists and is used by another process!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            
            var ext = (Path.GetExtension(e.FullPath) ?? string.Empty).ToLower();

            if (!_extensionsToBeIgnoredByWatcher.Any(ext.Equals))
            {
                if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    FileCryptor.EncryptFile(e.FullPath, _authorizationForm._userCryptor);
                }
            }

            refreshDirectoryList();
        }

        public void onDeleteEvent(object source, FileSystemEventArgs e)
        {
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
            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                PrepareFolderStructure(fbd.SelectedPath);

                _directoryPath = fbd.SelectedPath;
                GDriveManager.LocalFolderPath = _directoryPath;
                GDriveManager.SyncFiles();
                GDriveManager.SyncNewUserKeys();
                GDriveManager.SyncUserKeys();

                refreshDirectoryList();
                directoryWatcherCreate();
                choseFolder(fbd.SelectedPath, true);              
            }
        }

        private void PrepareFolderStructure(string directory)
        {
            if (!Directory.EnumerateDirectories(directory, GDriveManager.SharedWithMeFolder).Any())
            {
                Directory.CreateDirectory(directory + Path.DirectorySeparatorChar + GDriveManager.SharedWithMeFolder);
            }
            if (!Directory.EnumerateDirectories(directory, GDriveManager.MySharingFolder).Any())
            {
                Directory.CreateDirectory(directory + Path.DirectorySeparatorChar + GDriveManager.MySharingFolder);
            }
            if (!Directory.EnumerateDirectories(directory, GDriveManager.UserKeysFolder).Any())
            {
                Directory.CreateDirectory(directory + Path.DirectorySeparatorChar + GDriveManager.UserKeysFolder);
            }
        }

        private void choseFolder(string path, bool isExist)
        {
            var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/dirPath.txt");
            var file = new StreamWriter(credPath);
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
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = _directoryPath;
            ofd.Filter = "All Files(*.*) | *.*";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var emailToShare = emailInput.Text;
                if(!IsValidEmail(emailToShare))
                {
                    MessageBox.Show("Invalid email address!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                var filenameWithoutPath = Path.GetFileName(ofd.FileName);

                var file = GDriveManager.UploadFile(ofd.FileName, filenameWithoutPath);
                GDriveManager.ShareFile(file.Id, emailToShare, _authorizationForm._userInfo.Name, filenameWithoutPath);

                var userId = Base64Utils.EncodeBase64(emailToShare);
                var shareKeyCryptor = new UserCryptor(userId);

                var keyFilePath = GetUserKeysFolder() + Path.DirectorySeparatorChar + userId + UserCryptor.PUB_KEY_EXTENSION;
                if (File.Exists(keyFilePath))
                {
                    shareKeyCryptor.LoadPublicKey(keyFilePath);
                }
                else
                {
                    MessageBox.Show("The requested user did not share his keys with you yet!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var keyFilename = FileCryptor.PrepareKeyForSharing(ofd.FileName, _authorizationForm._userCryptor, shareKeyCryptor);
                var keyFilenameWithoutPath = Path.GetFileName(keyFilename);
                file = GDriveManager.UploadFile(keyFilename, keyFilenameWithoutPath);
                GDriveManager.ShareFile(file.Id, emailToShare, _authorizationForm._userInfo.Name, filenameWithoutPath);
            }
        }

        private void sharePublicKey_Click(object sender, EventArgs e)
        {
            var publicKeyPath = UserCryptor.GetPublicKeyPath(_authorizationForm._userId);
            if (!File.Exists(publicKeyPath))
            {
                _authorizationForm._userCryptor.SavePublicKey();
            }

            var emailToShare = emailInput.Text;
            if (!IsValidEmail(emailToShare))
            {
                MessageBox.Show("Invalid email address!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            var keyFilenameWithoutPath = Path.GetFileName(publicKeyPath);

            var file = GDriveManager.UploadFile(publicKeyPath, keyFilenameWithoutPath);
            GDriveManager.ShareFile(file.Id, emailToShare, _authorizationForm._userInfo.Name, keyFilenameWithoutPath);
        }

        #region Private helpers
        private static bool IsValidEmail(string strIn)
        {
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        private string ExcludeFolderStructureFromFilePath(string filePath)
        {
            int index = filePath.IndexOf(_directoryPath);
            string cleanPath = (index < 0)
                ? filePath
                : filePath.Remove(index, _directoryPath.Length);

            return cleanPath;
        }

        private string GetSharedWithMeFolder()
        {
            return _directoryPath + Path.DirectorySeparatorChar + GDriveManager.SharedWithMeFolderId;
        }

        private string GetUserKeysFolder()
        {
            return _directoryPath + Path.DirectorySeparatorChar + GDriveManager.UserKeysFolder;
        }
        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            GDriveManager.SyncFiles();
            GDriveManager.SyncNewUserKeys();
            GDriveManager.SyncUserKeys();

            refreshDirectoryList();
        }
    }
}
