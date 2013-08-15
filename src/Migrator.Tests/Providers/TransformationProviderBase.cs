using System;
using System.Data;
using Migrator.Framework;
using Migrator.Framework.Loggers;
using NUnit.Framework;

namespace Migrator.Tests.Providers
{
	/// <summary>
	/// Base class for Provider tests for all non-constraint oriented tests.
	/// </summary>
	public abstract class TransformationProviderBase
	{
		private ITransformationProvider _provider;

		protected ITransformationProvider Provider
		{
			get { return _provider; }
			set
			{
				_provider = value;
				_provider.Logger = Logger.ConsoleLogger();
			}
		}

		[TearDown]
		public virtual void TearDown()
		{
			DropTestTables();

			Provider.Rollback();
		}

		protected void DropTestTables()
		{
			// Because MySql doesn't support schema transaction
			// we got to remove the tables manually... sad...
			Provider.RemoveTable("TestTwo");
			Provider.RemoveTable("Test");
			Provider.RemoveTable("SchemaInfo");
		}

		public void AddDefaultTable()
		{
			Provider.AddTable("TestTwo",
			                   new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
			                   new Column("TestId", DbType.Int32, ColumnProperty.ForeignKey)
				);
		}

		public void AddTable()
		{
			Provider.AddTable("Test",
			                   new Column("Id", DbType.Int32, ColumnProperty.NotNull),
			                   new Column("Title", DbType.String, 100, ColumnProperty.Null),
			                   new Column("name", DbType.String, 50, ColumnProperty.Null),
			                   new Column("blobVal", DbType.Binary, ColumnProperty.Null),
			                   new Column("boolVal", DbType.Boolean, ColumnProperty.Null),
			                   new Column("bigstring", DbType.String, 50000, ColumnProperty.Null)
				);
		}

		public void AddTableWithPrimaryKey()
		{
			Provider.AddTable("Test",
			                   new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
			                   new Column("Title", DbType.String, 100, ColumnProperty.Null),
			                   new Column("name", DbType.String, 50, ColumnProperty.NotNull),
			                   new Column("blobVal", DbType.Binary),
			                   new Column("boolVal", DbType.Boolean),
			                   new Column("bigstring", DbType.String, 50000)
				);
		}

		[Test]
		public void TableExistsWorks()
		{
			Assert.IsFalse(Provider.TableExists("gadadadadseeqwe"));
			Assert.IsTrue(Provider.TableExists("TestTwo"));
		}

