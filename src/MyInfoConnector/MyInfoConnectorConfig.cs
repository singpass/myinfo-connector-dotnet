﻿using System;
using System.Security.Cryptography;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Specialized;

namespace sg.gov.ndi.MyInfoConnector
{
    public class MyInfoConnectorConfig
    {
        private X509Certificate2 _x509private;
        private X509Certificate2 _x509public;

        /// <summary>
        /// Path to Private certificate identifying the client
        /// </summary>
        /// <remarks>
        /// Should be PFX format
        /// </remarks>
        public string PrivateCertificateFilename    {get;set;}


        /// <summary>
        /// Password to private certificate
        /// </summary>
        public string PrivateCertificatePassword    {get;set;}

        /// <summary>
        /// Path to Public certificate of consent.myinfo.gov.sg
        /// </summary>
        /// <remarks>
        /// Should be binary (DER) or Base64 encoded
        /// </remarks>
        public string PublicCertificateFilename     {get;set;}

        public string ClientAppId                   {get;set;}

        public string ClientAppPassword             {get;set;}

        public string AttributeCsv                  {get;set;}

        public string Environment                   {get;set;}

        public string AuthoriseUrl                  {get;set;}

        public string TokenUrl                      {get;set;}

        public string PersonUrl                     {get;set;}

        public string Purpose                       {get;set;}

        public bool IsSandbox => Environment == ApplicationConstant.SANDBOX;

        private MyInfoConnectorConfig()
        {

        }

        internal RSACryptoServiceProvider GetPrivateKey()
        {
            if (_x509private == null)
            {
                var filePath = ResolvePath(PrivateCertificateFilename);
                _x509private = new X509Certificate2(filePath, PrivateCertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            }
            var privateKey = _x509private.PrivateKey as RSACryptoServiceProvider;
            return privateKey;
        }


        internal RSA GetPublicKey()
        {
            if (_x509public == null)
            {
                var filePath = ResolvePath(PublicCertificateFilename);
                _x509public = new X509Certificate2(filePath);
            }
            var publicKey = _x509public.GetRSAPublicKey();
            return publicKey;
        }


        private static string ResolvePath(string filename)
        {
            var filePath = filename;
            if (!File.Exists(filename))
            {
                // It must be a relative path from bin folder
                var appFolder = GetApplicationFolder();
                filePath = Path.Combine(appFolder, filename);
            }
            return filePath;
        }

        private static string GetApplicationFolder()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            var assemblyPath = thisAssembly.Location;

            var folder = Path.GetDirectoryName(assemblyPath);

            if (folder.ToLower().Contains("windows"))
            {
                // Handle asp.net running from a temp folder in C:\Windows
                return AppDomain.CurrentDomain.BaseDirectory + "bin";
            }

            return folder;
        }

        /// <summary>
        /// Sanity check the settings but don't throw
        /// Allow the caller to react appropriately
        /// </summary>
        public (bool isValid, string[] messages) IsValid()
        {
            var messages = new List<string>();

            if (string.IsNullOrEmpty(PrivateCertificateFilename))   messages.Add($"PrivateCertificateFilename missing or empty");
            if (string.IsNullOrEmpty(PrivateCertificatePassword))   messages.Add($"PrivateCertificatePassword missing or empty");
            if (string.IsNullOrEmpty(PublicCertificateFilename))    messages.Add($"PublicCertificateFilename missing or empty");
            if (string.IsNullOrEmpty(ClientAppId))                  messages.Add($"ClientAppId missing or empty");
            if (string.IsNullOrEmpty(ClientAppPassword))            messages.Add($"ClientAppPassword missing or empty");      
            if (string.IsNullOrEmpty(AttributeCsv))                 messages.Add($"AttributeCsv missing or empty");
            if (string.IsNullOrEmpty(Environment))                  messages.Add($"Environment missing or empty");
            if (string.IsNullOrEmpty(TokenUrl))                     messages.Add($"TokenUrl missing or empty");
            if (string.IsNullOrEmpty(PersonUrl))                    messages.Add($"PersonUrl missing or empty");


            if (!string.IsNullOrEmpty(PrivateCertificateFilename))
            {
                try
                {
                    GetPrivateKey();
                }
                catch (Exception e)
                {
                    messages.Add("PrivateCertificateFilename failed to load - " + e.Message);
                }
            }

            if (!string.IsNullOrEmpty(PublicCertificateFilename))
            {
                try
                {
                    GetPublicKey();
                }
                catch (Exception e)
                {
                    messages.Add("PublicCertificateFilename failed to load - " + e.Message);
                }
            }

            return (messages.Count == 0, messages.ToArray());
        }

        public string[] GetDiagnosticInfo()
        {
            GetPrivateKey();
            GetPublicKey();

            var messages = new List<string>();

            messages.Add($"Our private certificate SerialNumber={_x509private.SerialNumber}, Thumbprint={_x509private.Thumbprint}");
            messages.Add($"MyInfo public certificate SerialNumber={_x509public.SerialNumber}, Thumbprint={_x509public.Thumbprint}");
            messages.Add($"ClientAppId={ClientAppId}");
            messages.Add($"AuthoriseUrl={AuthoriseUrl}");
            messages.Add($"TokenUrl={TokenUrl}");
            messages.Add($"PersonUrl={PersonUrl}");

            return messages.ToArray();
        }

        public static MyInfoConnectorConfig Load(AppSettingsSection section, string keyPrefix = null)
        {
            Func<string, string> getSetting = (string keySuffix) =>
            {
                return section.Settings[$"{keyPrefix}{keySuffix}"].Value;
            };

            return Load(getSetting);
        }

        public static MyInfoConnectorConfig Load(NameValueCollection appSettings, string keyPrefix = null)
        {
            Func<string, string> getSetting = (string keySuffix) =>
            {
                return appSettings[$"{keyPrefix}{keySuffix}"];
            };

            return Load(getSetting);
        }

        public static MyInfoConnectorConfig Load(Func<string, string> getSetting)
        {
            var output = new MyInfoConnectorConfig();

            output.PrivateCertificateFilename = getSetting("PrivateCertificateFilename");
            output.PrivateCertificatePassword = getSetting("PrivateCertificatePassword");
            output.PublicCertificateFilename = getSetting("PublicCertificateFilename");

            output.ClientAppId = getSetting("ClientAppId");
            output.ClientAppPassword = getSetting("ClientAppPassword");
            output.AttributeCsv = getSetting("AttributeCsv");
            output.Environment = getSetting("Environment");
            output.TokenUrl = getSetting("TokenUrl");
            output.PersonUrl = getSetting("PersonUrl");
            output.AuthoriseUrl = getSetting("AuthoriseUrl");
            output.Purpose = getSetting("Purpose");

            return output;
        }
    }
}
