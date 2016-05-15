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

namespace DriveCrypt
{
    public partial class Form1 : Form
    {
        public const string FolderName = "DriveCrypt";

        private readonly string[] _accessScopes = {DriveService.Scope.Drive};
        private UserCredential _credential;

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
    }
}
