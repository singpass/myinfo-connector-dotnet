using NUnit.Framework;
using sg.gov.ndi.MyInfoConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ir.Common.UnitTest.Common.MyInfo
{
    [TestFixture]
    public class MyInfoConnectorConfigFixture
    {
        public static MyInfoConnectorConfig GetFromAppConfig()
        {
            var config = MyInfoConnectorConfig.Load(ConfigurationManager.AppSettings, "MyInfo");
            return config;
        }

        public MyInfoConnectorConfig GetSystemUnderTest()
        {
            Func<string, string> settingLoader = (string key) =>
             {
                 return $"valueFor{key}";
             };

            var config = MyInfoConnectorConfig.Load(settingLoader);
            return config;
        }

        [Test]
        public void DecodeTokenToPerson()
        {
            var config = GetFromAppConfig();
            var connector = MyInfoConnector.Create(config);

            var data = connector.DecodeTokenToPerson("eyJhbGciOiJSU0EtT0FFUCIsImVuYyI6IkEyNTZHQ00iLCJraWQiOiJkZXYud2Vic2l0ZS5pbmRlcGVuZGVudHJlc2VydmUubmV0In0.YX8fL1-9bPC2jSf2C-VZOgURh-A4vxDohxUIK8ZapMQ1UbNqf6mPrtnsIARPAX9q8PkH77tKwUtzr9T1sFkFk6Hw-qr3gRJIAkzuBVGrtPmjukHCDINDLlaJ_R3qtWKuUl-S1iSJO5nItDeUKr9M2HwBO3hhervY0jVeu3dFD3LpUE9yRiBmn7ja4Hy08offSs7blCHKL3qJssU9oa-PApGHp-eRhMmfkEMf1Sl8oXXXpVdSHH3ghwZ-g3X7-Zckj__KPJJO2ZRy5CJ6hlp4SeFuz0BUD1oiInUIUaQ83KYuGKTLW09ThJcg8790vNaifBNKC_GuGKMQ7CCtRn_WYA.2zRyUjiNkBnTlPoX.MBVgcW5dTE8ZP7XoIk1N-wdVOpy_yB10yfqpxUZwdaaObH7XUpR7ccEorTCKujsa3__0MoI7GQB56-g9Zc5XHSlKI8BvOfsNck8d1_g_kCNihYlhn4Z1vxqJMmK8LuZWONxT1HHdm1rwQqAAjSLht5cCHPLtstBYzVpRDDobLYnU0zmeKlVCUjaUTqH2-_ZZHDARhRhaeBiSBkDtvN9OKjU7waMC3CvyKix1RVw4wi1eH2fhD6q_aAgYXA3DYQP7S4v9dbUdOdqMe46weFC3gQ78mJu-KdWig8ZcqFXZE01FKUtENab8Fri1uSad1RTGrleJVBIlBxr8yk8E8UhL2Na0tAkpWLe8Ps6-ig20bOcn5be4wckqbOW1nLefJ-SkcaQW9Z-d5xWJnm1_EJbnHhl1kqF_M6_ThYtGn2IBYACmFrGgjjTD4CkDYGD6bmllrrronmsN1YpdCbvEigyJ1bhj2z80hne0tZGTEqGJ9AM_RsPEYBAIh94doeCTnu82u9pHlFIwE9O1hK2S41AA-VhcOL9HzVHaMwhKu9qH8kZroTZIeaiex7m9RIYFGUkcmJVu8CRZ0YvQhJuU9vbK6z5kkV7k6vENkdeEX3mBXG6fdquO3yR36SmM2UNsnBZ5gohCglKKMurnc2FIAunrl6yBG4Zmb1JmcKhpuV-uoyBZ3D_-1RMKi7r9YCRJc16Ab3Xulqql4ZufEY1dNQcXf-Sns6z9P3HzBttrY9-PCtY3nOr6Ir5lsCa5meg6S4P6DEAIpctFaLtrk1GSyAsit-w4spsfSZn7yLgxFDWNnk06TWXpLXId06ZVEOvd5cNhl6LOjyitJXTCwA1Y4DlDu-0V6MGUUxml6laVp_Q2gxg-SY9TrRd1mVbtgHdlTmkpKx0_auEKU5WqwyPTyvH4uS78BT6qTeK3N10x8mR-qt1oC3b0g2wF5leDEdZCFaKzz9G45PUILyYLu7vDZlmE8mrkotOK7I8gWZ3y43kBS5gtmsv3p14goz_ajXYhZ2SzSbm6fLgwP7NoR_rR7DrbzlKl7vaZaIck8hihFN_Q0K5tLbw5pXNU9sRI742yJP_fIoWDadGrBdzvhJyjhzi0lEQBMIle-4hwNCvAdp6YoVE0AJ31kEBjczefMZrw1PvmGgbrCjjozlFPXDWRYdtpueVQnu_In6xeGX_W8YrwtivEn8asifC5-UAz_5bY2skOP0YYiM09yeoZIhQQ9agsv2SDyx-lkLOlAAAb9eGX_defT9WOLo3gvTHrAPd8j7WUkFqE8YNS4iQnbnvBuqKnwF2S_O3m5xOi-X2NHjLFABv3Ukykhs9i7oCJ2pAzjVkBwvuXD92j861UW4jj5BPFJTbSgAPj6EkHrYqsS8KUbrGn2HaRUfjvyQU67il6pEioTu1X6vI6JWj50Xqw1dQ7HOUyxW0USxM-G1F6JV3lCGCzMnmLofaZfm7GblGok78QskbHP-faQ4Kv8J_vXGtGJFsBDVeBbyGCuRJy_yTn-5Rngg7rAMtfHnpNWdvq6vrlMqRIg0RB08p1FRLY0npS7ji9xrpVey4ivAxpy1uwxtlzPEQy-eZRkMTNrbySRRlpNj6FVWeM2I_5V4FwC4YKouXatDbgUSPYxMCPz0vAIWdhmWKqMEKU5egwwG7S1zc7E_DPQVx_DDrERiQeHIKB9l1g6mSggoPhDgQb4XtJTsvFe__WUgNpSNThgs01icrpyxOyps9puZgAxxx-HwxxQ6VOCjHR-uWwHuUSVSdm9UDl6mRe_SefoWiyVBprIR7FV5f8sn8Id9D0jmvyTWhw9uHlq70gVV3Q2nqPXfLrs5Lv6Vhm5sz7hMl9Tvr8I0SURIAd20yODueWwuNuDmQjuDC7sZcs7rM31K0HeNwf5OQlh-T0lZRlehDK2ZaD4t-HAl49tSBflrOTis-XyW59sltzWD5on4fhLH8rIET5D7QwQ1CwG7W1QICS7dqt3q288VGt4hPkW4TMyCT5CHhUf1OhxZdGtJ55M7Mx2agN1MDbgI2DwTLypM7j0seOUjT3vwgId2wma9m16yKZKilw9xIuhCX23lwP_svIjikizVkcPAbF4hH2capVeV4qLJDBNTexgkyjiU0ygog33lzLG1Fd1dFOAy2qG3hvkWcAvF5e1rKMRvmmbtOU46sNSUIDr8hBbwth2vNtwNUK5twJBAdlVF0_ZvQg5R1QFkAWMibbcCBqF6xj28WXIbqm8Fr_jTAJ3c5ZDWDrrOGxAhvMTD2qBp3zRlWXh5kJ0CAq4wfBlvkgdYnZ1rIbFFZHBKc.TEOV_0dE8FtFNX8HtpRbRQ");

            Assert.IsNotNull(data);
        }

        [Category("Integration")]
        [Test]
        public void LoadPrivateCertificate()
        {
            var config = GetSystemUnderTest();
            config.PrivateCertificateFilename = @"C:\Downloads\myinfo-dev\dev.website.independentreserve.pfx";
            config.PrivateCertificatePassword = "ThisIsSecret";

            Assert.DoesNotThrow(() => config.GetPrivateKey());
        }

        [Category("Integration")]
        [Test]
        public void LoadPublicCertificate()
        {
            var config = GetSystemUnderTest();
            config.PublicCertificateFilename = @"C:\Downloads\myinfo-dev\test.der";

            Assert.DoesNotThrow(() => config.GetPublicKey());
        }
    }
}
