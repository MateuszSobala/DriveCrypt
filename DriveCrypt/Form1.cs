using System;
using System.Collections.Generic;
using System.IO;
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

            //TEST POBIERAJACY LISTE PLIKOW Z DYSKU - DO USUNIECIA
            //var service = new DriveService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = _credential,
            //    ApplicationName = "DriveCrypt",
            //});

            //var listRequest = service.Files.List();
            //listRequest.PageSize = 10;
            //listRequest.Fields = "nextPageToken, files(id, name)";

            //IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
        }
    }
}
