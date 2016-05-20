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
using DriveCrypt.Utils;

namespace DriveCrypt
{
    public partial class AuthorizationForm : Form
    {
        public const string FolderName = "DriveCrypt";

        private readonly string[] _accessScopes = { DriveService.Scope.Drive, Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail };
        public UserCredential _credential { get; private set; }
        public UserCryptor _userCryptor { get; private set; }
        public Userinfoplus _userInfo { get; private set; }
        public string _userId { get; private set; }

        public AuthorizationForm()
        {
            InitializeComponent();

            Authorize();
            GetUserId();
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

            userNameLabel.Text = "Logged in with email: " + _userInfo.Email;
            _userId = Base64Utils.EncodeBase64(_userInfo.Email);
        }

        private void login_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_userId))
            {
                MessageBox.Show("Please authorize your Google account!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _userCryptor = new UserCryptor(_userId);
                _userCryptor.LoadKeys(password.Text);
                var mainForm = new Form1(this);
                Hide();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load user keys! Incorrect master password?\nReason: " + ex.Message, "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exportRsaKeys_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_userId))
            {
                MessageBox.Show("Please authorize your Google account!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (_userCryptor == null)
                        _userCryptor = new UserCryptor(_userId);
                    _userCryptor.ExportKeys(fbd.SelectedPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not export user keys!\nReason: " + ex.Message, "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void importRsaKeys_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_userId))
            {
                MessageBox.Show("Please authorize your Google account!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ofd.Filter = "Drive Crypt user keys (*" + UserCryptor.PRIV_KEY_EXTENSION + ")|*" + UserCryptor.PRIV_KEY_EXTENSION;
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (_userCryptor == null)
                        _userCryptor = new UserCryptor(_userId);
                    _userCryptor.ImportKeys(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not import user keys!\nReason: " + ex.Message, "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void register_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_userId))
            {
                MessageBox.Show("Please authorize your Google account!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Text.Equals(confirmPassword.Text))
            {
                try
                {
                    _userCryptor = new UserCryptor(_userId);
                    _userCryptor.CreateKeys(password.Text);
                    var mainForm = new Form1(this);
                    Hide();
                    mainForm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not create new user keys!\nReason: " + ex.Message, "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Passwords don't match!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void changePassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_userId))
            {
                MessageBox.Show("Please authorize your Google account!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Text.Equals(confirmNewPassword.Text))
            {
                try
                {
                    _userCryptor = new UserCryptor(_userId);
                    _userCryptor.LoadKeys(password.Text);
                    _userCryptor.SaveKeys(newPassword.Text, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not change password! Incorrect master password?\nReason: " + ex.Message, "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Passwords don't match!", "Drive Crypt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
