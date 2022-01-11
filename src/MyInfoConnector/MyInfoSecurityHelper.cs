using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace sg.gov.ndi.MyInfoConnector
{
    public class MyInfoSecurityHelper
    {        
	    //Generate Authorization Header Method	    
        public static string GenerateAuthorizationHeader(string defaultHeader, string bearer)
        {
            string authHeader = string.Empty;
            try
            {
                if (bearer != null)
                    authHeader = ApplicationConstant.PKI_SIGN + " " + defaultHeader + "," + bearer;
                else
                    authHeader = ApplicationConstant.PKI_SIGN + " " + defaultHeader;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return authHeader;
        }
        
	    //Generate Base String Method	    
        public static string GenerateBaseString(string method, string url, string baseParams)
        {
            string basestring = string.Empty;
            basestring = method.ToUpper() + "&" + url + "&" + baseParams;
            return basestring;
        }
        
	    //Generate Signature Method	     
        public static string GenerateSignature(string basestring, string privateKey)
        {
            string signature = null;
            try
            {
                if (!string.IsNullOrEmpty(privateKey))
                {
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSA.FromXmlString(privateKey);
                    RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(RSA);
                    RSAFormatter.SetHashAlgorithm("SHA256");
                    SHA256Managed SHhash = new SHA256Managed();
                    byte[] SignedHashValue = RSAFormatter.CreateSignature(SHhash.ComputeHash(Encoding.UTF8.GetBytes(basestring)));
                    signature = System.Convert.ToBase64String(SignedHashValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return signature;
        }

        //Decode Token Method
        public static object DecodeToken(string token)
        {
            string encodedPayload = token.Split('.')[1];
            string decodedPayload = Encoding.ASCII.GetString(FromBase64Url(encodedPayload));
            object jsonObject = JsonConvert.DeserializeObject(decodedPayload);
            return jsonObject;
        }
                	 
	    // Verify Token Method	     
        public static bool VerifyToken(string token, string publicKey)
        {
            bool signVerified = false;
            string[] tokenParts = token.TrimStart('"').TrimEnd('"').Split('.');
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(
              new RSAParameters()
              {
                  Modulus = FromBase64Url(XElement.Parse(publicKey).Element("Modulus").Value),
                  Exponent = FromBase64Url(XElement.Parse(publicKey).Element("Exponent").Value)
              });

            SHA256 sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenParts[0] + '.' + tokenParts[1]));

            RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("SHA256");
            if (rsaDeformatter.VerifySignature(hash, FromBase64Url(tokenParts[2])))
                signVerified = true;
            return signVerified;
        }

        private static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/")
                                  .Replace("-", "+");
            return Convert.FromBase64String(base64);
        }

    }
}
