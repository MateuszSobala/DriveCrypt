using System;
using System.Collections;
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
            request.Q = $"'{_folderId}' in parents";

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
    }
}
