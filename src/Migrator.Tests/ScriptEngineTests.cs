using System.IO;
using System.Reflection;
using Migrator.Compile;
using NUnit.Framework;

namespace Migrator.Tests
{
    [TestFixture]
    public class ScriptEngineTests
    {
        [Test]
        public void CanCompileAssemblies() 
        {
            ScriptEngine engine = new ScriptEngine();

            // This should let it work on windows or mono/unix I hope
            string dataPath = "Data";
        	var directory = new DirectoryInfo(dataPath);
			Assert.That(directory.Exists, Is.True, directory.FullName);

            Assembly asm = engine.Compile(dataPath);
            Assert.IsNotNull(asm);

            MigrationLoader loader = new MigrationLoader(null, asm, false);
            Assert.AreEqual(2, loader.LastVersion);

            Assert.AreEqual(2, MigrationLoader.GetMigrationTypes(asm).Count);
        }
    }
}