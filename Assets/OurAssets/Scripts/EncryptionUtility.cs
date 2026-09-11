using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionUtility
{
    static readonly string m_Key = "7b83d5a017fa459cf6b3f1b21153dd48";

    public static string EncryptString(string plainText)
    {
        byte[] key = Encoding.UTF8.GetBytes(m_Key[..32]);
        using Aes aesAlgo = Aes.Create();
        aesAlgo.Key = key;
        aesAlgo.GenerateIV();
        ICryptoTransform encryptor = aesAlgo.CreateEncryptor(aesAlgo.Key, aesAlgo.IV);
        using MemoryStream msEncrypt = new MemoryStream();
        msEncrypt.Write(aesAlgo.IV, 0, aesAlgo.IV.Length);
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static string DecryptString(string encryptedText)
    {
        byte[] fullCipher = Convert.FromBase64String(encryptedText);
        byte[] iv = new byte[16];
        byte[] cipher = new byte[fullCipher.Length - 16];
        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
        byte[] key = Encoding.UTF8.GetBytes(m_Key[..32]);
        using Aes aesAlgo = Aes.Create();
        aesAlgo.Key = key;
        aesAlgo.IV = iv;
        ICryptoTransform decryptor = aesAlgo.CreateDecryptor(aesAlgo.Key, aesAlgo.IV);
        using MemoryStream msDecrypt = new MemoryStream(cipher);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }
}
