using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DriveCrypt.Cryptography
{
    public class UserCryptor
    {
        RSACryptoServiceProvider rsa;

        public const string PRIV_KEY_EXTENSION = @".dckey";

        // Private key file
        private const string PrivKeyFileSuffix = @"priv.dckey";

        private readonly string _userId;

        //  Call this function to remove the key from memory after use for security.
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(ref string Destination, int Length);

        public UserCryptor(string userId)
        {
            _userId = userId;
        }

        // Function to Generate a key pair.
        public void GenerateKeys()
        {
            rsa = new RSACryptoServiceProvider();
        }

        public static string GetPrivateKeyPath(string userId)
        {
            return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + GetPrivateKeyFilename(userId);
        }

        public static string GetPrivateKeyFilename(string userId)
        {
            return userId + PrivKeyFileSuffix;
        }

        public void CreateKeys(string masterPassword)
        {
            string pathToKeyFile = GetPrivateKeyPath(_userId);
            if (!File.Exists(pathToKeyFile))
            {
                GenerateKeys();
                SaveKeys(masterPassword);
            }
            else
            {
                throw new ArgumentException("Keys for this user already exist!");
            }
        }

        public void ChangeMasterPassword(string oldPassword, string newPassword)
        {

        }

        public void LoadKeys(string masterPassword)
        {
            string pathToKeyFile = GetPrivateKeyPath(_userId);

            var rsaKeysXml = FileCryptor.LoadAndDecryptRsaKeys(pathToKeyFile, masterPassword);
            rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(rsaKeysXml);
            rsa.PersistKeyInCsp = true;
        }

        public void SaveKeys(string masterPassword, bool force = false)
        {
            string pathToKeyFile = GetPrivateKeyPath(_userId);
            if (!File.Exists(pathToKeyFile) || force)
            {
                var rsaKeysXml = rsa.ToXmlString(true);

                FileCryptor.EncryptAndSaveRsaKeys(rsaKeysXml, pathToKeyFile, masterPassword);
            }
            else
            {
                throw new ArgumentException("Keys for this user already exist!");
            }
        }

        public void ExportKeys(string directory)
        {
            string pathToKeyFile = GetPrivateKeyPath(_userId);
            string keyFilenameForUser = GetPrivateKeyFilename(_userId);

            File.Copy(pathToKeyFile, directory + Path.DirectorySeparatorChar + keyFilenameForUser);
        }

        public void ImportKeys(string sInputFilename)
        {
            File.Copy(sInputFilename, Directory.GetCurrentDirectory() + sInputFilename.Substring(sInputFilename.LastIndexOf(Path.DirectorySeparatorChar)));
        }

        public void LoadPublicKey(string sInputFilename)
        {
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            var keyParametersUnicode = new byte[fsread.Length];
            fsread.Read(keyParametersUnicode, 0, keyParametersUnicode.Length);

            var rsaKeyXml = Encoding.Unicode.GetChars(keyParametersUnicode).ToString();

            rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(rsaKeyXml);
        }

        public void SavePublicKey(string sOutputFilename)
        {
            FileStream fsOutput = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            var rsaKeysXml = rsa.ToXmlString(false);
            var bytesToSave = Encoding.Unicode.GetBytes(rsaKeysXml);

            fsOutput.Write(bytesToSave, 0, bytesToSave.Length);
            fsOutput.Flush();
            fsOutput.Close();
        }

        public void EncryptKey(string key, string sOutputFilename)
        {
            FileStream fsOutput = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            var keyBytes = Encoding.Unicode.GetBytes(key);

            byte[] keyEncrypted = rsa.Encrypt(keyBytes, false);

            fsOutput.Write(keyEncrypted, 0, keyEncrypted.Length);
            fsOutput.Flush();
            fsOutput.Close();
        }

        public string DecryptKey(string sInputFilename)
        {
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            var keyUnicode = new byte[fsread.Length];
            fsread.Read(keyUnicode, 0, keyUnicode.Length);

            byte[] keyDecrypted = rsa.Decrypt(keyUnicode, false);

            return Encoding.Unicode.GetString(keyDecrypted);
        }
    }
}
