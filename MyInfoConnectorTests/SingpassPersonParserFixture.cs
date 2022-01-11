using Common.MyInfo;
using Ir.Common.Data.Singpass;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ir.Common.UnitTest.Common.MyInfo
{
    [TestFixture]
    public class SingpassPersonParserFixture
    {
        public SingpassPerson InterpretShim(byte[] jsonBytes)
        {
            var json = Encoding.UTF8.GetString(jsonBytes);
            return SingpassPerson.Interpret(json);
        }

        
        [TestCase("JENNY LIM WAI FOOK", "Jenny", "Lim Wai Fook", "IR-4162 4 names")]
        [TestCase("TAN AH HONG", "Ah Hong", "Tan", "IR-4162 3 names")]
        [TestCase("DEWANARA VANASAMIN", "Dewanara", "Vanasamin", "IR-4162 2 names")]
        [TestCase("GOD", "God", "", "One name")]
        public void InterpretName(string singleNameIn, string firstNameOut, string lastNameOut, string rationale)
        {
            var info = InterpretShim(MyInfoResource.SampleS3100052A);
            info.RawJson = info.RawJson.Replace("HU DAT FUK", singleNameIn);
           
            info.Interpret();

            Assert.AreEqual(singleNameIn, info.OriginalName, rationale);
            Assert.AreEqual(firstNameOut, info.FirstName, rationale);
            Assert.AreEqual(lastNameOut, info.LastName, rationale);
        }

        [Test]
        public void InterpretNonSingapore()
        {
            var jsonBytes = MyInfoResource.SampleS3100052A;
            var json = Encoding.UTF8.GetString(jsonBytes);

            json = json.Replace("SINGAPORE", "NIGERIA");

            var info = SingpassPerson.Interpret(json);

            Assert.AreEqual("NIGERIA", info.Address.Country);
            Assert.False(info.IsAddressSingapore);
        }

        [Test]
        public void InterpretForeigner()
        {
            var info = InterpretShim(MyInfoResource.SampleF1612345P);

            Assert.True(info.HasForeignerPass, "Should have a pass");
            Assert.AreEqual("Live", info.ForeignerPass.Status);
            Assert.AreEqual(DateTime.Parse("2035-12-31"), info.ForeignerPass.Expiry);
            Assert.AreEqual("Dependent Pass", info.ForeignerPass.Type);

            Assert.AreEqual("No. 1 Xiu Shui Bei Jie, Jian Guo Men Wai", info.Address.Line1);
            Assert.AreEqual(null, info.Address.Line2        , "Expect null Line2    due to no address supplied");
            Assert.AreEqual(null, info.Address.City         , "Expect null City     due to no address supplied");
            Assert.AreEqual(null, info.Address.State        , "Expect null State    due to no address supplied");
            Assert.AreEqual(null, info.Address.Postcode     , "Expect null Postcode due to no address supplied");
            Assert.AreEqual(null, info.Address.Country      , "Expect null Country  due to no address supplied");
        }

        [Test]
        public void InterpretFull()
        {
            var info = InterpretShim(MyInfoResource.SampleS3100052A);
            Console.WriteLine(info.Address);

            Assert.False(info.HasForeignerPass);
            Assert.AreEqual("Hu", info.LastName);
            Assert.AreEqual("Dat Fuk", info.FirstName);
            Assert.AreEqual("S3100052A", info.NricFin);
            Assert.AreEqual(DateTime.Parse("1998-06-06"), info.DateOfBirth);
            Assert.AreEqual("SINGAPORE CITIZEN", info.Nationality);

            /*
Raks said this would format as follows
109 Bedok North Avenue 4
09 - 128 Pearl Garden
460102, Singapore
             */
            Assert.AreEqual("102 BEDOK NORTH AVENUE 4", info.Address.Line1);
            Assert.AreEqual("09 - 128 PEARL GARDEN", info.Address.Line2);
            Assert.AreEqual(null, info.Address.City);
            Assert.AreEqual(null, info.Address.State);
            Assert.AreEqual("460102", info.Address.Postcode);
            Assert.AreEqual("Singapore", info.Address.Country, "Conform casing");

            Assert.True(info.IsAddressSingapore);
        }
    }
}
