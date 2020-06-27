using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Configuration;
using System.Net;

namespace Anonymous.Classes
{
    public class Security
    {
        public string Md5Hash(string input)
        {
            string hashedInput = string.Empty;
            var inputToByteArray = Encoding.ASCII.GetBytes(input);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(inputToByteArray);
                hashedInput = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            return hashedInput;
        }

        public static string AppDllHash()
        {
            string hashedAppDll = string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(ConfigurationManager.AppSettings["wwwrootPath"].ToString() + @"Anonymous\bin\anonymous.dll"))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    hashedAppDll = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }

            return hashedAppDll;
        }

        public static byte[] Hash(string value, byte[] salt)
        {
            return Hash(Encoding.UTF8.GetBytes(value), salt);
        }

        /// <summary>
        /// Defaults salt to application specific salt value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Hash(string value)
        {
            byte[] salt = Convert.FromBase64String(ConfigurationManager.AppSettings["AppSaltBase64"]);
            return Hash(Encoding.UTF8.GetBytes(value), salt);
        }

        public static byte[] Hash(string value, string strSalt)
        {
            byte[] salt = Encoding.ASCII.GetBytes(strSalt);
            return Hash(Encoding.UTF8.GetBytes(value), salt);
        }

        static byte[] Hash(byte[] value, byte[] salt)
        {
            byte[] saltedValue = value.Concat(salt).ToArray();

            SHA512Managed sHA512Managed = new SHA512Managed();
            return sHA512Managed.ComputeHash(saltedValue);

        }

        public static bool AreByteArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static void IsLoggedInOrKick(string goAfterUrl = "")
        {
            if (!IsLoggedIn())
            {
                string url = "Login";
                if (goAfterUrl != string.Empty)
                {
                    url += "?goAfter=" + goAfterUrl;
                }
                HttpContext.Current.Response.Redirect(url);
                HttpContext.Current.Response.End();
            }
        }

        public static bool IsLoggedIn()
        {
            if (HttpContext.Current.Session["anonId"] != null)
            {
                return true;
            }

            return false;
        }

        public static void IsAdminOrKick()
        {
            bool shouldKick = false;

            if (!IsLoggedIn())
            {
                shouldKick = true;
            }
            else
            {
                if (!Db.Common.IsAdmin())
                {
                    shouldKick = true;
                }
            }

            if (shouldKick)
            {
                HttpContext.Current.Response.Redirect("Login");
                HttpContext.Current.Response.End();
            }

        }

        public static bool IsEmailVerified
        {
            get
            {
                return Db.Common.IsEmailVerified;
            }
        }

        public static bool isRecaptchaChallengeSuccesful()
        {
            string requestUriString = "https://www.google.com/recaptcha/api/siteverify?secret=" + ConfigurationManager.AppSettings["RecaptchaSecret"].ToString() + "&response=" + HttpContext.Current.Request.Form["g-recaptcha-response"];

            var req = (HttpWebRequest)WebRequest.Create(requestUriString);

            using (var wResponse = req.GetResponse())
            {

                using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                {
                    string responseFromServer = readStream.ReadToEnd();
                    //Response.Write(" // responseFromServer: " + responseFromServer);
                    if (!responseFromServer.Contains("\"success\": false"))
                    {
                        //Response.Write(" - success! - ");
                        //recaptchaSuccess = true;
                        return true;
                    }
                }
            }
            return false;
        }

        //While an app specific salt is not the best practice for
        //password based encryption, it's probably safe enough as long as
        //it is truly uncommon. Also too much work to alter this answer otherwise.
        private static byte[] _salt = Convert.FromBase64String(ConfigurationManager.AppSettings["AppSaltBase64"].ToString());

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        public static string EncryptStringAESbyAppSalt(string plainText)
        {
            return EncryptStringAES(plainText, ConfigurationManager.AppSettings["AppSalt"].ToString());
        }


        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }

        public class Rsa
        {
            private static UnicodeEncoding _encoder = new UnicodeEncoding();
            public static string Decrypt(string data, string privateKey)
            {
                var rsa = new RSACryptoServiceProvider(2048);
                var dataArray = data.Split(new char[] { ',' });
                byte[] dataByte = new byte[dataArray.Length];
                for (int i = 0; i < dataArray.Length; i++)
                {
                    dataByte[i] = Convert.ToByte(dataArray[i]);
                }

                rsa.FromXmlString(privateKey);
                var decryptedByte = rsa.Decrypt(dataByte, false);
                return _encoder.GetString(decryptedByte);
            }

            public static string Encrypt(string data, string publicKey)
            {
                var rsa = new RSACryptoServiceProvider(2048);
                rsa.FromXmlString(publicKey);
                var dataToEncrypt = _encoder.GetBytes(data);
                var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();
                var length = encryptedByteArray.Count();
                var item = 0;
                var sb = new StringBuilder();
                foreach (var x in encryptedByteArray)
                {
                    item++;
                    sb.Append(x);

                    if (item < length)
                        sb.Append(",");
                }

                return sb.ToString();
            }

            public static string Decrypt(byte[] data, string privateKey)
            {
                var rsa = new RSACryptoServiceProvider(2048);
                //var dataArray = data.Split(new char[] { ',' });
                //byte[] dataByte = new byte[dataArray.Length];
                //for (int i = 0; i < dataArray.Length; i++)
                //{
                //    dataByte[i] = Convert.ToByte(dataArray[i]);
                //}

                rsa.FromXmlString(privateKey);
                var decryptedByte = rsa.Decrypt(data, false);
                return _encoder.GetString(decryptedByte);
            }
            public static string Encrypt(byte[] data, string publicKey)
            {
                var rsa = new RSACryptoServiceProvider(2048);
                rsa.FromXmlString(publicKey);
                var dataToEncrypt = data;
                var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();
                var length = encryptedByteArray.Count();
                var item = 0;
                var sb = new StringBuilder();
                foreach (var x in encryptedByteArray)
                {
                    item++;
                    sb.Append(x);

                    if (item < length)
                        sb.Append(",");
                }

                return sb.ToString();
            }

        }

        public class AESThenHMAC
        {
            private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

            //Preconfigured Encryption Parameters
            public static readonly int BlockBitSize = 128;
            public static readonly int KeyBitSize = 256;

            //Preconfigured Password Key Derivation Parameters
            public static readonly int SaltBitSize = 64;
            public static readonly int Iterations = 10000;
            public static readonly int MinPasswordLength = 12;

            /// <summary>
            /// Helper that generates a random key on each call.
            /// </summary>
            /// <returns></returns>
            public static byte[] NewKey()
            {
                var key = new byte[KeyBitSize / 8];
                Random.GetBytes(key);
                return key;
            }

            public static byte[] authKeyDefault()
            {
                if (HttpContext.Current.Session["hashedPassword"] != null)
                {
                    return Security.AESThenHMAC.CreateAuthKey(HttpContext.Current.Session["hashedPassword"].ToString());
                }

                return null;
            }

            /// <summary>
            /// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="cryptKey">The crypt key.</param>
            /// <param name="authKey">The auth key.</param>
            /// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
            /// <returns>
            /// Encrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
            /// <remarks>
            /// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
            /// </remarks>
            public static string SimpleEncrypt(string secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
            {
                if (string.IsNullOrEmpty(secretMessage))
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                var plainText = Encoding.UTF8.GetBytes(secretMessage);
                var cipherText = SimpleEncrypt(plainText, cryptKey, authKey, nonSecretPayload);
                return Convert.ToBase64String(cipherText);
            }

            /// <summary>
            /// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="cryptKey">The crypt key.</param>
            /// <param name="authKey">The auth key.</param>
            /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
            /// <returns>
            /// Decrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
            public static string SimpleDecrypt(string encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
            {
                if (string.IsNullOrWhiteSpace(encryptedMessage))
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                var cipherText = Convert.FromBase64String(encryptedMessage);
                var plainText = SimpleDecrypt(cipherText, cryptKey, authKey, nonSecretPayloadLength);
                return plainText == null ? null : Encoding.UTF8.GetString(plainText);
            }

            /// <summary>
            /// Simple Encryption (AES) then Authentication (HMAC) of a UTF8 message
            /// using Keys derived from a Password (PBKDF2).
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="password">The password.</param>
            /// <param name="nonSecretPayload">The non secret payload.</param>
            /// <returns>
            /// Encrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">password</exception>
            /// <remarks>
            /// Significantly less secure than using random binary keys.
            /// Adds additional non secret payload for key generation parameters.
            /// </remarks>
            public static string SimpleEncryptWithPassword(string secretMessage, string password, byte[] nonSecretPayload = null)
            {
                if (string.IsNullOrEmpty(secretMessage))
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                var plainText = Encoding.UTF8.GetBytes(secretMessage);
                var cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
                return Convert.ToBase64String(cipherText);
            }

            /// <summary>
            /// Simple Authentication (HMAC) and then Descryption (AES) of a UTF8 Message
            /// using keys derived from a password (PBKDF2). 
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="password">The password.</param>
            /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
            /// <returns>
            /// Decrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
            /// <remarks>
            /// Significantly less secure than using random binary keys.
            /// </remarks>
            public static string SimpleDecryptWithPassword(string encryptedMessage, string password, int nonSecretPayloadLength = 0)
            {
                if (string.IsNullOrWhiteSpace(encryptedMessage))
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                var cipherText = Convert.FromBase64String(encryptedMessage);
                var plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
                return plainText == null ? null : Encoding.UTF8.GetString(plainText);
            }

            public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
            {
                //User Error Checks
                if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "cryptKey");

                if (authKey == null || authKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "authKey");

                if (secretMessage == null || secretMessage.Length < 1)
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                //non-secret payload optional
                nonSecretPayload = nonSecretPayload ?? new byte[] { };

                byte[] cipherText;
                byte[] iv;

                using (var aes = new AesManaged
                {
                    KeySize = KeyBitSize,
                    BlockSize = BlockBitSize,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                })
                {

                    //Use random IV
                    aes.GenerateIV();
                    iv = aes.IV;

                    using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
                    using (var cipherStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                        using (var binaryWriter = new BinaryWriter(cryptoStream))
                        {
                            //Encrypt Data
                            binaryWriter.Write(secretMessage);
                        }

                        cipherText = cipherStream.ToArray();
                    }

                }

                //Assemble encrypted message and add authentication
                using (var hmac = new HMACSHA256(authKey))
                using (var encryptedStream = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(encryptedStream))
                    {
                        //Prepend non-secret payload if any
                        binaryWriter.Write(nonSecretPayload);
                        //Prepend IV
                        binaryWriter.Write(iv);
                        //Write Ciphertext
                        binaryWriter.Write(cipherText);
                        binaryWriter.Flush();

                        //Authenticate all data
                        var tag = hmac.ComputeHash(encryptedStream.ToArray());
                        //Postpend tag
                        binaryWriter.Write(tag);
                    }
                    return encryptedStream.ToArray();
                }

            }

            public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
            {

                //Basic Usage Error Checks
                if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), "cryptKey");

                if (authKey == null || authKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), "authKey");

                if (encryptedMessage == null || encryptedMessage.Length == 0)
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                using (var hmac = new HMACSHA256(authKey))
                {
                    var sentTag = new byte[hmac.HashSize / 8];
                    //Calculate Tag
                    var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
                    var ivLength = (BlockBitSize / 8);

                    //if message length is to small just return null
                    if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                        return null;

                    //Grab Sent Tag
                    Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                    //Compare Tag with constant time comparison
                    var compare = 0;
                    for (var i = 0; i < sentTag.Length; i++)
                        compare |= sentTag[i] ^ calcTag[i];

                    //if message doesn't authenticate return null
                    if (compare != 0)
                        return null;

                    using (var aes = new AesManaged
                    {
                        KeySize = KeyBitSize,
                        BlockSize = BlockBitSize,
                        Mode = CipherMode.CBC,
                        Padding = PaddingMode.PKCS7
                    })
                    {

                        //Grab IV from message
                        var iv = new byte[ivLength];
                        Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

                        using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
                        using (var plainTextStream = new MemoryStream())
                        {
                            using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                            using (var binaryWriter = new BinaryWriter(decrypterStream))
                            {
                                //Decrypt Cipher Text from Message
                                binaryWriter.Write(
                                  encryptedMessage,
                                  nonSecretPayloadLength + iv.Length,
                                  encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
                                );
                            }
                            //Return Plain Text
                            return plainTextStream.ToArray();
                        }
                    }
                }
            }

            public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
            {
                nonSecretPayload = nonSecretPayload ?? new byte[] { };

                //User Error Checks
                if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                    throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

                if (secretMessage == null || secretMessage.Length == 0)
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                var payload = new byte[((SaltBitSize / 8) * 2) + nonSecretPayload.Length];

                Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
                int payloadIndex = nonSecretPayload.Length;

                byte[] cryptKey;
                byte[] authKey;
                //Use Random Salt to prevent pre-generated weak password attacks.
                using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
                {
                    var salt = generator.Salt;

                    //Generate Keys
                    cryptKey = generator.GetBytes(KeyBitSize / 8);

                    //Create Non Secret Payload
                    Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
                    payloadIndex += salt.Length;
                }

                //Deriving separate key, might be less efficient than using HKDF, 
                //but now compatible with RNEncryptor which had a very similar wireformat and requires less code than HKDF.
                using (var generator = new Rfc2898DeriveBytes(password, SaltBitSize / 8, Iterations))
                {
                    var salt = generator.Salt;

                    //Generate Keys
                    authKey = generator.GetBytes(KeyBitSize / 8);

                    //Create Rest of Non Secret Payload
                    Array.Copy(salt, 0, payload, payloadIndex, salt.Length);
                }

                return SimpleEncrypt(secretMessage, cryptKey, authKey, payload);
            }

            public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
            {
                //User Error Checks
                if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                    throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

                if (encryptedMessage == null || encryptedMessage.Length == 0)
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                var cryptSalt = new byte[SaltBitSize / 8];
                var authSalt = new byte[SaltBitSize / 8];

                //Grab Salt from Non-Secret Payload
                Array.Copy(encryptedMessage, nonSecretPayloadLength, cryptSalt, 0, cryptSalt.Length);
                Array.Copy(encryptedMessage, nonSecretPayloadLength + cryptSalt.Length, authSalt, 0, authSalt.Length);

                byte[] cryptKey;
                byte[] authKey;

                //Generate crypt key
                using (var generator = new Rfc2898DeriveBytes(password, cryptSalt, Iterations))
                {
                    cryptKey = generator.GetBytes(KeyBitSize / 8);
                }
                //Generate auth key
                using (var generator = new Rfc2898DeriveBytes(password, authSalt, Iterations))
                {
                    authKey = generator.GetBytes(KeyBitSize / 8);
                }

                return SimpleDecrypt(encryptedMessage, cryptKey, authKey, cryptSalt.Length + authSalt.Length + nonSecretPayloadLength);
            }

            public static byte[] CreateAuthKey(string password, int keyBytes = 32)
            {
                const int Iterations = 300;

                var keyGenerator = new Rfc2898DeriveBytes(password, Convert.FromBase64String(ConfigurationManager.AppSettings["AppSaltBase64"].ToString()), Iterations);
                return keyGenerator.GetBytes(keyBytes);
            }

        }


    }
}