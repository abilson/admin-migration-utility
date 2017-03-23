using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.Models
{
	/// <summary>
	/// Represents a single row in the Export Worker Queue Table
	/// </summary>
	public class ExportWorkerQueueRecord
	{
		public Int32 QueueRowId { get; }
		public Int32? AgentId { get; }
		public Int32 WorkspaceArtifactId { get; }
		public Int32 ExportJobArtifactId { get; }
		public String ObjectType { get; }
		public Int32? Priority { get; }
		public Int32 WorkspaceResourceGroupArtifactId { get; }
		public Int32 QueueStatus { get; }
		public Int32 ArtifactId { get; }
		public String ResultTableName { get; }
		public String MetaData { get; }
		public DateTime TimeStampUtc { get; }

		public ExportWorkerQueueRecord(DataRow exportWorkerQueueDataRow)
		{
			if (exportWorkerQueueDataRow == null)
			{
				throw new ArgumentNullException(nameof(exportWorkerQueueDataRow));
			}

			QueueRowId = (Int32)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId];
			AgentId = exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId] == DBNull.Value ? null : (Int32?)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId];
			WorkspaceArtifactId = (Int32)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId];
			ExportJobArtifactId = (Int32)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId];
			ObjectType = (String)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType];
			Priority = exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority] == DBNull.Value ? null : (Int32?)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority];
			WorkspaceResourceGroupArtifactId = (Int32)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId];
			QueueStatus = (Int32)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus];
			ArtifactId = (Int32)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId];
			ResultTableName = (String)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName];
			MetaData = (String)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData];
			TimeStampUtc = (DateTime)exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc];
		}
	}
}