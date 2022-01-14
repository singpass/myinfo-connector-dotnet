using Newtonsoft.Json;
using NLog;
using sg.gov.ndi.MyInfoConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyInfoConnectorConsole
{
    internal class MyInfoTester
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public void Run()
        {
            var config = GetConfig();

            var isValidConfig = config.IsValid();
            if (!isValidConfig.isValid)
            {
                Log.Warn("Config warnings:\r\n" + string.Join("\r\n", isValidConfig.messages));
            }

            var connector = MyInfoConnector.Create(config);

            var redirectUri = GetRedirectUri();
            var state = "a-fixed-state-value";

            Log.Info($"Generating an authorise URL that will redirect back to {redirectUri}");

            var authoriseUrl = connector.GetAuthoriseUrl(redirectUri, state);

            Log.Info($"Opening URL to\r\n\r\n{authoriseUrl}\r\n");

            OpenBrowser(authoriseUrl);

            Log.Info($"After authorising you will get redirected to a localhost:3001 url with a code in the query string");
            Log.Info($"Paste in the query string now");

            var code = Console.ReadLine();

            Log.Info($"We read code='{code}'. Now getting the person json");

            var personJson = connector.GetPersonJson(redirectUri, code);
            var formattedJson = FormatJson(personJson);

            Log.Info($"Person json is\r\n{formattedJson}");
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        private MyInfoConnectorConfig GetConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;

            // Allows us to programmatically switch to a different set
            Func<string, string> getSetting = key => appSettings["Sandbox" + key];

            var config = MyInfoConnectorConfig.Load(getSetting);
            return config;
        }

        /// <summary>
        /// It is convenient to be able to change this to different routes on the fly depending on client conditions
        /// </summary>
        private string GetRedirectUri() => "http://localhost:3001/callback";

        private string GetState() => nameof(MyInfoTester) + DateTime.Now.Ticks.ToString();

        /// <summary>
        /// https://stackoverflow.com/a/57783751/1141876
        /// </summary>
        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
