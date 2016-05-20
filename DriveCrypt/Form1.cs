using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using DriveCrypt.Cryptography;
using System.Runtime.InteropServices;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;

namespace DriveCrypt
{
    public partial class Form1 : Form
    {
        public const string FolderName = "DriveCrypt";

        private readonly string[] _accessScopes = { DriveService.Scope.Drive, Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail };
        private AuthorizationForm _authorizationForm;

        private string _folderId;
        private IEnumerable<Google.Apis.Drive.v3.Data.File> _files;
        private string _directoryPath;
        private FileSystemWatcher _folderWatcher;

        public Form1(AuthorizationForm authorizationForm)
        {
            InitializeComponent();
            FormClosed += Form1_FormClosed;
            AllowDrop = true;
            DragEnter += new DragEventHandler(dragEnter);
            DragDrop += new DragEventHandler(dragDrop);

            _authorizationForm = authorizationForm;
            MaintainMainFolder();
            GetFiles();
            userNameLabel.Text = _authorizationForm._userInfo.Email;
        }

        private void MaintainMainFolder()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _authorizationForm._credential,
                ApplicationName = "DriveCrypt",
            });

            var fileList = service.Files.List().Execute();

            if (fileList.Files.All(x => x.Name != FolderName))
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = FolderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var request = service.Files.Create(fileMetadata);
                request.Fields = "id";

                _folderId = request.Execute().Id;
            }
            else
            {
                _folderId = fileList.Files.First(x => x.Name == FolderName).Id;
            }
        }

        private async void GetFiles()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _authorizationForm._credential,
                ApplicationName = "DriveCrypt",
            });

            var request = service.Files.List();
            request.Q = string.Format("'{0}' in parents", _folderId);

            var response = await request.ExecuteAsync();

            _files = response.Files;
        }

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
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _authorizationForm._credential,
                    ApplicationName = "DriveCrypt",
                });

                var filenameWithoutPath = ofd.FileName.Remove(0, ofd.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = filenameWithoutPath,
                    MimeType = GetMimeType(filenameWithoutPath),
                    Parents = new List<string> { _folderId }
                };

                FilesResource.CreateMediaUpload request;
                using (var stream = new FileStream(ofd.FileName, FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream, "text/plain");
                    request.Fields = "id";
                    request.Upload();
                }
                var file = request.ResponseBody;
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
            MessageBox.Show("File: " + e.FullPath + " " + e.ChangeType);
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
                MessageBox.Show("File: " + e.FullPath + " " + e.ChangeType);
            }
            //refreshDirecotryList();
        }

        public void onDeleteEvent(object source, FileSystemEventArgs e)
        {
            MessageBox.Show("File: " + e.FullPath + " " + e.ChangeType);
            //refreshDirecotryList();
        }

        public void onRenameEvent(object source, RenamedEventArgs e)
        {
            MessageBox.Show("File: " + e.OldFullPath + "renamed to " + e.FullPath);
            //refreshDirecotryList();
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
            this._folderWatcher = new FileSystemWatcher(this._directoryPath);

            this._folderWatcher.Created += new FileSystemEventHandler(onCreateEvent);
            this._folderWatcher.Deleted += new FileSystemEventHandler(onDeleteEvent);

            _folderWatcher.EnableRaisingEvents = true;
        }

        private void chooseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                _directoryPath = fbd.SelectedPath;
                refreshDirectoryList();
                directoryWatcherCreate();
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
