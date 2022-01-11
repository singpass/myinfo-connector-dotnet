using NUnit.Framework;
using sg.gov.ndi.MyInfoConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ir.Common.UnitTest.Common.MyInfo
{
    [TestFixture]
    public class MyInfoSecurityHelperFixture
    {
        [Test]
        public void GetRandomString()
        {
            var randomString = MyInfoSecurityHelper.GetRandomInteger();

            // Test will fail if not an int
            Assert.DoesNotThrow(()=> Convert.ToInt32(randomString));
        }



        [Test]
        public void VerifyToken()
        {
            var config = MyInfoConnectorConfigFixture.GetFromAppConfig();
            var publicKeyXml = config.GetPublicKey();

            var result = MyInfoSecurityHelper.VerifyToken("\"eyJhbGciOiJSUzI1NiIsImtpZCI6Il9SQzZ4d09NdmJ0dDZhald1WmU2R2xncy1qM3dtNXJpQXlDVW9SYXNhLUkifQ.eyJ1aW5maW4iOnsibGFzdHVwZGF0ZWQiOiIyMDIxLTExLTEwIiwic291cmNlIjoiMSIsImNsYXNzaWZpY2F0aW9uIjoiQyIsInZhbHVlIjoiUzYwMDUwNDhBIn0sIm5hbWUiOnsibGFzdHVwZGF0ZWQiOiIyMDIxLTExLTEwIiwic291cmNlIjoiMSIsImNsYXNzaWZpY2F0aW9uIjoiQyIsInZhbHVlIjoiQU5EWSBMQVUifSwiZG9iIjp7Imxhc3R1cGRhdGVkIjoiMjAyMS0xMS0xMCIsInNvdXJjZSI6IjEiLCJjbGFzc2lmaWNhdGlvbiI6IkMiLCJ2YWx1ZSI6IjE5ODgtMTAtMDYifSwibmF0aW9uYWxpdHkiOnsibGFzdHVwZGF0ZWQiOiIyMDIxLTExLTEwIiwiY29kZSI6IlNHIiwic291cmNlIjoiMSIsImNsYXNzaWZpY2F0aW9uIjoiQyIsImRlc2MiOiJTSU5HQVBPUkUgQ0lUSVpFTiJ9LCJwYXNzdHlwZSI6eyJsYXN0dXBkYXRlZCI6IjIwMjEtMTEtMTAiLCJjb2RlIjoiIiwic291cmNlIjoiMyIsImNsYXNzaWZpY2F0aW9uIjoiQyIsImRlc2MiOiIifSwicGFzc3N0YXR1cyI6eyJsYXN0dXBkYXRlZCI6IjIwMjEtMTEtMTAiLCJzb3VyY2UiOiIzIiwiY2xhc3NpZmljYXRpb24iOiJDIiwidmFsdWUiOiIifSwicGFzc2V4cGlyeWRhdGUiOnsibGFzdHVwZGF0ZWQiOiIyMDIxLTExLTEwIiwic291cmNlIjoiMyIsImNsYXNzaWZpY2F0aW9uIjoiQyIsInZhbHVlIjoiIn0sInJlZ2FkZCI6eyJjb3VudHJ5Ijp7ImNvZGUiOiJTRyIsImRlc2MiOiJTSU5HQVBPUkUifSwidW5pdCI6eyJ2YWx1ZSI6IjEwIn0sInN0cmVldCI6eyJ2YWx1ZSI6IkFOQ0hPUlZBTEUgRFJJVkUifSwibGFzdHVwZGF0ZWQiOiIyMDIxLTExLTEwIiwiYmxvY2siOnsidmFsdWUiOiIzMTkifSwic291cmNlIjoiMSIsInBvc3RhbCI6eyJ2YWx1ZSI6IjU0MjMxOSJ9LCJjbGFzc2lmaWNhdGlvbiI6IkMiLCJmbG9vciI6eyJ2YWx1ZSI6IjM4In0sInR5cGUiOiJTRyIsImJ1aWxkaW5nIjp7InZhbHVlIjoiIn19fQ.iy3srYWZBIZCFISnFPHtsrXXHc7-W-d14B8vqBafqY5Oz5MnbcdWSSR6FnN0TxRS1b_mCFbXP6PjzOaaVuNkFOqRkPJ_6R1dmhQBlYizeh4onqq7uETZ2KobZArBFDIvp3cusoCn1b4dNN9ortkYlXgsdmAOx7awbwLyjL14xf5LqXmZqhhIdPsInfd4yINKYDtFZjv2KP-Gu7yKjVNeXaFHvxp3N375luu0xg-gBuJJ_yj5VcxpC4xnV_c4WkVMn4jSR5odD-zRFgVPBQA60O5Qy6evXKEYRA5CUoUJoew_U8eWYOGfDfT2rP-LfuLT141XIQdHZkRa5O7L-UImYw\"", publicKeyXml);
            
            Assert.True(result);
        }
    }
}
