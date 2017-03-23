using AdminMigrationUtility.Helpers.Models;
using NUnit.Framework;
using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class ExportManagerQueueRecordTests
	{
		[Test]
		public void Constructor_ReceivesTable_Initializes()
		{
			// Arrange
			var table = GetTable();

			// Act
			var record = new ExportManagerQueueRecord(table.Rows[0]);

			// Assert
			Assert.AreEqual(2345678, record.WorkspaceArtifactId);
			Assert.AreEqual(1, record.TableRowId);
			Assert.AreEqual(3456789, record.ExportJobArtifactId);
			Assert.AreEqual(2, record.Priority);
		}

		[Test]
		public void Constructor_ReceivesNullTable_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new ExportManagerQueueRecord(null));
		}

		private DataTable GetTable()
		{
			var table = new DataTable();
			/*
			table.Columns.Add("WorkspaceArtifactId", typeof(Int32));
			table.Columns.Add("ID", typeof(Int32));
			table.Columns.Add("ArtifactId", typeof(Int32));
			table.Columns.Add("Priority", typeof(Int32));
			table.Columns.Add("ResourceGroupId", typeof(Int32));*/

			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.Priority, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.ResultsTableName, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc, typeof(DateTime));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceName, typeof(String));
			//table.Rows.Add(1,1,2345678, 3456789, "User",2, 100001, Constant.Status.Queue.NOT_STARTED,"Temp Table", DateTime.UtcNow);
			table.Rows.Add(1, null, 2345678, 3456789, "User", 2, 100001, Constant.Status.Queue.NOT_STARTED, "Temp Table", DateTime.UtcNow, "Random Workspace Name");
			return table;
		}
	}
}