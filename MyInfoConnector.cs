using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Xml;

namespace sg.gov.ndi.MyInfoConnector
{
    public class MyInfoConnector
    {
        public static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private string keystoreFile;
        private string keystoreFilePassword;
        private string keystoreFilePubKey;
        private string publicKeystoreFilePassword;


        private string publicKey;
        private string privateKey;
        private string clientAppId;
        private string clientAppPwd;

        private string redirectUri;        
        private string attributes;
        private string env;
        private string tokenURL;
        private string personURL;
        private string proxyTokenURL;
        private string proxyPersonURL;
        private string useProxy;
        MyInfoSecurityHelper securityHelper = new MyInfoSecurityHelper();
        private static MyInfoConnector instance;

        // Private constructor to avoid client applications to use constructor
        private MyInfoConnector(string path)
        {
            try
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap() { ExeConfigFilename = path };
                Configuration libConfig = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                AppSettingsSection section = (libConfig.GetSection("appSettings") as AppSettingsSection);
                LoadProperties(section);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Return current instance
        public static MyInfoConnector getCurrentInstance()
        {
            if (instance == null)
            {
                throw new Exception("No instance has been initialized.");
            }
            return instance;
        }

        // Create singleton
        public static MyInfoConnector getInstance(string configPath)
        {
            if (instance == null)
            {
                instance = new MyInfoConnector(configPath);
            }
            else
            {
                throw new Exception("Instance has been initialized. Please get the current instance.");
            }
            return instance;
        }

        private void LoadProperties(AppSettingsSection section)
        {
            if (string.IsNullOrEmpty(section.Settings["KEYSTORE"].Value))
                throw new Exception("KeyStore value not found for privatekey or empty in configuration file!");
            else
                this.keystoreFile = section.Settings["KEYSTORE"].Value;

            if (string.IsNullOrEmpty(section.Settings["KEYSTORE_PASSPHRASE"].Value))
                throw new Exception("KeyStore pass phrase not found or empty in configuration file!");
            else
                this.keystoreFilePassword = section.Settings["KEYSTORE_PASSPHRASE"].Value;

            if (string.IsNullOrEmpty(section.Settings["KEYSTORE_Public"].Value))
                throw new Exception("KeyStore value not found for public key or empty in configuration file!");
            else
                this.keystoreFilePubKey = section.Settings["KEYSTORE_Public"].Value;

            if (string.IsNullOrEmpty(section.Settings["KEYSTORE_PASSPHRASE_PUBLIC"].Value))
                throw new Exception("KeyStore pass phrase for public key not found for public key or empty in configuration file!");
            else
                this.publicKeystoreFilePassword = section.Settings["KEYSTORE_PASSPHRASE_PUBLIC"].Value;

            if (string.IsNullOrEmpty(section.Settings["CLIENT_ID"].Value))
                throw new Exception("Client id not found or empty in configuration file!");
            else
                this.clientAppId = section.Settings["CLIENT_ID"].Value;

            if (string.IsNullOrEmpty(section.Settings["CLIENT_SECRET"].Value))
                throw new Exception("Client secret not found or empty in configuration file!");
            else
                this.clientAppPwd = section.Settings["CLIENT_SECRET"].Value;

            if (string.IsNullOrEmpty(section.Settings["REDIRECT_URL"].Value))
                throw new Exception("Redirect url not found or empty in configuration file!");
            else
                this.redirectUri = section.Settings["REDIRECT_URL"].Value;

            if (string.IsNullOrEmpty(section.Settings["ATTRIBUTES"].Value))
                throw new Exception("Attributes not found or empty in configuration file!");
            else
                this.attributes = section.Settings["ATTRIBUTES"].Value;

            if (string.IsNullOrEmpty(section.Settings["ENVIRONMENT"].Value))
                throw new Exception("Environment not found or empty in configuration file!");
            else
                this.env = section.Settings["ENVIRONMENT"].Value;

            if (string.IsNullOrEmpty(section.Settings["TOKEN_URL"].Value))
                throw new Exception("Token url not found or empty in configuration file!");
            else
                this.tokenURL = section.Settings["TOKEN_URL"].Value;

            if (string.IsNullOrEmpty(section.Settings["PERSON_URL"].Value))
                throw new Exception("Person url not found or empty in configuration file!");
            else
                this.personURL = section.Settings["PERSON_URL"].Value;


            if (string.IsNullOrEmpty(section.Settings["USE_PROXY"].Value))
                throw new Exception("Use proxy indicator not found or empty in configuration file!");
            else
            {
                this.useProxy = section.Settings["USE_PROXY"].Value;
                if (this.useProxy.Equals(ApplicationConstant.YES, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(section.Settings["PROXY_TOKEN_URL"].Value))
                        throw new Exception("Proxy token url not found or empty in configuration file!");
                    else
                        this.proxyTokenURL = section.Settings["PROXY_TOKEN_URL"].Value;

                    if (string.IsNullOrEmpty(section.Settings["PROXY_PERSON_URL"].Value))
                        throw new Exception("Proxy person url not found or empty in congiguration file!");
                    else
                        this.proxyPersonURL = section.Settings["PROXY_PERSON_URL"].Value;
                }
            }

            X509Certificate2 certificate = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + keystoreFile, keystoreFilePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            this.privateKey = certificate.PrivateKey.ToXmlString(true);
            X509Certificate2 certificate1 = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + keystoreFilePubKey, keystoreFilePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            this.publicKey = certificate1.GetRSAPublicKey().ToXmlString(false);
        }

        /*    
            
         * Get MyInfo Person Data	    

         * This function takes in all the required variables, invoke the

	     * getAccessToken API to generate the access token. The access token is then

	     * use to invoke the person API to get the Person data.
  	 	    */
        protected static string GetMyInfoPersonData(string authCode, string txnNo, string state, string publicCert,
            string agencyPrivateKey, string clientAppId, string clientAppPwd, string redirectUri, string attributes,
            string env, string tokenURL, string personURL, string proxyTokenURL, string proxyPersonURL, string useProxy)
        {
            string result = null;
            string jsonResponse = null;

            // GET ACCESS TOKEN
            string token = GetAccessToken(authCode, tokenURL, clientAppId, clientAppPwd, redirectUri, env,
                    agencyPrivateKey, state, proxyTokenURL, useProxy);

            //VERIFY and DECODE TOKEN
            JObject jObject = JObject.Parse(MyInfoSecurityHelper.DecodeToken(token).ToString());
            string uinfin = (string)jObject.SelectToken("sub");

            // GET PERSON
            result = MyInfoConnector.GetPersonData(uinfin, "Bearer " + token,
                    txnNo, personURL, clientAppId, attributes, env, agencyPrivateKey, proxyPersonURL, useProxy);

            if (!env.Equals(ApplicationConstant.SANDBOX, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    jsonResponse = MyInfoConnector.DecodePersonData(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                jsonResponse = result;
            }

            return jsonResponse;
        }

        /*
         * Get MyInfo Person Data	
	     * This function will takes in a keystore, retrieve all the properties value
	     * from the class variable and call the main static getMyInfoPersonData
	     * function to retrieve MyInfo Person data.
	    */
        protected string GetMyInfoPersonData(string authCode, string txnNo, string state, string keyStoreDir,
            string keyStorePwd, string publicKeystoreDir, string publicKeyPwd)
        {
            X509Certificate2 certificate = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + keyStoreDir, keyStorePwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            this.privateKey = certificate.PrivateKey.ToXmlString(true);
            X509Certificate2 certificate1 = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + publicKeystoreDir, publicKeyPwd, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            this.publicKey = certificate1.GetRSAPublicKey().ToXmlString(false);

            return GetMyInfoPersonData(authCode, txnNo, state, this.publicKey, this.privateKey, this.clientAppId,
                    this.clientAppPwd, this.redirectUri, this.attributes, this.env, this.tokenURL,
                    this.personURL, this.proxyTokenURL, this.proxyPersonURL, this.useProxy);
        }

        /* 
	     * Get MyInfo Person Data	 
	     * This function will retrieve all the properties value from the class
	     * variable and call the static getMyInfoPersonData function to retrieve
	     * MyInfo Person data.
	    */
        public string GetMyInfoPersonData(string authCode, string txnNo, string state)
        {
            return GetMyInfoPersonData(authCode, txnNo, state, this.keystoreFile, this.keystoreFilePassword, this.keystoreFilePubKey, this.publicKeystoreFilePassword);
        }

        /*
	     * Get MyInfo Person Data
	     * This function will retrieve all the properties value from the class
	     * variable and call the static getMyInfoPersonData function to retrieve
	     * MyInfo Person data.
	    */
        public string GetMyInfoPersonData(string authCode, string state)
        {
            return GetMyInfoPersonData(authCode, state, this.keystoreFile, this.keystoreFilePassword, this.keystoreFilePubKey, this.publicKeystoreFilePassword);
        }

        /* 
	     * Get MyInfo Person Data
	     * This function will retrieve all the properties value from the class
	     * variable and call the static getMyInfoPersonData function to retrieve
	     * MyInfo Person data.    
       */
        protected string GetMyInfoPersonData(string authCode, string state, string keyStoreDir, string keyStorePwd, string publicCert, string publicKeystoreFilePassword)
        {
            return GetMyInfoPersonData(authCode, null, state, keyStoreDir, keyStorePwd, publicCert, publicKeystoreFilePassword);
        }

        /*	 
	     * Get Authorization(Access) Token
	     * This API is invoked by your application server to obtain an "access
	     * token", which can be used to call the Person API for the actual data.
	     * Your application needs to provide a valid "authorisation code" from the
	     * authorise API in exchange for the "access token".
	    */
        protected static string GetAccessToken(string authCode, string apiUrl, string clientAppId, string clientAppPwd,
                    string redirectUri, string env, string myinfoPrivateKey, string state, string proxyTokenURL, string useProxy)
        {
            string baseParams = string.Empty;
            string AccessToken = string.Empty;
            try
            {
                string nonce = RandomString();
                long timestamp = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

                string authHeader = null;
                string signature = null;
                string userInputURL = useProxy.Equals(ApplicationConstant.YES) ? proxyTokenURL : apiUrl;

                // A) Forming the Signature Base String
                baseParams = ApplicationConstant.APP_ID + "=" + clientAppId + "&" + ApplicationConstant.CLIENT_ID + "=" + clientAppId + "&" + ApplicationConstant.CLIENT_SECRET + "=" + clientAppPwd + "&" + ApplicationConstant.CODE + "=" + authCode + "&" + ApplicationConstant.GRANT_TYPE + "=" + ApplicationConstant.AUTHORIZATION_CODE + "&" + ApplicationConstant.NONCE + "=" + nonce + "&" + ApplicationConstant.REDIRECT_URI + "=" + redirectUri + "&" + ApplicationConstant.SIGNATURE_METHOD + "=" + ApplicationConstant.RS256 + "&" + ApplicationConstant.TIMESTAMP + "=" + timestamp + "&" + ApplicationConstant.STATE + "=" + state;
                string baseString = MyInfoSecurityHelper.GenerateBaseString(ApplicationConstant.POST_METHOD, apiUrl, baseParams);
                if (!env.Equals(ApplicationConstant.SANDBOX, StringComparison.InvariantCultureIgnoreCase))
                {
                    // B) Signing Base String to get Digital Signature
                    if (baseString != null)
                    {
                        signature = MyInfoSecurityHelper.GenerateSignature(baseString, myinfoPrivateKey);
                    }

                    // C) Assembling the Header
                    if (signature != null)
                    {
                        string headers = ApplicationConstant.APP_ID + "=\"" + clientAppId + "\"," + ApplicationConstant.NONCE + "=\"" + nonce + "\"," + ApplicationConstant.SIGNATURE_METHOD + "=\"" + ApplicationConstant.RS256 + "\"" + "," + ApplicationConstant.SIGNATURE + "=\"" + signature + "\"," + ApplicationConstant.TIMESTAMP + "=\"" + timestamp + "\"";
                        authHeader = MyInfoSecurityHelper.GenerateAuthorizationHeader(headers, null);
                    }
                }

                // D) Assembling the params
                string parameters = ApplicationConstant.GRANT_TYPE + "=" + ApplicationConstant.AUTHORIZATION_CODE + "&" + ApplicationConstant.CODE + "=" + authCode + "&" + ApplicationConstant.REDIRECT_URI + "=" + redirectUri + "&" + ApplicationConstant.CLIENT_ID + "=" + clientAppId + "&" + ApplicationConstant.CLIENT_SECRET + "=" + clientAppPwd + "&" + ApplicationConstant.STATE + "=" + state;

                // E) Prepare request for TOKEN API
                var request = (HttpWebRequest)WebRequest.Create(userInputURL);
                request.Method = ApplicationConstant.POST_METHOD;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add(ApplicationConstant.CACHE_CONTROL, ApplicationConstant.NO_CACHE);
                if (!env.Equals(ApplicationConstant.SANDBOX, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(authHeader))
                    request.Headers.Add(ApplicationConstant.AUTHORIZATION, authHeader);

                byte[] byteArray = Encoding.UTF8.GetBytes(parameters.ToString());
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                request.Accept = "application/json";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                object jsonObject = JsonConvert.DeserializeObject(responseString);
                var jsonObj = JObject.Parse(jsonObject.ToString());
                AccessToken = (string)jsonObj.SelectToken("access_token");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return AccessToken;
        }

        /*
        * Get Person Data
        * This method calls the Person API and returns a JSON response with the
        * personal data that was requested. Your application needs to provide a
        * valid "access token" in exchange for the JSON data. Once your application
        * receives this JSON data, you can use this data to populate the online
        * form on your application.
        */
        protected static string GetPersonData(string uinFin, string bearer, string txnNo, string apiUrl,
                    string clientAppId, string attributes, string env, string myinfoPrivateKey, string proxyPersonURL,
                    string useProxy)
        {

            string baseParams = string.Empty;
            string content = string.Empty;

            try
            {

                string userInputURL = (useProxy == ApplicationConstant.YES) ? proxyPersonURL : apiUrl;
                userInputURL = userInputURL + "/" + uinFin + "/";
                apiUrl = apiUrl + "/" + uinFin + "/";

                string nonce = RandomString();
                long timestamp = (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

                string signature = null;
                string authHeader = null;

                // A) Forming the Signature Base String
                baseParams = ApplicationConstant.APP_ID + "=" + clientAppId + "&" + ApplicationConstant.ATTRIBUTE + "=" + attributes + "&" + ApplicationConstant.CLIENT_ID + "=" + clientAppId + "&" + ApplicationConstant.NONCE + "=" + nonce + "&" + ApplicationConstant.SIGNATURE_METHOD + "=" + ApplicationConstant.RS256 + "&" + ApplicationConstant.TIMESTAMP + "=" + timestamp;

                if (txnNo != null)
                {
                    baseParams = baseParams + "&" + ApplicationConstant.TRANSACTION_NO + "=" + txnNo;
                }
                string baseString = MyInfoSecurityHelper.GenerateBaseString(ApplicationConstant.GET_METHOD, apiUrl, baseParams);

                // B) Signing Base String to get Digital Signature
                if (baseString != null)
                {
                    signature = MyInfoSecurityHelper.GenerateSignature(baseString, myinfoPrivateKey);
                }

                // C) Assembling the Header
                if (signature != null)
                {
                    string header = ApplicationConstant.APP_ID + "=\"" + clientAppId + "\"," + ApplicationConstant.NONCE + "=\"" + nonce + "\"," + ApplicationConstant.SIGNATURE_METHOD + "=\"" + ApplicationConstant.RS256 + "\"" + "," + ApplicationConstant.SIGNATURE + "=\"" + signature + "\"," + ApplicationConstant.TIMESTAMP + "=\"" + timestamp + "\"";
                    authHeader = MyInfoSecurityHelper.GenerateAuthorizationHeader(header, bearer);                    
                }

                // D) Assembling the params
                userInputURL = userInputURL + "?" + ApplicationConstant.CLIENT_ID + "=" + clientAppId + "&" + ApplicationConstant.ATTRIBUTE + "=" + attributes;

                if (txnNo != null)
                {
                    userInputURL = userInputURL + "&" + ApplicationConstant.TRANSACTION_NO + "=" + txnNo;
                }

                var request = (HttpWebRequest)WebRequest.Create(userInputURL);
                request.Headers.Add(ApplicationConstant.CACHE_CONTROL, ApplicationConstant.NO_CACHE);
                request.Method = ApplicationConstant.GET_METHOD;
                request.Headers.Add("Authorization", authHeader);
                var response = (HttpWebResponse)request.GetResponse();
                Stream stream = null;
                using (stream = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(stream))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return content.ToString();
        }

        /*
	     * Get Person Data
	     * This method calls the Person API without the transaction no and returns a
	     * JSON response with the personal data that was requested. Your application
	     * needs to provide a valid "access token" in exchange for the JSON data.
	     * Once your application receives this JSON data, you can use this data to
	     * populate the online form on your application.
         */
        protected static string getPersonData(string uinFin, string bearer, string personurl, string clientAppId,
           string attributes, string env, string myinfoPrivateKey, string proxyPersonURL, string useProxy)
        {
            return GetPersonData(uinFin, bearer, null, personurl, clientAppId, attributes, env, myinfoPrivateKey,
                    proxyPersonURL, useProxy);
        }

        private static string RandomString()
        {
            Random random = new Random();
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 15)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string DecodePersonData(string Data)
        {
            string decodedData = string.Empty;
            try
            {
                //  Decrypt.
                var privateKey = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + MyInfoConnector.instance.keystoreFile, MyInfoConnector.instance.keystoreFilePassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet).PrivateKey as RSACryptoServiceProvider;
                string DecryptedToken = Jose.JWT.Decode(Data, privateKey);

                //Verify
                if (MyInfoSecurityHelper.VerifyToken(DecryptedToken,MyInfoConnector.instance.publicKey))
                {
                    //Decode
                    object jsonObject = MyInfoSecurityHelper.DecodeToken(DecryptedToken);
                    decodedData = jsonObject.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return decodedData;
        }

        
        
    }
}

