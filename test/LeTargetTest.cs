using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Le;
using NLog;
using NLog.Config;
using NLog.Layouts;
using System.Configuration;

/**
* Tests the Logentries NLog Plugin
*
* @author Mark Lacomber
*
*/

namespace NLog_Unit
{
    /// <summary>
    /// Summary description for LeTargetTest
    /// </summary>
    [TestClass]
    public class LeTargetTest
    {
        /**General Le target ready for tweaking */
        LeTarget x;
        LoggingConfiguration config;

        /**Some random key */
        readonly static String k0 = Guid.NewGuid().ToString();
        /**Some random key */
        readonly static String k1 = Guid.NewGuid().ToString();
        /**Some random location */
        readonly static String l0 = "location0";
        /**Some random location */
        readonly static String l1 = "location1";

        public LeTargetTest()
        {
            x = new LeTarget();
            config = new LoggingConfiguration();
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
        public void testLeTargetBoolean()
        {
            LeTarget l = new LeTarget();
            Assert.IsFalse(l.Debug);
            Assert.IsFalse(l.Ssl);

            l.TestClose();
        }

        [TestMethod]
        public void testSetKey()
        {
            LeTarget l = new LeTarget();

            l.Key = k0;
            Assert.AreEqual(k0, l.Key);

            l.Key = k1;
            Assert.AreEqual(k1, l.Key);

            l.TestClose();
        }

        [TestMethod]
        public void testSetLocation()
        {
            LeTarget l = new LeTarget();
            l.Location = l0;
            Assert.AreEqual(l0, l.Location);

            l.Location = l1;
            Assert.AreEqual(l1, l.Location);

            l.TestClose();
        }

        [TestMethod]
        public void testSetDebug()
        {
            LeTarget l = new LeTarget();
            l.Debug = true;
            Assert.IsTrue(l.Debug);

            l.Debug = false;
            Assert.IsFalse(l.Debug);

            l.TestClose();
        }

        [TestMethod]
        public void testSetSsl()
        {
            LeTarget l = new LeTarget();
            l.Ssl = true;
            Assert.IsTrue(l.Ssl);

            l.Ssl = false;
            Assert.IsFalse(l.Ssl);

            l.TestClose();
        }

        [TestMethod]
        public void testCheckCredentials()
        {
            LeTarget l = new LeTarget();
            var appSettings = ConfigurationManager.AppSettings;
            Assert.IsFalse(l.checkCredentials());

            appSettings["LOGENTRIES_ACCOUNT_KEY"] = "";
            Assert.IsFalse(l.checkCredentials());

            appSettings["LOGENTRIES_LOCATION"] = "";
            Assert.IsFalse(l.checkCredentials());

            appSettings["LOGENTRIES_ACCOUNT_KEY"] = k0;
            Assert.IsFalse(l.checkCredentials());

            appSettings["LOGENTRIES_LOCATION"] = l0;
            Assert.IsTrue(l.checkCredentials());

            //Reset appSettings for following tests
            appSettings["LOGENTRIES_ACCOUNT_KEY"] = "";
            appSettings["LOGENTRIES_LOCATION"] = "";

            l.TestClose();
        }

        [TestMethod]
        public void testAppendLine()
        {
            LeTarget l = new LeTarget();

            String line0 = "line0";
            l.addLine(line0);
            Assert.AreEqual(1, l.queue.Count);

            for (int i = 0; i < LeTarget.QUEUE_SIZE; ++i)
            {
                l.addLine("line" + i);
            }
            Assert.AreEqual(LeTarget.QUEUE_SIZE, l.queue.Count);

            l.TestClose();
        }

        [TestMethod]
        public void testAppendLoggingEvent()
        {
            LeTarget l = new LeTarget();
            l.Layout = new SimpleLayout();

            LogEventInfo logEvent = new LogEventInfo();
            logEvent.Message = "Critical";
            logEvent.LoggerName = "root";
            logEvent.Level = LogLevel.Debug;
            logEvent.TimeStamp = DateTime.Now;
            logEvent.Exception = new Exception();

            l.TestWrite(logEvent);
            Assert.AreEqual(0, l.queue.Count);

            var appSettings = ConfigurationManager.AppSettings;
            appSettings["LOGENTRIES_ACCOUNT_KEY"] = k0;
            appSettings["LOGENTRIES_LOCATION"] = l0;
            l.TestWrite(logEvent);

            //Message and Exception count as 2 events
            Assert.AreEqual(2, l.queue.Count);

            l.TestClose();
        }

        public void testCloseTarget()
        {
            LeTarget l = new LeTarget();
            var appSettings = ConfigurationManager.AppSettings;
            appSettings["LOGENTRIES_ACCOUNT_KEY"] = k0;
            appSettings["LOGENTRIES_LOCATION"] = l0;
            l.thread.Start();
            
            //Wait until thread is active
            for (int i = 0; i < 10; ++i)
            {
                Thread.Sleep(100);
                if (l.thread.IsAlive == true)
                    break;
            }
            Assert.IsTrue(l.thread.IsAlive);

            l.TestClose();

            //Wait until the thread is not active
            for (int i = 0; i < 10; ++i)
            {
                Thread.Sleep(100);
                if (!l.thread.IsAlive)
                    break;
            }
            Assert.IsFalse(l.thread.IsAlive);
        }
    }
}
