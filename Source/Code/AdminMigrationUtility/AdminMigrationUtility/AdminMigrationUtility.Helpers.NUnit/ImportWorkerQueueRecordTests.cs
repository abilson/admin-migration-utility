using AdminMigrationUtility.Helpers.Models;
using NUnit.Framework;
using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class ImportWorkerQueueRecordTests
	{
		public Int32 RecordID;
		public Int32 WorkspaceArtifactID;
		public Int32 AgentID;
		public Int32 JobID;
		public String ObjectType;
		public String JobType;
		public String MetaData;
		public Int32 ImportRowID;
		public Int32 QueueStatus;
		public Int32 ResourceGroupID;
		public Int32 Priority;
		public DateTime TimeStampUTC;

		[SetUp]
		public void Setup()
		{
			RecordID = 1;
			WorkspaceArtifactID = 1014750;
			AgentID = 1;
			JobID = 1;
			ObjectType = Enum.GetName(typeof(Constant.Enums.SupportedObjects), Constant.Enums.SupportedObjects.User);
			JobType = Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT;
			MetaData = "";
			ImportRowID = 1;
			QueueStatus = Constant.Status.Queue.NOT_STARTED;
			ResourceGroupID = 123;
			Priority = 1;
			TimeStampUTC = DateTime.UtcNow;
		}

		[TearDown]
		public void TearDown()
		{
			RecordID = 0;
			WorkspaceArtifactID = 0;
			AgentID = 0;
			JobID = 0;
			ObjectType = null;
			JobType = null;
			MetaData = null;
			ImportRowID = 0;
			QueueStatus = 0;
			ResourceGroupID = 0;
			Priority = 0;
			TimeStampUTC = new DateTime();
		}

		[Test]
		public void Constructor_ReceivesTable_Initializes()
		{
			// Arrange
			var table = GetTable();

			// Act
			var record = new ImportWorkerQueueRecord(table.Rows[0]);

			// Assert
			Assert.AreEqual(RecordID, record.RecordID);
			Assert.AreEqual(WorkspaceArtifactID, record.WorkspaceArtifactID);
			Assert.AreEqual(AgentID, record.AgentID);
			Assert.AreEqual(JobID, record.JobID);

			Assert.AreEqual(ObjectType, record.ObjectType);
			Assert.AreEqual(JobType, record.JobType);
			Assert.AreEqual(MetaData, record.MetaData);
			Assert.AreEqual(ImportRowID, record.ImportRowID);

			Assert.AreEqual(QueueStatus, record.QueueStatus);
			Assert.AreEqual(ResourceGroupID, record.ResourceGroupID);
			Assert.AreEqual(Priority, record.Priority);
			Assert.AreEqual(TimeStampUTC.Day, record.TimeStampUTC.Day);
		}

		[Test]
		public void Constructor_ReceivesNullTable_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => new ImportWorkerQueueRecord(null));
		}

		private DataTable GetTable()
		{
			var table = new DataTable("Test Worker Table");
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC, typeof(DateTime));
			table.Rows.Add(RecordID, AgentID, JobID, WorkspaceArtifactID, ObjectType, JobType, MetaData, ImportRowID, QueueStatus, ResourceGroupID, Priority, TimeStampUTC);
			return table;
		}
	}
}