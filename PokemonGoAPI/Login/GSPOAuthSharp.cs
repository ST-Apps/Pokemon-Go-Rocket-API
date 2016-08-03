using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Security.Cryptography.Core;

namespace DankMemes.GPSOAuthSharp
{
    // gpsoauth:__init__.py
    // URL: https://github.com/simon-weber/gpsoauth/blob/master/gpsoauth/__init__.py
    public class GPSOAuthClient
    {
        static string b64Key = "AAAAgMom/1a/v0lblO2Ubrt60J2gcuXSljGFQXgcyZWveWLEwo6prwgi3" +
            "iJIZdodyhKZQrNWp5nKJ3srRXcUW+F1BD3baEVGcmEgqaLZUNBjm057pK" +
            "RI16kB0YppeGx5qIQ5QjKzsR8ETQbKLNWgRY0QRNVz34kMJR3P/LgHax/" +
            "6rmf5AAAAAwEAAQ==";
        static RSAParameters androidKey = GoogleKeyUtils.KeyFromB64(b64Key);

        static string version = "0.0.5";
        static string authUrl = "https://android.clients.google.com/auth";
        static string userAgent = "GPSOAuthSharp/" + version;

        private string email;
        private string password;

        public GPSOAuthClient(string email, string password)
        {
            this.email = email;
            this.password = password;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        // _perform_auth_request
        private async Task<Dictionary<string, string>> PerformAuthRequest(Dictionary<string, string> data)
        {
            var nvc = data.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value)).ToList();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("UserAgent", userAgent);      
                string result;
                try                    
                {             
                    IHttpContent content = new HttpFormUrlEncodedContent(nvc);
                    byte[] response =
                        ReadFully((await (await client.PostAsync(new Uri(authUrl), content)).Content.ReadAsInputStreamAsync()).AsStreamForRead());
                    result = Encoding.UTF8.GetString(response);
                }
                catch (WebException e)
                {
                    result = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
                }
                return GoogleKeyUtils.ParseAuthResponse(result);
            }
        }

        // perform_master_login
        public async Task<Dictionary<string, string>> PerformMasterLogin(string service = "ac2dm",
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            string signature = GoogleKeyUtils.CreateSignature(email, password, androidKey);
            var dict = new Dictionary<string, string> {
                { "accountType", "HOSTED_OR_GOOGLE" },
                { "Email", email },
                { "has_permission", 1.ToString() },
                { "add_account", 1.ToString() },
                { "EncryptedPasswd",  signature},
                { "service", service },
                { "source", "android" },
                { "device_country", deviceCountry },
                { "operatorCountry", operatorCountry },
                { "lang", lang },
                { "sdk_version", sdkVersion.ToString() }
            };
            return await PerformAuthRequest(dict);
        }

        // perform_oauth
        public async Task<Dictionary<string, string>> PerformOAuth(string masterToken, string service, string app, string clientSig,
            string deviceCountry = "us", string operatorCountry = "us", string lang = "en", int sdkVersion = 21)
        {
            var dict = new Dictionary<string, string> {
                { "accountType", "HOSTED_OR_GOOGLE" },
                { "Email", email },
                { "has_permission", 1.ToString() },
                { "EncryptedPasswd",  masterToken},
                { "service", service },
                { "source", "android" },
                { "app", app },
                { "client_sig", clientSig },
                { "device_country", deviceCountry },
                { "operatorCountry", operatorCountry },
                { "lang", lang },
                { "sdk_version", sdkVersion.ToString() }
            };
            return await PerformAuthRequest(dict);
        }
    }

    // gpsoauth:google.py
    // URL: https://github.com/simon-weber/gpsoauth/blob/master/gpsoauth/google.py
    class GoogleKeyUtils
    {
        // key_from_b64
        // BitConverter has different endianness, hence the Reverse()
        public static RSAParameters KeyFromB64(string b64Key)
        {
            byte[] decoded = Convert.FromBase64String(b64Key);
            int modLength = BitConverter.ToInt32(decoded.Take(4).Reverse().ToArray(), 0);
            byte[] mod = decoded.Skip(4).Take(modLength).ToArray();
            int expLength = BitConverter.ToInt32(decoded.Skip(modLength + 4).Take(4).Reverse().ToArray(), 0);
            byte[] exponent = decoded.Skip(modLength + 8).Take(expLength).ToArray();
            RSAParameters rsaKeyInfo = new RSAParameters();
            rsaKeyInfo.Modulus = mod;
            rsaKeyInfo.Exponent = exponent;
            return rsaKeyInfo;
        }

        // key_to_struct
        // Python version returns a string, but we use byte[] to get the same results
        public static byte[] KeyToStruct(RSAParameters key)
        {
            byte[] modLength = { 0x00, 0x00, 0x00, 0x80 };
            byte[] mod = key.Modulus;
            byte[] expLength = { 0x00, 0x00, 0x00, 0x03 };
            byte[] exponent = key.Exponent;
            return DataTypeUtils.CombineBytes(modLength, mod, expLength, exponent);
        }

        // parse_auth_response
        public static Dictionary<string, string> ParseAuthResponse(string text)
        {
            Dictionary<string, string> responseData = new Dictionary<string, string>();
            foreach (string line in text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split('=');
                responseData.Add(parts[0], parts[1]);
            }
            return responseData;
        }

        // signature
        public static string CreateSignature(string email, string password, RSAParameters key)
        {
            // TODO: implement!
            return "";
            //var rsa = new  RSACryptoServiceProvider();
            //rsa.ImportParameters(key);
            //SHA1 sha1 = SHA1.Create();
            //byte[] prefix = { 0x00 };
            //byte[] hash = sha1.ComputeHash(KeyToStruct(key)).Take(4).ToArray();
            //byte[] encrypted = rsa.Encrypt(Encoding.UTF8.GetBytes(email + "\x00" + password), true);
            //return DataTypeUtils.UrlSafeBase64(DataTypeUtils.CombineBytes(prefix, hash, encrypted));
        }
    }

    class DataTypeUtils
    {
        public static string UrlSafeBase64(byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray).Replace('+', '-').Replace('/', '_');
        }

        public static byte[] CombineBytes(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}