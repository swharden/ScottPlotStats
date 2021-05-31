using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NuGetPPT.Tests
{
    public static class SampleData
    {
        public static string ReadSampleData(string filename)
        {
            string filePath = TestContext.CurrentContext.TestDirectory + "../../../../../../dev/data/" + filename;
            filePath = Path.GetFullPath(filePath);
            if (File.Exists(filePath) == false)
                throw new InvalidOperationException($"file does not exist: {filePath}");
            return File.ReadAllText(filePath);
        }
    }
}
