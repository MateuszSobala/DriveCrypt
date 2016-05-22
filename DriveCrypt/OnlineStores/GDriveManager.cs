using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace DriveCrypt.OnlineStores
{
    public static class GDriveManager
    {
        private const string FolderName = "DriveCrypt";

        private static readonly string[] AccessScopes = { DriveService.Scope.Drive, Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail };

        #region Private members
        private static UserCredential _credential;

        private static Oauth2Service _authService;

        private static DriveService _driveService;

        private static string _mainFolderId;
        #endregion

        #region Public access
        public static UserCredential Credential
        {
            get { return _credential ?? (_credential = Authorize()); }
        }

        public static Oauth2Service OAuthService
        {
            get { return _authService ?? (_authService = GetOAuthService()); }
        }

        public static DriveService DriveService
        {
            get { return _driveService ?? (_driveService = GetDriveService()); }
        }

        public static string MainFolderId
        {
            get { return _mainFolderId ?? (_mainFolderId = GetMainFolder()); }
        }
        #endregion

        public static File UploadFile(string fileNameWithPath, string fileNameWithoutPath, string mimeType)
        {
            var fileMetadata = new File
            {
                Name = fileNameWithoutPath,
                MimeType = mimeType,
                Parents = new List<string> { MainFolderId }
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(fileNameWithPath, FileMode.Open))
            {
                request = DriveService.Files.Create(fileMetadata, stream, "text/plain");
                request.Fields = "id";
                request.Upload();
            }

            return request.ResponseBody;
        }

        #region Private helpers
        private static UserCredential Authorize()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-crypt-auth.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    AccessScopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }

        private static Oauth2Service GetOAuthService()
        {
            return new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = "DriveCrypt",
            });
        }

        private static DriveService GetDriveService()
        {
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = "DriveCrypt",
            });
        }

        private static string GetMainFolder()
        {
            var service = DriveService;

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

                return request.Execute().Id;
            }

            return fileList.Files.First(x => x.Name == FolderName).Id;
        }
        #endregion
    }
}
