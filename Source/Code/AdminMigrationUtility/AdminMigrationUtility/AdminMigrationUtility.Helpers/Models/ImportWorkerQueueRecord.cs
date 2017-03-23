using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.Models
{
	/// <summary>
	/// Represents a single row in the Worker Queue Table
	/// </summary>
	public class ImportWorkerQueueRecord
	{
		#region Properties

		public Int32 RecordID { get; }
		public Int32 WorkspaceArtifactID { get; }
		public Int32? AgentID { get; }
		public Int32 JobID { get; }
		public String ObjectType { get; }
		public String JobType { get; }
		public String MetaData { get; }
		public Int32 ImportRowID { get; }
		public Int32 QueueStatus { get; }
		public Int32 ResourceGroupID { get; }
		public Int32? Priority { get; }
		public DateTime TimeStampUTC { get; }

		#endregion Properties

		public ImportWorkerQueueRecord(DataRow row)
		{
			if (row == null) { throw new ArgumentNullException("row"); }

			RecordID = (Int32)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.ID];
			AgentID = row[Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID] == DBNull.Value ? null : (Int32?)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID];
			JobID = (Int32)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID];
			WorkspaceArtifactID = (Int32)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID];
			ObjectType = (String)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType];
			JobType = (String)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType];
			MetaData = (String)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData];
			ImportRowID = (Int32)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID];
			QueueStatus = (Int32)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus];
			ResourceGroupID = (Int32)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID];
			Priority = row[Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority] == DBNull.Value ? null : (Int32?)row[Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority];
			TimeStampUTC = DateTime.Parse(row[Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC].ToString());
		}
	}
}