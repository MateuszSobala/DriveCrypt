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
        private IEnumerable<Google.Apis.Drive.v3.Data.File> _sharedWithMeFiles;
        private string _directoryPath;
        private FileSystemWatcher _decryptWatcher = null;
        private FileSystemWatcher _encryptWatcher = null;

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
            
            ReadFolder();
            userNameLabel.Text = _authorizationForm._userInfo.Email;
        }

        private async void GetFiles()
        {
            var service = GDriveManager.DriveService;

            var request = service.Files.List();
            request.Q = string.Format("'{0}' in parents AND trashed = false", GDriveManager.MainFolderId);
            request.Fields = "files(modifiedTime, name, id, owners)";

            var response = await request.ExecuteAsync();

            _files = response.Files;

            request = service.Files.List();
            request.Q = string.Format("'{0}' in parents AND trashed = false", GDriveManager.SharedWithMeFolderId);
            request.Fields = "files(modifiedTime, name, id, owners)";

            response = await request.ExecuteAsync();

            _sharedWithMeFiles = response.Files;

            SendPublicKeyIfNoFileKeyExists();
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
                var filenameWithoutPath = Path.GetFileName(ofd.FileName);

                var file = GDriveManager.UploadFile(ofd.FileName, filenameWithoutPath);
            }
        }

        public void onChangeEvent(object source, FileSystemEventArgs e)
        {
            MessageBox.Show(" DC Event File: " + e.FullPath + " " + e.ChangeType);
        }

        public void onCreateEncryptEvent(object source, FileSystemEventArgs e)
        {
            string newPath = this._directoryPath + "\\My sharing";
            FileAttributes atributes = File.GetAttributes(e.FullPath);
            if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
            {
                var ext = (Path.GetExtension(e.FullPath) ?? string.Empty).ToLower();
                if (!_extensionsToBeIgnoredByWatcher.Any(ext.Equals))
                {
                    while (IsFileLocked(e.FullPath))
                    {
                        //MessageBox.Show("The requested file " + Path.GetFileName(e.FullPath) + " already exists and is used by another process!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //return;
                        Thread.Sleep(100);
                    }
                    string newFilePath = e.FullPath;
                    newPath = newFilePath.Replace(newPath, this._directoryPath);
                    File.Copy(e.FullPath, newPath, true);
                    while (IsFileLocked(newPath))
                    {
                        //MessageBox.Show("The requested file " + Path.GetFileName(e.FullPath) + " already exists and is used by another process!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //return;
                        Thread.Sleep(100);
                    }

                }

                if (!_extensionsToBeIgnoredByWatcher.Any(ext.Equals))
                {
                    if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
                    {
                        FileCryptor.EncryptFile(e.FullPath, _authorizationForm._userCryptor);
                        File.Delete(e.FullPath);
                    }
                }      
            }       
            RefreshDirectoryList();
        }


        public void onCreateDecryptedEvent(object source, FileSystemEventArgs e)
        {
            var ext = (Path.GetExtension(e.FullPath) ?? string.Empty).ToLower();
            if (_extensionsToBeIgnoredByWatcher[0] == ext)
            {
                string newPath = this._directoryPath + "\\Shared with me";
                FileAttributes atributes = File.GetAttributes(e.FullPath);
                if ((atributes & FileAttributes.Directory) != FileAttributes.Directory)
                {
                
                    while (IsFileLocked(e.FullPath))
                    {
                        //MessageBox.Show("The requested file " + Path.GetFileName(e.FullPath) + " already exists and is used by another process!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //return;
                        Thread.Sleep(100);
                    }
                    string newFilePath = e.FullPath;
                    string decryptedFile = FileCryptor.DecryptFile(e.FullPath, _authorizationForm._userCryptor);
                    string[] decryptedFileName = decryptedFile.Split(Path.DirectorySeparatorChar);
                    string toDelete = newFilePath;
                    newPath = newFilePath.Replace(newPath, this._directoryPath);
                    string[] fileName = newPath.Split(Path.DirectorySeparatorChar);
                    newPath = newPath.Replace(fileName.Last(), decryptedFileName.Last());
                    toDelete = toDelete.Replace(fileName.Last(), decryptedFileName.Last());
                    File.Copy(decryptedFile, newPath,true);
                    File.Delete(toDelete);
                    while (IsFileLocked(newPath))
                    {
                        //MessageBox.Show("The requested file " + Path.GetFileName(e.FullPath) + " already exists and is used by another process!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //return;
                        Thread.Sleep(100);
                    }

                }
            }
            RefreshDirectoryList();
        }

        public void onDeleteEvent(object source, FileSystemEventArgs e)
        {
            RefreshDirectoryList();
        }

        public void onCreateDcEvent(object source, FileSystemEventArgs e)
        {
            MessageBox.Show("DC Event File: " + e.FullPath + " " + e.ChangeType);
            RefreshDirectoryList();
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

        private void DirectoryWatcherCreate()
        {
            _encryptWatcher = new FileSystemWatcher(this._directoryPath+"\\My sharing");

            _encryptWatcher.Created += new FileSystemEventHandler(onCreateEncryptEvent);
            _encryptWatcher.Deleted += new FileSystemEventHandler(onDeleteEvent);

            _encryptWatcher.EnableRaisingEvents = true;
            _encryptWatcher.IncludeSubdirectories = true;
            _encryptWatcher.SynchronizingObject = this;

            _decryptWatcher = new FileSystemWatcher(this._directoryPath + "\\Shared with me");

            _decryptWatcher.Created += new FileSystemEventHandler(onCreateDecryptedEvent);
            _decryptWatcher.Deleted += new FileSystemEventHandler(onDeleteEvent);

            _decryptWatcher.EnableRaisingEvents = true;
            _decryptWatcher.IncludeSubdirectories = true;
            _decryptWatcher.SynchronizingObject = this;

           // _driveWatcher = new FileSystemWatcher("C:\\");
           // _driveWatcher.Created += new FileSystemEventHandler(onCreateDcEvent);
           // _driveWatcher.Changed += new FileSystemEventHandler(onChangeEvent);
           // _driveWatcher.Filter = "*.dc";
           // _driveWatcher.EnableRaisingEvents = true;
           // _driveWatcher.IncludeSubdirectories = true;
           // _driveWatcher.SynchronizingObject = this;
        }

        private void chooseFolder_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SynchronizeFolder(fbd.SelectedPath);

                SaveChosenFolder(fbd.SelectedPath, true);
            }
        }

        private void SynchronizeFolder(string folderPath)
        {
            PrepareFolderStructure(folderPath);

            _directoryPath = folderPath;

            GDriveManager.LocalFolderPath = _directoryPath;
            GDriveManager.SyncNewFiles();
            GDriveManager.SyncFiles();
            GDriveManager.SyncNewUserKeys();
            GDriveManager.SyncUserKeys();

            GetFiles();
            RefreshDirectoryList();
            DirectoryWatcherCreate();
        }

        private void SendPublicKeyIfNoFileKeyExists()
        {
            var dcFiles = _sharedWithMeFiles.Where(x => Path.GetExtension(x.Name) == FileCryptor.DRIVE_CRYPT_EXTENSTION).ToDictionary(x => Path.GetFileNameWithoutExtension(x.Name), x => x.Owners.First());
            var fileKeyFiles = _sharedWithMeFiles.Where(x => Path.GetExtension(x.Name) == FileCryptor.FILE_KEY_EXTENSION).Select(x => Path.GetFileNameWithoutExtension(x.Name));

            foreach (var dcFile in dcFiles)
            {
                if (!fileKeyFiles.Any(fk => fk.StartsWith(dcFile.Key) && fk.EndsWith(_authorizationForm._userId)))
                {
                    SharePublicKey(dcFile.Value.EmailAddress);
                }
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

        private void SaveChosenFolder(string path, bool isExist)
        {
            var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/dirPath.txt");
            var file = new StreamWriter(credPath);
            file.Write(path);
            file.Close();
        }

        private void ReadFolder()
        {
            var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/dirPath.txt");
            if (File.Exists(credPath))
            {
                var folderPath = File.ReadAllText(credPath);
                SynchronizeFolder(folderPath);
            }
            else
            {
                MessageBox.Show("Please choose folder before using the application!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BuildTree(DirectoryInfo directoryInfo, TreeNodeCollection addInMe)
        {
            TreeNode curNode = addInMe.Add(directoryInfo.Name);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                TreeNode node = new TreeNode();
                node.Text = file.Name;
                node.Tag = file.FullName;
                node.ContextMenuStrip = FolderListMenu;
                curNode.Nodes.Add(node);
            }
            foreach (DirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                BuildTree(subdir, curNode.Nodes);
            }
        }

        private void RefreshDirectoryList()
        {
            FolderList.Nodes.Clear();
            if (!string.IsNullOrWhiteSpace(_directoryPath))
            {
                DirectoryInfo dirs = new DirectoryInfo(_directoryPath);

                textBox2.Text = _directoryPath;
                BuildTree(dirs, FolderList.Nodes);
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
                if (!IsValidEmail(emailToShare))
                {
                    MessageBox.Show("Invalid email address!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                GDriveManager.ShareFile(ofd.FileName, emailToShare, _authorizationForm._userInfo.Name);

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
                GDriveManager.UploadFile(keyFilename, keyFilenameWithoutPath);
                GDriveManager.ShareFile(keyFilename, emailToShare, _authorizationForm._userInfo.Name);
            }
        }

        private void sharePublicKey_Click(object sender, EventArgs e)
        {
            var emailToShare = emailInput.Text;
            if (!IsValidEmail(emailToShare))
            {
                MessageBox.Show("Invalid email address!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SharePublicKey(emailToShare);
        }

        private void SharePublicKey(string emailToShare)
        {
            var publicKeyPath = UserCryptor.GetPublicKeyPath(_authorizationForm._userId);
            if (!File.Exists(publicKeyPath))
            {
                _authorizationForm._userCryptor.SavePublicKey();
            }
            var keyFilenameWithoutPath = Path.GetFileName(publicKeyPath);

            var file = GDriveManager.UploadFile(publicKeyPath, keyFilenameWithoutPath);
            GDriveManager.ShareFile(publicKeyPath, emailToShare, _authorizationForm._userInfo.Name);
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
            SynchronizeFolder(_directoryPath);
        }

        private void encodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FolderList.SelectedNode != null)
            {

                TreeNode SelectedNode = FolderList.SelectedNode;

                string newPath = this._directoryPath + "\\My sharing";

                string newFilePath = SelectedNode.Tag.ToString();
                newPath = newFilePath.Replace(this._directoryPath, newPath);
                File.Copy(SelectedNode.Tag.ToString(), newPath, true);
            }
        }
        private void shareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FolderList.SelectedNode != null)
            {
                TreeNode SelectedNode = FolderList.SelectedNode;
                string FilePath = SelectedNode.Tag.ToString();

                var emailToShare = emailInput.Text;
                if (!IsValidEmail(emailToShare))
                {
                    MessageBox.Show("Invalid email address!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                GDriveManager.ShareFile(FilePath, emailToShare, _authorizationForm._userInfo.Name);

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

                var keyFilename = FileCryptor.PrepareKeyForSharing(FilePath, _authorizationForm._userCryptor, shareKeyCryptor);
                var keyFilenameWithoutPath = Path.GetFileName(keyFilename);
                GDriveManager.UploadFile(keyFilename, keyFilenameWithoutPath);
                GDriveManager.ShareFile(keyFilename, emailToShare, _authorizationForm._userInfo.Name);
            }
        }
    }
}
