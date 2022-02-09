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
            Morix.Log.Dir = "Logs";
            Morix.Log.NoDays = 1;
        }

        [TestMethod]
        public void Write()
        {
            try
            {
                Morix.Log.Debug("Hello debug 1");

                Morix.Log.Error(new Exception("Hello exception 1"));

                Morix.Log.Error(new Exception("Hello exception 1"), "exception 1 with note");

                Morix.Log.Custom("Write", "Custom log 1");
                Morix.Log.Custom("Write", "Custom log same line 1", true);

                string line = "This is line for testing " + DateTime.Now.ToString();
                var sb = new System.Text.StringBuilder(line);
                for (int i = 0; i < 3000; i++)
                {
                    Morix.Log.Custom("Big", sb.ToString());
                    sb.AppendLine(line);
                }


                Morix.Log.Debug("Hello debug 2");

                Morix.Log.Error(new Exception("Hello exception 2"));

                Morix.Log.Error(new Exception("Hello exception 2"), "exception 2 with note");

                Morix.Log.Custom("Write", "Custom log 2");
                Morix.Log.Custom("Write", "Custom log same line 2", true);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex == null, ex.ToString());
            }
        }
    }
}
