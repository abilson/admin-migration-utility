using AdminMigrationUtility.Helpers.Models;
using NUnit.Framework;
using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class ExportWorkerQueueRecordTests
	{
		[Test]
		public void Constructor_ReceivesTable_Initializes()
		{
			// Arrange
			var table = GetTable();

			// Act
			var record = new ExportWorkerQueueRecord(table.Rows[0]);

			// Assert
			Assert.AreEqual(2345678, record.WorkspaceArtifactId);
			Assert.AreEqual(1, record.QueueRowId);
			Assert.AreEqual(3456789, record.ArtifactId);
			Assert.AreEqual(2, record.Priority);
		}

		[Test]
		public void Constructor_ReceivesNullTable_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new ImportWorkerQueueRecord(null));
		}

		private DataTable GetTable()
		{
			var table = new DataTable();
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc, typeof(DateTime));

			table.Rows.Add(1, 1, 2345678, 3456789, "User", 2, 12345, Constant.Status.Queue.NOT_STARTED, 3456789, "TestTable", "TestMetaData", DateTime.UtcNow);
			return table;
		}
	}
}