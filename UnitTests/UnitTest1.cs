using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simple_MT940_Checker;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        Random RndGen = new Random();


        [TestMethod]
        public void Test_IBAN_Check()
        {
            Assert.IsTrue(Model.IsValid_IBAN("GB82 WEST 1234 5698 7654 32"));
            Assert.IsTrue(Model.IsValid_IBAN("NL11 RABO 0110 0992 22"));

            Assert.ThrowsException<Model.Validation_Exception>(() => Model.IsValid_IBAN("NL11 RABO 0120 0992 22"));
            Assert.ThrowsException<Model.Validation_Exception>(() => Model.IsValid_IBAN("NL11 RABO 0210 0992 22"));
            Assert.ThrowsException<Model.Validation_Exception>(() => Model.IsValid_IBAN("NL13 RABO 0120 0992 22"));
            Assert.ThrowsException<Model.Validation_Exception>(() => Model.IsValid_IBAN("NL13 RABO 0120 0992 25"));
            Assert.ThrowsException<Model.Validation_Exception>(() => Model.IsValid_IBAN("NL13 RABO 0120 0902 22"));
        }


        [TestMethod]
        public void Test_BigModulo()
        {

            for (int i = 0; i < 1000; i++) {

                var baseInt = RndGen.Next(100, 1000000000);
                var factor = RndGen.Next(100, 1000000);
                var remainder = RndGen.Next(0, 100);

                var total = ((UInt64)baseInt * (UInt64)factor + (UInt64)remainder);

                Assert.AreEqual(Model.BigModulo(total.ToString(), factor), 
                                remainder);
            }
        }
    }
}
