using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private readonly string[] _accessScopes = {DriveService.Scope.Drive};
        private UserCredential _credential;

        public Form1()
        {
            InitializeComponent();
        }

        private async void LogIn_Click(object sender, EventArgs e)
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-crypt-auth.json");

                _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _accessScopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            MaintainMainFolder();
        }

        private async void MaintainMainFolder()
        {
            const string folderName = "DriveCrypt";

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "DriveCrypt",
            });

            var fileList = await service.Files.List().ExecuteAsync();

            if(fileList.Files.All(x => x.Name != folderName))
            {
                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var request = service.Files.Create(fileMetadata);
                request.Fields = "id";

                await request.ExecuteAsync();
            }
        }
    }
}
