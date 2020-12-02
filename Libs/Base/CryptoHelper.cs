using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;


public static partial class CryptoHelper
{
    const string Key = Key0/*"KEY"*/;
    public const string Key2 = Key1 /*"SECKEY"*/;
    public static string Encrypt(string plainString, string key = Key)
    {
        try
        {
            var hashKey = GetMD5Hash(key);
            var decryptBuffer = CryptographicBuffer.ConvertStringToBinary(plainString, BinaryStringEncoding.Utf8);
            var AES = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            var symmetricKey = AES.CreateSymmetricKey(hashKey);
            var encryptedBuffer = CryptographicEngine.Encrypt(symmetricKey, decryptBuffer, null);
            var encryptedString = CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
            return encryptedString;
        }
        catch (Exception ex)
        {
            ex.PrintException("EncryptException");
            return "";
        }
    }
    public static string Decrypt(string encryptedString, string key = Key)
    {
        try
        {
            var hashKey = GetMD5Hash(key);
            IBuffer decryptBuffer = CryptographicBuffer.DecodeFromBase64String(encryptedString);
            var AES = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            var symmetricKey = AES.CreateSymmetricKey(hashKey);
            var decryptedBuffer = CryptographicEngine.Decrypt(symmetricKey, decryptBuffer, null);
            string decryptedString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, decryptedBuffer);
            return decryptedString;
        }
        catch (Exception ex)
        {
            ex.PrintException("DecryptException");
            return "";
        }
    }
    private static IBuffer GetMD5Hash(string key)
    {
        IBuffer bufferUTF8Msg = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
        HashAlgorithmProvider hashAlgorithmProvider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
        IBuffer hashBuffer = hashAlgorithmProvider.HashData(bufferUTF8Msg);
        if (hashBuffer.Length != hashAlgorithmProvider.HashLength)
        {
            throw new Exception("There was an error creating the hash");
        }
        return hashBuffer;
    }
    private static string PrintException(this Exception ex, string name = null)
    {
        var sb = new StringBuilder();
        if (name == null)
            sb.AppendLine("Exeption thrown:");
        else sb.AppendLine($"Exeption thrown in '{name}': ");
        sb.AppendLine(ex.Message);
        sb.AppendLine("Source: " + ex.Source);
        sb.AppendLine("StackTrace: " + ex.StackTrace);
        sb.AppendLine();
        var content = sb.PrintDebug();
        return content;
    }
    private static string PrintDebug(this object obj)
    {
        var content = Convert.ToString(obj);
        Debug.WriteLine(content);
        return content;
    }
}