using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;
using Google.Apis.Requests;
using Google.Apis.Drive.v3.Data;
using System.Windows.Forms;
using DriveCrypt.Cryptography;
using Org.BouncyCastle.Asn1.Crmf;

namespace DriveCrypt.OnlineStores
{
    public static class GDriveManager
    {
        private const string MainFolderName = "DriveCrypt";

        public const string MySharingFolder = "My sharing";

        public const string SharedWithMeFolder = "Shared with me";

        public const string UserKeysFolder = "User keys";

        private static readonly string[] AccessScopes =
        {
            DriveService.Scope.Drive, DriveService.Scope.DriveMetadata, DriveService.Scope.DriveFile,
            Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail
        };

        #region Private members
        private static UserCredential _credential;

        private static Oauth2Service _authService;

        private static DriveService _driveService;

        private static string _mainFolderId;

        private static string _sharedWithMeFolderId;

        private static string _mySharingFolderId;

        private static string _userKeysFolderId;
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
            get { return _mainFolderId ?? (_mainFolderId = GetFolder(MainFolderName)); }
        }

        public static string SharedWithMeFolderId
        {
            get { return _sharedWithMeFolderId ?? (_sharedWithMeFolderId = GetFolder(SharedWithMeFolder, new List<string> { MainFolderId })); }
        }

        public static string MySharingFolderId
        {
            get { return _mySharingFolderId ?? (_mySharingFolderId = GetFolder(MySharingFolder, new List<string> { MainFolderId })); }
        }

        public static string UserKeysFolderId
        {
            get { return _userKeysFolderId ?? (_userKeysFolderId = GetFolder(UserKeysFolder, new List<string> { MainFolderId })); }
        }

        public static string LocalFolderPath { get; set; }
        #endregion

