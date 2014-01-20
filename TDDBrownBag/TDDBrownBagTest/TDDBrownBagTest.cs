using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using NUnit.Framework;

namespace TDDBrownBagTest
{
    [TestFixture]
    public class TDDBrownBagTest
    {
        TextWriter _normalOutput;
        StringWriter _testingConsole;
        StringBuilder _testingBuffer;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Environment.CurrentDirectory = "C:/_github/TTDBrownBag/TDDBrownBag/TDDBrownBag/bin/Debug/";

            _testingBuffer = new StringBuilder();
            _testingConsole = new StringWriter(_testingBuffer);
            _normalOutput = System.Console.Out;
            System.Console.SetOut(_testingConsole);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            System.Console.SetOut(_normalOutput);
        }

        [SetUp]
        public void SetUp()
        {
            _testingBuffer.Remove(0, _testingBuffer.Length);
        }

        [TearDown]
        public void TearDown()
        {
            _normalOutput.Write(_testingBuffer.ToString());
        }

        private static int StartConsoleApplication()
        {
            var proc = new Process
                           {
                               StartInfo =
                                   {
                                       FileName = "TDDBrownBag.exe",
                                       Arguments = "",
                                       UseShellExecute = false,
                                       RedirectStandardOutput = true
                                   }
                           };
            proc.Start();
            proc.WaitForExit();
            System.Console.Write(proc.StandardOutput.ReadToEnd());
            return proc.ExitCode;
        }

        [Test]
        public void TTDBrownBag()
        {
            // Check the TTDBrownBag report writer did not throw an error
            Assert.AreEqual(0, StartConsoleApplication());

            // Check We have the beginning of the header record
            Assert.IsTrue(_testingBuffer.ToString().Contains("AccountID,"));
        }
    }
}
