using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Morix
{
    [TestClass]
    public class Tests
    {
        [TestInitialize]
        public void Init()
        {
            //Morix.Log.Dir = "Logss";
            Morix.Log.NoDays = 1;
        }

        [TestMethod]
        public void Write()
        {
            try
            {
                Morix.Log.Debug("Hello debug");

                Morix.Log.Error(new Exception("Hello exception"));

                Morix.Log.Error(new Exception("Hello exception"), "exception with note");

                Morix.Log.Custom("Write", "Custom log");
                Morix.Log.Custom("Write", "Custom log same line", true);

                Morix.Log.WriteCallingMethod();

                string line = "This is line for testing " + DateTime.Now.ToString();
                var sb = new System.Text.StringBuilder(line);
                for (int i = 0; i < 5000; i++)
                {
                    Morix.Log.Custom("Big", sb.ToString());
                    sb.AppendLine(line);
                }
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex == null, ex.ToString());
            }
        }
    }
}
