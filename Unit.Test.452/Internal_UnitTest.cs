using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeanCloud;
using LeanMessage.Internal;
using System.Diagnostics;

namespace Unit.Test._452
{
    /// <summary>
    /// Summary description for Internal_UnitTest
    /// </summary>
    [TestClass]
    public class Internal_UnitTest
    {
        public Internal_UnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SerializeJsonString()
        {
            //
            // TODO: Add test logic here
            //
            IDictionary<string, object> dic = new Dictionary<string, object>();
            dic["m"] = new List<string>() { "Tom", "Jerry" };

            AVClient.SerializeJsonString(dic);
        }

        [TestMethod]
        public void Grab_Dictionary()
        {
            IDictionary<string, object> dic = new Dictionary<string, object>()
            {
            };

            dic.Add("a", 1);
            dic.Add("b", new Dictionary<string, object>() { { "c", 2 } });

            Trace.WriteLine(dic.Grab("b.c"));

        }
    }
}
