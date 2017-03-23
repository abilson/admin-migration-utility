using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.Models
{
	/// <summary>
	/// Represents a single row in the Export Manager Queue Table
	/// </summary>
	public class ExportManagerQueueRecord
	{
		public Int32 TableRowId { get; }
		public Int32? AgentId { get; }
		public Int32 WorkspaceArtifactId { get; }
		public Int32 ExportJobArtifactId { get; }
		public String ObjectType { get; }
		public Int32? Priority { get; }
		public Int32 WorkspaceResourceGroupArtifactId { get; }
		public Int32 QueueStatus { get; }
		public String ResultsTableName { get; }
		public DateTime TimeStampUtc { get; }
		public String WorkspaceName { get; }

		public ExportManagerQueueRecord(DataRow exportManagerQueueDataRow)
		{
			if (exportManagerQueueDataRow == null)
			{
				throw new ArgumentNullException(nameof(exportManagerQueueDataRow));
			}

			TableRowId = (Int32)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId];
			AgentId = exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId] == DBNull.Value ? null : (Int32?)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId];
			WorkspaceArtifactId = (Int32)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId];
			ExportJobArtifactId = (Int32)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId];
			ObjectType = (String)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType];
			Priority = exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.Priority] == DBNull.Value ? null : (Int32?)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.Priority];
			WorkspaceResourceGroupArtifactId = (Int32)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId];
			QueueStatus = (Int32)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus];
			ResultsTableName =
				exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.ResultsTableName] != DBNull.Value
				? exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.ResultsTableName].ToString()
				: String.Empty;
			TimeStampUtc = (DateTime)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc];
			WorkspaceName = (String)exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceName];
		}
	}
}