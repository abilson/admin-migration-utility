using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.Models
{
	/// <summary>
	/// Represents a single row in the Manager Queue Table
	/// </summary>
	public class ImportManagerQueueRecord
	{
		#region Properties

		public Int32 RecordId { get; }
		public Int32 WorkspaceArtifactId { get; }
		public Int32 JobId { get; }
		public Int32? Priority { get; }
		public Int32 ResourceGroupId { get; }
		public Int32 QueueStatus { get; }
		public Int32? AgentId { get; }
		public string ObjectType { get; }
		public string JobType { get; }
		public string WorkspaceName { get; }

		#endregion Properties

		public ImportManagerQueueRecord(DataRow row)
		{
			if (row == null) { throw new ArgumentNullException(nameof(row)); }

			WorkspaceArtifactId = (Int32)row[Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID];
			RecordId = (Int32)row[Constant.Sql.ColumnsNames.ImportManagerQueue.ID];
			AgentId = row[Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID] == DBNull.Value ? null : (Int32?)row[Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID];
			JobId = (Int32)row[Constant.Sql.ColumnsNames.ImportManagerQueue.JobID];
			Priority = row[Constant.Sql.ColumnsNames.ImportManagerQueue.Priority] == DBNull.Value ? null : (Int32?)row[Constant.Sql.ColumnsNames.ImportManagerQueue.Priority];
			ResourceGroupId = (Int32)row[Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID];
			ObjectType = (string)row[Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType];
			JobType = (string)row[Constant.Sql.ColumnsNames.ImportManagerQueue.JobType];
			QueueStatus = (Int32)row[Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus];
			WorkspaceName = (string)row[Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName];
		}
	}
}