using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace DankMemes.GPSOAuthSharp
{
    // gpsoauth:__init__.py
    // URL: https://github.com/simon-weber/gpsoauth/blob/master/gpsoauth/__init__.py
    public class GPSOAuthClient
    {
        private static readonly string _b64Key = "AAAAgMom/1a/v0lblO2Ubrt60J2gcuXSljGFQXgcyZWveWLEwo6prwgi3" +
                                                 "iJIZdodyhKZQrNWp5nKJ3srRXcUW+F1BD3baEVGcmEgqaLZUNBjm057pK" +
                                                 "RI16kB0YppeGx5qIQ5QjKzsR8ETQbKLNWgRY0QRNVz34kMJR3P/LgHax/" +
                                                 "6rmf5AAAAAwEAAQ==";

        private static readonly RsaParameters _androidKey = GoogleKeyUtils.KeyFromB64(_b64Key);

        private static readonly string _version = "0.0.5";
        private static readonly string _authUrl = "https://android.clients.google.com/auth";
        private static readonly string _userAgent = "GPSOAuthSharp/" + _version;

        private readonly string _email;
        private readonly string _password;

        public GPSOAuthClient(string email, string password)
        {
            _email = email;
            _password = password;
        }

        // _perform_auth_request
        private async Task<Dictionary<string, string>> PerformAuthRequest(Dictionary<string, string> data)
        {
            var nvc = new List<KeyValuePair<string, string>>();
            foreach (var kvp in data)
            {
                nvc.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", _userAgent);
                string result;
                try
                {
                    var response = await client.PostAsync(_authUrl, new FormUrlEncodedContent(nvc));

                    result = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    result = e.Message;
                }

                return GoogleKeyUtils.ParseAuthResponse(result);
            }
        }

        // perform_master_login
        public async Task<Dictionary<string, string>> PerformMasterLogin(string androidId, string service = "ac2dm",
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            var signature = GoogleKeyUtils.CreateSignature(_email, _password, _androidKey);
            var dict = new Dictionary<string, string>
            {
                {"accountType", "HOSTED_OR_GOOGLE"},
                {"Email", _email},
                {"has_permission", 1.ToString()},
                {"add_account", 1.ToString()},
                {"EncryptedPasswd", signature},
                {"service", service},
                {"source", "android"},
                {"androidId", androidId},
                {"device_country", deviceCountry},
                {"operatorCountry", operatorCountry},
                {"lang", lang},
                {"sdk_version", sdkVersion.ToString()}
            };
            return await PerformAuthRequest(dict);
        }

        // perform_oauth
        public async Task<Dictionary<string, string>> PerformOAuth(string masterToken, string service, string androidId,
            string app, string clientSig,
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            var dict = new Dictionary<string, string>
            {
                {"accountType", "HOSTED_OR_GOOGLE"},
                {"Email", _email},
                {"has_permission", 1.ToString()},
                {"EncryptedPasswd", masterToken},
                {"service", service},
                {"source", "android"},
                {"androidId", androidId},
                {"app", app},
                {"client_sig", clientSig},
                {"device_country", deviceCountry},
                {"operatorCountry", operatorCountry},
                {"lang", lang},
                {"sdk_version", sdkVersion.ToString()}
            };
            return await PerformAuthRequest(dict);
        }
    }

    // gpsoauth:google.py
    // URL: https://github.com/simon-weber/gpsoauth/blob/master/gpsoauth/google.py
    internal class GoogleKeyUtils
    {
        private static readonly string _b64KeyBlob =
            "BgIAAACkAABSU0ExAAQAAAEAAQD5Z676H2sHuPzPHSUMid9z1UQQjUWg1SzKBk0EH7GzMkI5hKh5bHhpitEBqddIpKR7Tptj0FDZoqkgYXJGRWjbPQR14VsUd0UreyfKmadWs0KZEsod2mVIIt4iCK+pjsLEYnmvlckceEGFMZbS5XKgndB6u26U7ZRbSb+/Vv8myg==";

        // key_from_b64
        // BitConverter has different endianness, hence the Reverse()
        public static RsaParameters KeyFromB64(string b64Key)
        {
            var decoded = Convert.FromBase64String(b64Key);
            var modLength = BitConverter.ToInt32(decoded.Take(4).Reverse().ToArray(), 0);
            var mod = decoded.Skip(4).Take(modLength).ToArray();
            var expLength = BitConverter.ToInt32(decoded.Skip(modLength + 4).Take(4).Reverse().ToArray(), 0);
            var exponent = decoded.Skip(modLength + 8).Take(expLength).ToArray();
            var rsaKeyInfo = new RsaParameters
            {
                Modulus = mod,
                Exponent = exponent
            };
            return rsaKeyInfo;
        }

        // key_to_struct
        // Python version returns a string, but we use byte[] to get the same results
        public static byte[] KeyToStruct(RsaParameters key)
        {
            byte[] modLength = {0x00, 0x00, 0x00, 0x80};
            var mod = key.Modulus;
            byte[] expLength = {0x00, 0x00, 0x00, 0x03};
            var exponent = key.Exponent;
            return DataTypeUtils.CombineBytes(modLength, mod, expLength, exponent);
        }

        // parse_auth_response
        public static Dictionary<string, string> ParseAuthResponse(string text)
        {
            var responseData = new Dictionary<string, string>();
            foreach (var line in text.Split(new[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('=');
                responseData.Add(parts[0], parts[1]);
            }
            return responseData;
        }

        // signature
        public static string CreateSignature(string email, string password, RsaParameters key)
        {
            var rsa = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaOaepSha1);
            var keyBlob = CryptographicBuffer.DecodeFromBase64String(_b64KeyBlob);
            var publicKey = rsa.ImportPublicKey(keyBlob, CryptographicPublicKeyBlobType.Capi1PublicKey);

            byte[] prefix = {0x00};
            var hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hash = hashAlgorithm.HashData(KeyToStruct(key).AsBuffer()).ToArray().Take(4).ToArray();

            var encrypted =
                CryptographicEngine.Encrypt(publicKey, Encoding.UTF8.GetBytes(email + "\x00" + password).AsBuffer(),
                    null).ToArray();
            return DataTypeUtils.UrlSafeBase64(DataTypeUtils.CombineBytes(prefix, hash, encrypted));
        }

        public static byte[] HmacSha1Sign(byte[] keyBytes, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var objMacProv = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var hmacKey = objMacProv.CreateKey(keyBytes.AsBuffer());
            var buffHmac = CryptographicEngine.Sign(hmacKey, messageBytes.AsBuffer());
            return buffHmac.ToArray();
        }
    }

    internal class RsaParameters
    {
        public byte[] Modulus { get; set; }
        public byte[] Exponent { get; set; }
    }

    internal class DataTypeUtils
    {
        public static string UrlSafeBase64(byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray).Replace('+', '-').Replace('/', '_');
        }

        public static byte[] CombineBytes(params byte[][] arrays)
        {
            var rv = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}