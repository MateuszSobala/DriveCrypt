using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DriveCrypt.Cryptography
{
    /**
     * https://support.microsoft.com/en-us/kb/307010
     **/
    public sealed class FileCryptor
    {
        public const string FILE_KEY_EXTENSION = ".flkey";

        //  Call this function to remove the key from memory after use for security.
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        // Function to Generate a 64 bits Key.
        public static string GenerateKey()
        {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            AesCryptoServiceProvider aesCrypto = (AesCryptoServiceProvider)Aes.Create();

            // Use the Automatically generated key for Encryption. 
            return Encoding.ASCII.GetString(aesCrypto.Key);
        }

        public static void EncryptFile(string sInputFilename, UserCryptor userCryptor)
        {
            // Must be 64 bits, 8 bytes.
            // Distribute this key to the user who will decrypt this file.
            string sSecretKey;

            // Get the key for the file to encrypt.
            sSecretKey = GenerateKey();

            // For additional security pin the key.
            GCHandle gch = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

            userCryptor.EncryptKey(sSecretKey, sInputFilename + FILE_KEY_EXTENSION);

            // Encrypt the file.        
            Encrypt(sInputFilename, sInputFilename + ".dc", sSecretKey);

            // Remove the key from memory.
            ZeroMemory(gch.AddrOfPinnedObject(), sSecretKey.Length * 2);
            gch.Free();
        }

        public static void DecryptFile(string sInputFilename, UserCryptor userCryptor)
        {
            // Must be 64 bits, 8 bytes.
            // Distribute this key to the user who will decrypt this file.
            string sSecretKey;

            // Get the key for the file to encrypt.
            sSecretKey = userCryptor.DecryptKey(sInputFilename.Remove(sInputFilename.Length - 3, 3) + FILE_KEY_EXTENSION);

            // For additional security pin the key.
            GCHandle gch = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

            // Decrypt the file.
            Decrypt(sInputFilename, sInputFilename.Remove(sInputFilename.Length - 3, 3), sSecretKey);

            // Remove the key from memory.
            ZeroMemory(gch.AddrOfPinnedObject(), sSecretKey.Length * 2);
            gch.Free();
        }

        public static void Encrypt(string sInputFilename, string sOutputFilename, string sKey)
        {
            FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);

            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);

            ICryptoTransform desencrypt = CreateAesEncryptor(sKey);
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            int data;
            while ((data = fsInput.ReadByte()) != -1)
            {
                cryptostream.WriteByte((byte)data);
            }
            cryptostream.FlushFinalBlock();
            cryptostream.Close();
            fsInput.Close();
        }

        public static void Decrypt(string sInputFilename, string sOutputFilename, string sKey)
        {
            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = CreateAesDecryptor(sKey);
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            FileStream fsDecrypted = new FileStream(sOutputFilename, FileMode.Create);

            int data;
            while ((data = cryptostreamDecr.ReadByte()) != -1)
            {
                fsDecrypted.WriteByte((byte)data);
            }
            cryptostreamDecr.Close();
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }

        public static void EncryptAndSaveRsaKeys(string keyParameters, string sOutputFilename, string password)
        {
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.CreateNew, FileAccess.Write);

            ICryptoTransform desencrypt = CreateAesEncryptorWithPass(password);
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            var keyParametersUnicode = Encoding.Unicode.GetBytes(keyParameters);
            cryptostream.Write(keyParametersUnicode, 0, keyParametersUnicode.Length);
            cryptostream.FlushFinalBlock();
            cryptostream.Close();
        }

        public static string LoadAndDecryptRsaKeys(string sInputFilename, string password)
        {
            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = CreateAesDecryptorWithPass(password);
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            var keyParametersUnicode = new byte[fsread.Length];

            cryptostreamDecr.Read(keyParametersUnicode, 0, keyParametersUnicode.Length);
            cryptostreamDecr.Close();

            return Encoding.Unicode.GetString(keyParametersUnicode);
        }

        private static ICryptoTransform CreateAesDecryptor(string key)
        {
            var Aes = CreateAes(key);

            return Aes.CreateDecryptor();
        }

        private static ICryptoTransform CreateAesEncryptor(string key)
        {
            var Aes = CreateAes(key);

            return Aes.CreateEncryptor();
        }

        private static AesCryptoServiceProvider CreateAes(string key)
        {
            AesCryptoServiceProvider Aes = new AesCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For Aes algorithm.
            Aes.Key = Encoding.ASCII.GetBytes(key);
            //Set initialization vector.
            Aes.IV = Encoding.ASCII.GetBytes(key.Substring(0, Aes.IV.Length));

            return Aes;
        }

        private static ICryptoTransform CreateAesDecryptorWithPass(string password)
        {
            var Aes = CreateAesWithPass(password);

            return Aes.CreateDecryptor();
        }

        private static ICryptoTransform CreateAesEncryptorWithPass(string password)
        {
            var Aes = CreateAesWithPass(password);

            return Aes.CreateEncryptor();
        }

        private static AesCryptoServiceProvider CreateAesWithPass(string password)
        {
            AesCryptoServiceProvider Aes = new AesCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For Aes algorithm.
            Aes.Key = CreateKey(password);
            //Set initialization vector.
            Aes.IV = CreateKey(password, 16);

            return Aes;
        }

        private static readonly byte[] Salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        public static byte[] CreateKey(string password, int keyBytes = 32)
        {
            const int Iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, Iterations);
            return keyGenerator.GetBytes(keyBytes);
        }
    }
}
