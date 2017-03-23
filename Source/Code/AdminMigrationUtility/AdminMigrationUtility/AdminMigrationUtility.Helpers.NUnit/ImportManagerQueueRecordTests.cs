using AdminMigrationUtility.Helpers.Models;
using NUnit.Framework;
using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class ImportManagerQueueRecordTests
	{
		[Test]
		public void Constructor_ReceivesTable_Initializes()
		{
			// Arrange
			var table = GetTable();

			// Act
			var record = new ImportManagerQueueRecord(table.Rows[0]);

			// Assert
			Assert.AreEqual(2345678, record.WorkspaceArtifactId);
			Assert.AreEqual(1, record.RecordId);
			Assert.AreEqual(3456789, record.JobId);
			Assert.AreEqual(2, record.Priority);
		}

		[Test]
		public void Constructor_ReceivesNullTable_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new ImportManagerQueueRecord(null));
		}

		private DataTable GetTable()
		{
			var table = new DataTable();
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.ID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.JobID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.Priority, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.JobType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName, typeof(String));
			table.Rows.Add(1, 2345678, 1, 3456789, 2, 100001, "", "", 0, "Random Workspace Name");
			return table;
		}
	}
}