        public static File UploadFile(string fileNameWithPath, string fileNameWithoutPath, string parent = null)
        {
            var fileMetadata = new File
            {
                Name = fileNameWithoutPath,
                MimeType = GetMimeType(fileNameWithoutPath),
                Parents = new List<string> { parent ?? MainFolderId }
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

        public static File CreateFolder(string fileName, string parentId)
        {
            var fileMetadata = new File
            {
                Name = fileName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new[] {parentId}
            };

            var request = DriveService.Files.Create(fileMetadata);
            request.Fields = "id, parents, name";

            return request.Execute();
        }

        public static void SyncFiles()
        {
            if (!string.IsNullOrEmpty(LocalFolderPath))
            {
                var service = DriveService;

                var mineDir = new DirectoryInfo(string.Format("{0}\\{1}", LocalFolderPath, MySharingFolder));
                var sharedWithMeDir = new DirectoryInfo(string.Format("{0}\\{1}", LocalFolderPath, SharedWithMeFolder));

                //Sync files from others
                var getSharedWithMeDataRequest = service.Files.List();
                getSharedWithMeDataRequest.Q = string.Format("name contains '.dc' AND '{0}' in parents", SharedWithMeFolderId);
                getSharedWithMeDataRequest.Fields = "files(modifiedTime, name, id, mimeType)";
                var getSharedWithMeDataResponse = getSharedWithMeDataRequest.Execute();

                var othersDriveFiles = getSharedWithMeDataResponse.Files.ToDictionary(x => x.Name, x => x);
                var othersLocalFiles = sharedWithMeDir.GetFiles().ToDictionary(x => x.Name, x => x);

                var newFiles = othersDriveFiles.Where(x => !othersLocalFiles.ContainsKey(x.Key)).ToList();
                foreach (var newFile in newFiles)
                {
                    var request = DriveService.Files.Get(newFile.Value.Id);
                    var downloadedStream = new MemoryStream();
                    request.Download(downloadedStream);

                    using (var fileStream = System.IO.File.Create(string.Format("{0}\\{1}\\{2}", LocalFolderPath, SharedWithMeFolder, newFile.Key)))
                    {
                        downloadedStream.Seek(0, SeekOrigin.Begin);
                        downloadedStream.CopyTo(fileStream);
                    }
                }

                var modifiedFiles =
                    othersDriveFiles.Where(
                        x =>
                            othersLocalFiles.ContainsKey(x.Key) &&
                            othersLocalFiles[x.Key].LastWriteTime < x.Value.ModifiedTime.Value).ToList();
                foreach (var modifiedFile in modifiedFiles)
                {
                    othersLocalFiles[modifiedFile.Key].Delete();
                    var request = DriveService.Files.Get(modifiedFile.Value.Id);
                    var downloadedStream = new MemoryStream();
                    request.Download(downloadedStream);

                    using (var fileStream = System.IO.File.Create(string.Format("{0}\\{1}\\{2}", LocalFolderPath, SharedWithMeFolder, modifiedFile.Key)))
                    {
                        downloadedStream.Seek(0, SeekOrigin.Begin);
                        downloadedStream.CopyTo(fileStream);
                    }
                }
                

                var deletedFiles = othersLocalFiles.Where(x => !othersDriveFiles.ContainsKey(x.Key)).ToList();
                foreach (var deletedFile in deletedFiles)
                {
                    deletedFile.Value.Delete();
                }

                //Sync my files
                var mySharingFolders = GetMySharingFoldersStructure();
                var mySharingFiles = GetFilesOwnedByMe(mySharingFolders).ToDictionary(x => x.Path, x => x);
                var mineLocalFiles =
                    mineDir.GetFiles("*", SearchOption.AllDirectories)
                        .ToDictionary(
                            x =>
                                x.FullName.Substring(x.FullName.IndexOf(MySharingFolder, StringComparison.Ordinal) + MySharingFolder.Length,
                                    x.FullName.Length - x.FullName.IndexOf(MySharingFolder, StringComparison.Ordinal) - MySharingFolder.Length), x => x);

                //old files
                var oldMineFiles = mySharingFiles.Where(x => !mineLocalFiles.ContainsKey(x.Key)).ToList();
                foreach (var file in oldMineFiles)
                {
                    var removeRequest = DriveService.Files.Delete(file.Value.Id);
                    removeRequest.Execute();
                }

                //modified files
                var mineModifiedFiles =
                    mineLocalFiles.Where(
                        x =>
                            mySharingFiles.ContainsKey(x.Key) &&
                            mySharingFiles[x.Key].ModifiedTime.Value < x.Value.LastWriteTime).ToList();

                //new files
                var newMineFiles = mineLocalFiles.Where(x => !mySharingFiles.ContainsKey(x.Key)).ToList();
                foreach (var file in newMineFiles)
                {
                    var pathElements = file.Key.Split('\\');
                    string parentElementId = null;

                    foreach (var element in pathElements.Where(x => !string.IsNullOrEmpty(x) && !x.EndsWith(".dc")))
                    {
                        var elementFolder = mySharingFolders.FirstOrDefault(x => x.Name == element && x.ParentId == parentElementId);

                        if (elementFolder == null)
                        {
                            var newFolder = CreateFolder(element, parentElementId ?? MySharingFolderId);
                            mySharingFolders.Add(new DriveFile { Id = newFolder.Id, Name = newFolder.Name, ParentId = newFolder.Parents.First()});
                            parentElementId = newFolder.Id;
                        }
                        else
                        {
                            parentElementId = elementFolder.Id;
                        }
                    }

                    UploadFile(file.Value.FullName, file.Value.Name, parentElementId);
                }
            }
        }

        private static List<DriveFile> GetFilesOwnedByMe(List<DriveFile> filesStructure)
        {
            var request = DriveService.Files.List();
            request.Fields = "files(modifiedTime, name, parents, id)";
            //dodac sprawdzenie czy jest wlascicielem oraz not '{0}' in parents AND
            request.Q = string.Format("name contains '.dc' AND mimeType!='application/vnd.google-apps.folder' and trashed = false", SharedWithMeFolder);
            var response = request.Execute();

            var files =
                response.Files.Select(
                    x =>
                        new DriveFile
                        {
                            Id = x.Id,
                            Name = x.Name,
                            ModifiedTime = x.ModifiedByMeTime,
                            ParentId = x.Parents != null ? x.Parents.First() : string.Empty
                        })
                    .Where(x => !string.IsNullOrEmpty(x.ParentId) && x.ParentId != SharedWithMeFolderId)
                    .ToList();

            foreach (var file in files)
            {
                SetParent(file, filesStructure);
            }

            return files;
        }

        private static void SetParent(DriveFile file, List<DriveFile> folders)
        {
            var tempFile = file;

            while (true)
            {
                if (tempFile == null)
                    return;

                tempFile.Parent = folders.FirstOrDefault(x => x.Id == tempFile.ParentId);

                tempFile = tempFile.Parent;
            }
        }

        private static List<DriveFile> GetMySharingFoldersStructure()
        {
            var request = DriveService.Files.List();
            request.Fields = "files(modifiedTime, name, parents, id)";
            request.Q = string.Format("'{0}' in parents AND mimeType='application/vnd.google-apps.folder'", MySharingFolderId);
            var response = request.Execute();

            var driveFolders = response.Files.ToDictionary(x => x.Name, x => x);

            var foldersList = new List<DriveFile>();
            foldersList.AddRange(driveFolders.Select(x => new DriveFile { Id = x.Value.Id, Name = x.Value.Name }));

            while (driveFolders.Any())
            {
                request.Q = string.Format("{0} AND mimeType='application/vnd.google-apps.folder'",
                    string.Join(" or ",
                        driveFolders.Select(mineDriveFile => string.Format("'{0}' in parents", mineDriveFile.Value.Id))
                            .ToList()));

                response = request.Execute();
                driveFolders = response.Files.ToDictionary(x => x.Name, x => x);

                foldersList.AddRange(driveFolders.Select(x => new DriveFile { Id = x.Value.Id, Name = x.Value.Name, ParentId = x.Value.Parents.First() }));
            }

            return foldersList;
        }

        public static void SyncNewFiles()
        {
            var service = DriveService;

            var getDataRequest = service.Files.List();
            getDataRequest.Q = "name contains '.dc' AND sharedWithMe";
            getDataRequest.Fields = "files(id, parents, modifiedTime)";

            var getDataResponse = getDataRequest.Execute();
            var files = getDataResponse.Files.Where(x => x.Parents == null).ToList();

            //Change to batch update
            foreach (var file in files)
            {
                var updateRequest = DriveService.Files.Update(new File(), file.Id);
                updateRequest.Fields = "id, parents";
                updateRequest.AddParents = SharedWithMeFolderId;
                updateRequest.Execute();
            }
        }

        public static void ShareFile(string fileId, string recipientEmail, RoleType roleType)
        {
            var batch = new BatchRequest(DriveService);
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
            {
                if (error != null)
                {
                    MessageBox.Show("Could not share the file with " + recipientEmail + "!\nReason: " + error.Message, "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Permission userPermission = new Permission();
            userPermission.Type = "user";
            userPermission.Role = roleType.ToString();
            userPermission.EmailAddress = recipientEmail;

            var request = DriveService.Permissions.Create(userPermission, fileId);
            request.Fields = "id";

            batch.Queue(request, callback);

            batch.ExecuteAsync();
        }

        public static void SyncNewUserKeys()
        {
            var service = DriveService;

            var getDataRequest = service.Files.List();
            getDataRequest.Q = "name contains '" + UserCryptor.PUB_KEY_EXTENSION + "' AND sharedWithMe";
            getDataRequest.Fields = "files(id, parents, modifiedTime)";

            var getDataResponse = getDataRequest.Execute();
            var files = getDataResponse.Files.Where(x => x.Parents == null).ToList();

            //Change to batch update
            foreach (var file in files)
            {
                var updateRequest = DriveService.Files.Update(new File(), file.Id);
                updateRequest.Fields = "id, parents";
                updateRequest.AddParents = UserKeysFolderId;
                updateRequest.Execute();
            }
        }

        public static void SyncUserKeys()
        {
            if (!string.IsNullOrEmpty(LocalFolderPath))
            {
                var service = DriveService;

                var userKeysDir = new DirectoryInfo(string.Format("{0}\\{1}", LocalFolderPath, UserKeysFolder));

                //Sync files from others
                var getUserKeysDataRequest = service.Files.List();
                getUserKeysDataRequest.Q = string.Format("name contains '" + UserCryptor.PUB_KEY_EXTENSION + "' AND '{0}' in parents", UserKeysFolderId);
                getUserKeysDataRequest.Fields = "files(modifiedTime, name, id, mimeType)";
                var getUserKeysDataResponse = getUserKeysDataRequest.Execute();

                var othersDriveFiles = getUserKeysDataResponse.Files.ToDictionary(x => x.Name, x => x);
                var othersLocalFiles = userKeysDir.GetFiles().ToDictionary(x => x.Name, x => x);

                var newFiles = othersDriveFiles.Where(x => !othersLocalFiles.ContainsKey(x.Key)).ToList();
                foreach (var newFile in newFiles)
                {
                    var downloadedStream = new MemoryStream();
                    var request = DriveService.Files.Get(newFile.Value.Id);
                    request.Download(downloadedStream);

                    using (var fileStream = System.IO.File.Create(string.Format("{0}\\{1}\\{2}", LocalFolderPath, UserKeysFolder, newFile.Key)))
                    {
                        downloadedStream.Seek(0, SeekOrigin.Begin);
                        downloadedStream.CopyTo(fileStream);
                    }
                }
            }
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
                ApplicationName = "DriveCrypt"
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

        private static string GetFolder(string folderName, IList<string> parents = null)
        {
            var service = DriveService;

            var fileList = service.Files.List().Execute();

            if (fileList.Files.All(x => x.Name != folderName))
            {
                var fileMetadata = new File
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = parents
                };

                var request = service.Files.Create(fileMetadata);
                request.Fields = "id";

                return request.Execute().Id;
            }

            return fileList.Files.First(x => x.Name == folderName).Id;
        }

        // tries to figure out the mime type of the file.
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        private static string ResolveFileNameWithExtFromPath(string filePath)
        {
            return filePath.Remove(0, filePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }
        #endregion
    }

    public class DriveFile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ParentId { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public string Path
        {
            get { return Parent == null ? "\\" + Name : Parent.Path + "\\" + Name; }
        }

        public DriveFile Parent { get; set; }
    }
}
