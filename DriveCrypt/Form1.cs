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

        private readonly string[] _accessScopes = { DriveService.Scope.Drive };
        private UserCredential _credential;
        private UserCryptor _userCryptor;
        private Userinfoplus _userInfo;

        private string _folderId;
        private IEnumerable<Google.Apis.Drive.v3.Data.File> _files;
        private string directoryPath;
        private FileSystemWatcher watcher;

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(dragEnter);
            this.DragDrop += new DragEventHandler(dragDrop);

            Authorize();
            MaintainMainFolder();
            GetFiles();
        }


        private void Authorize()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-crypt-auth.json");

                _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _accessScopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }

        private async void GetUserId()
        {
            var oauthSerivce = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "DriveCrypt",
            });

            _userInfo = await oauthSerivce.Userinfo.Get().ExecuteAsync();
        }

        private void MaintainMainFolder()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
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
                HttpClientInitializer = _credential,
                ApplicationName = "DriveCrypt",
            });

            var request = service.Files.List();
            request.Q = string.Format("'{0}' in parents", _folderId);

            var response = await request.ExecuteAsync();

            _files = response.Files;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Must be 64 bits, 8 bytes.
                // Distribute this key to the user who will decrypt this file.
                string sSecretKey;

                // Get the key for the file to encrypt.
                sSecretKey = FileCryptor.GenerateKey();

                // For additional security pin the key.
                GCHandle gch = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

                _userCryptor.EncryptKey(sSecretKey, @openFileDialog1.FileName + ".key");

                // Encrypt the file.        
                FileCryptor.Encrypt(@openFileDialog1.FileName, @openFileDialog1.FileName + ".dc", sSecretKey);

                // Remove the key from memory.
                FileCryptor.ZeroMemory(gch.AddrOfPinnedObject(), sSecretKey.Length * 2);
                gch.Free();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Must be 64 bits, 8 bytes.
                // Distribute this key to the user who will decrypt this file.
                string sSecretKey;

                // Get the key for the file to encrypt.
                sSecretKey = _userCryptor.DecryptKey(@openFileDialog1.FileName.Remove(openFileDialog1.FileName.Length - 3, 3) + ".key");

                // For additional security pin the key.
                GCHandle gch = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

                // Decrypt the file.
                FileCryptor.Decrypt(@openFileDialog1.FileName, @openFileDialog1.FileName.Remove(openFileDialog1.FileName.Length - 3, 3), sSecretKey);

                // Remove the key from memory.
                FileCryptor.ZeroMemory(gch.AddrOfPinnedObject(), sSecretKey.Length * 2);
                gch.Free();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var password = textBox1.Text;

            _userCryptor = new UserCryptor();
            _userCryptor.LoadKeys(_credential.UserId, password);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = "DriveCrypt",
                });

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = openFileDialog1.FileName,
                    MimeType = GetMimeType(openFileDialog1.FileName),
                    Parents = new List<string> { _folderId }
                };

                FilesResource.CreateMediaUpload request;
                using (var stream = new FileStream(openFileDialog1.FileName, FileMode.Open))
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
            this.watcher = new FileSystemWatcher(this.directoryPath);

            this.watcher.Created += new FileSystemEventHandler(onCreateEvent);
            this.watcher.Deleted += new FileSystemEventHandler(onDeleteEvent);

            watcher.EnableRaisingEvents = true;
        }

        private void choseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                directoryPath = fbd.SelectedPath;
                refreshDirecotryList();
                directoryWatcherCreate();
            }
        }

        private void refreshDirecotryList()
        {
            FolderList.Items.Clear();
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);
                string[] dirs = Directory.GetDirectories(directoryPath);

                textBox2.Text = directoryPath;

                foreach (var item in dirs)
                {
                    string[] name = item.Split('\\');
                    FolderList.Items.Add(name.Last());
                }

                foreach (var item in files)
                {
                    string[] name = item.Split('\\');
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
                DirectoryCopy(file, this.directoryPath, true);
            }
            else
            {
                string[] fileName = file.Split('\\');
                try
                {
                    File.Copy(file, this.directoryPath + '\\' + fileName.Last());
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

        private void exportRsaKeys_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                UserCryptor.ExportKeys(_credential.UserId, fbd.SelectedPath);
            }
        }

        private void importRsaKeys_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                UserCryptor.ImportKeys(openFileDialog1.FileName);
            }
        }
    }
}
