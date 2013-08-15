using System.Data;
using Migrator.Framework;
using NUnit.Framework;

namespace Migrator.Tests.Providers
{
    /// <summary>
    /// Base class for Provider tests for all tests including constraint oriented tests.
    /// </summary>
    public abstract class TransformationProviderConstraintBase : TransformationProviderBase
    {
        public void AddForeignKey()
        {
            AddTableWithPrimaryKey();
            Provider.AddForeignKey("FK_Test_TestTwo", "TestTwo", "TestId", "Test", "Id");
        }

        public void AddPrimaryKey()
        {
            AddTable();
            Provider.AddPrimaryKey("PK_Test", "Test", "Id");
        }

        public void AddUniqueConstraint()
        {
            Provider.AddUniqueConstraint("UN_Test_TestTwo", "TestTwo", "TestId");
        }

        public void AddMultipleUniqueConstraint()
        {
            Provider.AddUniqueConstraint("UN_Test_TestTwo", "TestTwo", "Id", "TestId");
        }

        public void AddCheckConstraint()
        {
            Provider.AddCheckConstraint("CK_TestTwo_TestId", "TestTwo", "TestId>5");
        }

        [Test]
        public void CanAddPrimaryKey()
        {
            AddPrimaryKey();
            Assert.IsTrue(Provider.PrimaryKeyExists("Test", "PK_Test"));
        }

        [Test]
        public void AddIndexedColumn()
        {
            Provider.AddColumn("TestTwo", "Test", DbType.String, 50, ColumnProperty.Indexed);
        }

        [Test]
        public void AddUniqueColumn()
        {
            Provider.AddColumn("TestTwo", "Test", DbType.String, 50, ColumnProperty.Unique);
        }

        [Test]
        public void CanAddForeignKey()
        {
            AddForeignKey();
            Assert.IsTrue(Provider.ConstraintExists("TestTwo", "FK_Test_TestTwo"));
        }

        [Test]
        public virtual void CanAddUniqueConstraint()
        {
            AddUniqueConstraint();
            Assert.IsTrue(Provider.ConstraintExists("TestTwo", "UN_Test_TestTwo"));
        }

        [Test]
        public virtual void CanAddMultipleUniqueConstraint()
        {
            AddMultipleUniqueConstraint();
            Assert.IsTrue(Provider.ConstraintExists("TestTwo", "UN_Test_TestTwo"));
        }

        [Test]
        public virtual void CanAddCheckConstraint()
        {
            AddCheckConstraint();
            Assert.IsTrue(Provider.ConstraintExists("TestTwo", "CK_TestTwo_TestId"));
        }

        [Test]
        public void RemoveForeignKey()
        {
            AddForeignKey();
            Provider.RemoveForeignKey("TestTwo", "FK_Test_TestTwo");
            Assert.IsFalse(Provider.ConstraintExists("TestTwo", "FK_Test_TestTwo"));
        }

        [Test]
        public void RemoveUniqueConstraint()
        {
            AddUniqueConstraint();
            Provider.RemoveConstraint("TestTwo", "UN_Test_TestTwo");
            Assert.IsFalse(Provider.ConstraintExists("TestTwo", "UN_Test_TestTwo"));
        }

        [Test]
        public virtual void RemoveCheckConstraint()
        {
            AddCheckConstraint();
            Provider.RemoveConstraint("TestTwo", "CK_TestTwo_TestId");
            Assert.IsFalse(Provider.ConstraintExists("TestTwo", "CK_TestTwo_TestId"));
        }

        [Test]
        public void RemoveUnexistingForeignKey()
        {
            AddForeignKey();
            Provider.RemoveForeignKey("abc", "FK_Test_TestTwo");
            Provider.RemoveForeignKey("abc", "abc");
            Provider.RemoveForeignKey("Test", "abc");
        }

        [Test]
        public void ConstraintExist()
        {
            AddForeignKey();
            Assert.IsTrue(Provider.ConstraintExists("TestTwo", "FK_Test_TestTwo"));
            Assert.IsFalse(Provider.ConstraintExists("abc", "abc"));
        }


        [Test]
        public void AddTableWithCompoundPrimaryKey()
        {
            Provider.AddTable("Test",
                               new Column("PersonId", DbType.Int32, ColumnProperty.PrimaryKey),
                               new Column("AddressId", DbType.Int32, ColumnProperty.PrimaryKey)
                );
            Assert.IsTrue(Provider.TableExists("Test"), "Table doesn't exist");
            Assert.IsTrue(Provider.PrimaryKeyExists("Test", "PK_Test"), "Constraint doesn't exist");
        }

        [Test]
        public void AddTableWithCompoundPrimaryKeyShouldKeepNullForOtherProperties()
        {
            Provider.AddTable("Test",
                               new Column("PersonId", DbType.Int32, ColumnProperty.PrimaryKey),
                               new Column("AddressId", DbType.Int32, ColumnProperty.PrimaryKey),
                               new Column("Name", DbType.String, 30, ColumnProperty.Null)
                );
            Assert.IsTrue(Provider.TableExists("Test"), "Table doesn't exist");
            Assert.IsTrue(Provider.PrimaryKeyExists("Test", "PK_Test"), "Constraint doesn't exist");

        	Column column = Provider.GetColumnByName("Test", "Name");
            Assert.IsNotNull(column);
            Assert.IsTrue((column.ColumnProperty & ColumnProperty.Null) == ColumnProperty.Null);
        }
    }
}
