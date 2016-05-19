using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DriveCrypt.Cryptography
{
    class UserCryptor
    {
        // Declare CspParmeters and RsaCryptoServiceProvider
        // objects with global scope of your Form class.
        CspParameters cspp = new CspParameters();
        RSACryptoServiceProvider rsa;

        // Path variables for source, encryption, and
        // decryption folders. Must end with a backslash.
        const string EncrFolder = @"c:\DriveCrypt\Encrypt\";
        const string DecrFolder = @"c:\DriveCrypt\Decrypt\";
        const string SrcFolder = @"c:\docs\";

        // Public key file
        const string PubKeyFile = @"c:\DriveCrypt\keys\rsaPublicKey.txt";

        // Private key file
        const string PrivKeyFile = @"priv.dckey";

        // Key container name for
        // private/public key value pair.
        const string keyName = "Key01";

        //  Call this function to remove the key from memory after use for security.
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(ref string Destination, int Length);

        // Function to Generate a key pair.
        public void GenerateKeys()
        {
            // Stores a key pair in the key container.
            cspp.KeyContainerName = keyName;
            rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;
        }

        public void LoadKeys(string userId, string masterPassword)
        {
            string pathToKeyFile = Directory.GetCurrentDirectory() + Path.PathSeparator + userId + PrivKeyFile;
            if (!File.Exists(pathToKeyFile))
            {
                GenerateKeys();
                SaveKeys(userId, masterPassword);
            }

            var rsaKeysXml = FileCryptor.LoadAndDecryptRsaKeys(pathToKeyFile, masterPassword);

            rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(rsaKeysXml);
            rsa.PersistKeyInCsp = true;
        }

        public void SaveKeys(string userId, string masterPassword)
        {
            string pathToKeyFile = Directory.GetCurrentDirectory() + Path.PathSeparator + userId + PrivKeyFile;
            if (!File.Exists(pathToKeyFile))
            {
                var rsaKeysXml = rsa.ToXmlString(true);

                FileCryptor.EncryptAndSaveRsaKeys(rsaKeysXml, pathToKeyFile, masterPassword);
            }
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
