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

        public const string PRIV_KEY_EXTENSION = @".prkey";

        public const string PUB_KEY_EXTENSION = @".pbkey";

        public readonly string UserId;

        //  Call this function to remove the key from memory after use for security.
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(ref string Destination, int Length);

        public UserCryptor(string userId)
        {
            UserId = userId;
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
            return userId + PRIV_KEY_EXTENSION;
        }

        public static string GetPublicKeyPath(string userId)
        {
            return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + GetPublicKeyFilename(userId);
        }

        public static string GetPublicKeyFilename(string userId)
        {
            return userId + PUB_KEY_EXTENSION;
        }

        public void CreateKeys(string masterPassword)
        {
            string pathToKeyFile = GetPrivateKeyPath(UserId);
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

        public void LoadKeys(string masterPassword)
        {
            string pathToKeyFile = GetPrivateKeyPath(UserId);

            var rsaKeysXml = FileCryptor.LoadAndDecryptRsaKeys(pathToKeyFile, masterPassword);
            rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(rsaKeysXml);
            rsa.PersistKeyInCsp = true;
        }

        public void SaveKeys(string masterPassword, bool force = false)
        {
            string pathToKeyFile = GetPrivateKeyPath(UserId);
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
            string pathToKeyFile = GetPrivateKeyPath(UserId);
            string keyFilenameForUser = GetPrivateKeyFilename(UserId);

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

        public void LoadPublicKey()
        {
            string pathToKeyFile = GetPublicKeyPath(UserId);

            LoadPublicKey(pathToKeyFile);
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

        public void SavePublicKey()
        {
            string pathToKeyFile = GetPublicKeyPath(UserId);

            SavePublicKey(pathToKeyFile);
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
