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

namespace DriveCrypt
{
    public partial class Form1 : Form
    {
        public const string FolderName = "DriveCrypt";

        private readonly string[] _accessScopes = {DriveService.Scope.Drive};
        private UserCredential _credential;
        private UserCryptor _userCryptor;

        private string _folderId;
        private IEnumerable<Google.Apis.Drive.v3.Data.File> _files; 

        public Form1()
        {
            InitializeComponent();

            Authorize();
            MaintainMainFolder();
            GetFiles();
        }

        public class DirectoryWatcher 
        {
            private FileSystemWatcher watcher;

            public DirectoryWatcher(string path)
            {
                watcher = new FileSystemWatcher(path);
                watcher.Changed += new FileSystemEventHandler(onChangeEvent);
                watcher.Created += new FileSystemEventHandler(onChangeEvent);
                watcher.Deleted += new FileSystemEventHandler(onChangeEvent);
                watcher.Renamed += new RenamedEventHandler(onRenameEvent);

                watcher.EnableRaisingEvents = true;
            }
            
            public static void onChangeEvent(object source, FileSystemEventArgs e)
            {
                MessageBox.Show("File: " + e.FullPath + " " + e.ChangeType);
            }

            public static void onRenameEvent(object source, RenamedEventArgs e)
            {
                MessageBox.Show("File: " + e.OldFullPath + "renamed to " + e.FullPath);
            }
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
            _userCryptor.LoadKeys(password);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "DriveCrypt",
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = "TestFile.dc",
                MimeType = "application/vnd.google-apps.file",
                Parents = new List<string> { _folderId }
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream("files/test.txt", FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "text/plain");
                request.Fields = "id";
                request.Upload();
            }
            var file = request.ResponseBody;
            Console.WriteLine("File ID: " + file.Id);
        }

        private void choseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath);
                string[] dirs = Directory.GetDirectories(fbd.SelectedPath);

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
            DirectoryWatcher watcher = new DirectoryWatcher(fbd.SelectedPath);
        }
    }
}