		[Test]
		public void ColumnExistsWorks()
		{
			Assert.IsFalse(Provider.ColumnExists("gadadadadseeqwe", "eqweqeq"));
			Assert.IsFalse(Provider.ColumnExists("TestTwo", "eqweqeq"));
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "Id"));
		}

		[Test]
		public void CanExecuteBadSqlForNonCurrentProvider()
		{
			Provider["foo"].ExecuteNonQuery("select foo from bar 123");
		}

		[Test]
		public void TableCanBeAdded()
		{
			AddTable();
			Assert.IsTrue(Provider.TableExists("Test"));
		}

		[Test]
		public void GetTablesWorks()
		{
			foreach (string name in Provider.GetTables())
			{
				Provider.Logger.Log("Table: {0}", name);
			}
			Assert.AreEqual(1, Provider.GetTables().Length);
			AddTable();
			Assert.AreEqual(2, Provider.GetTables().Length);
		}

		[Test]
		public void GetColumnsReturnsProperCount()
		{
			AddTable();
			Column[] cols = Provider.GetColumns("Test");
			Assert.IsNotNull(cols);
			Assert.AreEqual(6, cols.Length);
		}

		[Test]
		public void GetColumnsContainsProperNullInformation()
		{
			AddTableWithPrimaryKey();
			Column[] cols = Provider.GetColumns("Test");
			Assert.IsNotNull(cols);
			foreach (Column column in cols)
			{
				if (column.Name == "name")
					Assert.IsTrue((column.ColumnProperty & ColumnProperty.NotNull) == ColumnProperty.NotNull);
				else if (column.Name == "Title")
					Assert.IsTrue((column.ColumnProperty & ColumnProperty.Null) == ColumnProperty.Null);
			}
		}

		[Test]
		public void CanAddTableWithPrimaryKey()
		{
			AddTableWithPrimaryKey();
			Assert.IsTrue(Provider.TableExists("Test"));
		}

		[Test]
		public void RemoveTable()
		{
			AddTable();
			Provider.RemoveTable("Test");
			Assert.IsFalse(Provider.TableExists("Test"));
		}

		[Test]
		public virtual void RenameTableThatExists()
		{
			AddTable();
			Provider.RenameTable("Test", "Test_Rename");

			Assert.IsTrue(Provider.TableExists("Test_Rename"));
			Assert.IsFalse(Provider.TableExists("Test"));
			Provider.RemoveTable("Test_Rename");
		}

		[Test]
		[ExpectedException(typeof (MigrationException))]
		public void RenameTableToExistingTable()
		{
			AddTable();
			Provider.RenameTable("Test", "TestTwo");
		}

		[Test]
		public void RenameColumnThatExists()
		{
			AddTable();
			Provider.RenameColumn("Test", "name", "name_rename");

			Assert.IsTrue(Provider.ColumnExists("Test", "name_rename"));
			Assert.IsFalse(Provider.ColumnExists("Test", "name"));
		}

		[Test]
		[ExpectedException(typeof (MigrationException))]
		public void RenameColumnToExistingColumn()
		{
			AddTable();
			Provider.RenameColumn("Test", "Title", "name");
		}

		[Test]
		public void RemoveUnexistingTable()
		{
			Provider.RemoveTable("abc");
		}

		[Test]
		public void AddColumn()
		{
			Provider.AddColumn("TestTwo", "Test", DbType.String, 50);
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "Test"));
		}

		[Test]
		public void ChangeColumn()
		{
			Provider.ChangeColumn("TestTwo", new Column("TestId", DbType.String, 50));
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "TestId"));
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"Not an Int val."});
		}

		[Test]
		public void AddDecimalColumn()
		{
			Provider.AddColumn("TestTwo", "TestDecimal", DbType.Decimal, 38);
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "TestDecimal"));
		}

		[Test]
		public void AddColumnWithDefault()
		{
			Provider.AddColumn("TestTwo", "TestWithDefault", DbType.Int32, 50, 0, 10);
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "TestWithDefault"));
		}

		[Test]
		public void AddColumnWithDefaultButNoSize()
		{
			Provider.AddColumn("TestTwo", "TestWithDefault", DbType.Int32, 10);
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "TestWithDefault"));


			Provider.AddColumn("TestTwo", "TestWithDefaultString", DbType.String, "'foo'");
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "TestWithDefaultString"));
		}

		[Test]
		public void AddBooleanColumnWithDefault()
		{
			Provider.AddColumn("TestTwo", "TestBoolean", DbType.Boolean, 0, 0, false);
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "TestBoolean"));
		}

		[Test]
		public void CanGetNullableFromProvider()
		{
			Provider.AddColumn("TestTwo", "NullableColumn", DbType.String, 30, ColumnProperty.Null);
			Column[] columns = Provider.GetColumns("TestTwo");
			foreach (Column column in columns)
			{
				if (column.Name == "NullableColumn")
				{
					Assert.IsTrue((column.ColumnProperty & ColumnProperty.Null) == ColumnProperty.Null);
				}
			}
		}

		[Test]
		public void RemoveColumn()
		{
			AddColumn();
			Provider.RemoveColumn("TestTwo", "Test");
			Assert.IsFalse(Provider.ColumnExists("TestTwo", "Test"));
		}

		[Test]
		public void RemoveColumnWithDefault()
		{
			AddColumnWithDefault();
			Provider.RemoveColumn("TestTwo", "TestWithDefault");
			Assert.IsFalse(Provider.ColumnExists("TestTwo", "TestWithDefault"));
		}

		[Test]
		public void RemoveUnexistingColumn()
		{
			Provider.RemoveColumn("TestTwo", "abc");
			Provider.RemoveColumn("abc", "abc");
		}

		/// <summary>
		/// Supprimer une colonne bit causait une erreur à cause
		/// de la valeur par défaut.
		/// </summary>
		[Test]
		public void RemoveBoolColumn()
		{
			AddTable();
			Provider.AddColumn("Test", "Inactif", DbType.Boolean);
			Assert.IsTrue(Provider.ColumnExists("Test", "Inactif"));

			Provider.RemoveColumn("Test", "Inactif");
			Assert.IsFalse(Provider.ColumnExists("Test", "Inactif"));
		}

		[Test]
		public void HasColumn()
		{
			AddColumn();
			Assert.IsTrue(Provider.ColumnExists("TestTwo", "Test"));
			Assert.IsFalse(Provider.ColumnExists("TestTwo", "TestPasLa"));
		}

		[Test]
		public void HasTable()
		{
			Assert.IsTrue(Provider.TableExists("TestTwo"));
		}

		[Test]
		public void AppliedMigrations()
		{
			Assert.IsFalse(Provider.TableExists("SchemaInfo"));

			// Check that a "get" call works on the first run.
			Assert.AreEqual(0, Provider.AppliedMigrations.Count);
			Assert.IsTrue(Provider.TableExists("SchemaInfo"), "No SchemaInfo table created");

			// Check that a "set" called after the first run works.
			Provider.MigrationApplied(1);
			Assert.AreEqual(1, Provider.AppliedMigrations[0]);

			Provider.RemoveTable("SchemaInfo");
			// Check that a "set" call works on the first run.
			Provider.MigrationApplied(1);
			Assert.AreEqual(1, Provider.AppliedMigrations[0]);
			Assert.IsTrue(Provider.TableExists("SchemaInfo"), "No SchemaInfo table created");
		}

		/// <summary>
		/// Reproduce bug reported by Luke Melia & Daniel Berlinger :
		/// http://macournoyer.wordpress.com/2006/10/15/migrate-nant-task/#comment-113
		/// </summary>
		[Test]
		public void CommitTwice()
		{
			Provider.Commit();
			Assert.AreEqual(0, Provider.AppliedMigrations.Count);
			Provider.Commit();
		}

		[Test]
		public void InsertData()
		{
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"1"});
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"2"});
			using (IDataReader reader = Provider.Select("TestId", "TestTwo"))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, delegate(int val) { return val == 1; }));
				Assert.IsTrue(Array.Exists(vals, delegate(int val) { return val == 2; }));
			}
		}

		[Test]
		public void CanInsertNullData()
		{
			AddTable();
			Provider.Insert("Test", new string[] {"Id", "Title"}, new string[] {"1", "foo"});
			Provider.Insert("Test", new string[] {"Id", "Title"}, new string[] {"2", null});
			using (IDataReader reader = Provider.Select("Title", "Test"))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsTrue(Array.Exists(vals, delegate(string val) { return val == "foo"; }));
				Assert.IsTrue(Array.Exists(vals, delegate(string val) { return val == null; }));
			}
		}

		[Test]
		public void CanInsertDataWithSingleQuotes()
		{
			AddTable();
			Provider.Insert("Test", new string[] {"Id", "Title"}, new string[] {"1", "Muad'Dib"});
			using (IDataReader reader = Provider.Select("Title", "Test"))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual("Muad'Dib", reader.GetString(0));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void DeleteData()
		{
			InsertData();
			Provider.Delete("TestTwo", "TestId", "1");

			using (IDataReader reader = Provider.Select("TestId", "TestTwo"))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, Convert.ToInt32(reader[0]));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void DeleteDataWithArrays()
		{
			InsertData();
			Provider.Delete("TestTwo", new string[] {"TestId"}, new string[] {"1"});

			using (IDataReader reader = Provider.Select("TestId", "TestTwo"))
			{
				Assert.IsTrue(reader.Read());
				Assert.AreEqual(2, Convert.ToInt32(reader[0]));
				Assert.IsFalse(reader.Read());
			}
		}

		[Test]
		public void UpdateData()
		{
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"1"});
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"2"});

			Provider.Update("TestTwo", new string[] {"TestId"}, new string[] {"3"});

			using (IDataReader reader = Provider.Select("TestId", "TestTwo"))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, delegate(int val) { return val == 3; }));
				Assert.IsFalse(Array.Exists(vals, delegate(int val) { return val == 1; }));
				Assert.IsFalse(Array.Exists(vals, delegate(int val) { return val == 2; }));
			}
		}

		[Test]
		public void CanUpdateWithNullData()
		{
			AddTable();
			Provider.Insert("Test", new string[] {"Id", "Title"}, new string[] {"1", "foo"});
			Provider.Insert("Test", new string[] {"Id", "Title"}, new string[] {"2", null});

			Provider.Update("Test", new string[] {"Title"}, new string[] {null});

			using (IDataReader reader = Provider.Select("Title", "Test"))
			{
				string[] vals = GetStringVals(reader);

				Assert.IsNull(vals[0]);
				Assert.IsNull(vals[1]);
			}
		}

		[Test]
		public void UpdateDataWithWhere()
		{
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"1"});
			Provider.Insert("TestTwo", new string[] {"TestId"}, new string[] {"2"});

			Provider.Update("TestTwo", new string[] {"TestId"}, new string[] {"3"}, "TestId='1'");

			using (IDataReader reader = Provider.Select("TestId", "TestTwo"))
			{
				int[] vals = GetVals(reader);

				Assert.IsTrue(Array.Exists(vals, delegate(int val) { return val == 3; }));
				Assert.IsTrue(Array.Exists(vals, delegate(int val) { return val == 2; }));
				Assert.IsFalse(Array.Exists(vals, delegate(int val) { return val == 1; }));
			}
		}

		private int[] GetVals(IDataReader reader)
		{
			int[] vals = new int[2];
			Assert.IsTrue(reader.Read());
			vals[0] = Convert.ToInt32(reader[0]);
			Assert.IsTrue(reader.Read());
			vals[1] = Convert.ToInt32(reader[0]);
			return vals;
		}

		private string[] GetStringVals(IDataReader reader)
		{
			string[] vals = new string[2];
			Assert.IsTrue(reader.Read());
			vals[0] = reader[0] as string;
			Assert.IsTrue(reader.Read());
			vals[1] = reader[0] as string;
			return vals;
		}
	}
}
