using System;
using System.Data;

namespace AdminMigrationUtility.Helpers.Models
{
	public class ExportWorkerResultsTableRecord
	{
		public Int32 TableRowId;
		public Int32 AgentId;
		public Int32 WorkspaceArtifactId;
		public Int32 ExportJobArtifactId;
		public Constant.Enums.SupportedObjects ObjectType;
		public Int32 WorkspaceResourceGroupArtifactId;
		public Int32 QueueStatus;
		public String MetaData;
		public DateTime TimeStampUtc;

		public ExportWorkerResultsTableRecord(DataRow exportWorkerResultsTableDataRow)
		{
			if (exportWorkerResultsTableDataRow == null)
			{
				throw new ArgumentNullException(nameof(exportWorkerResultsTableDataRow));
			}

			TableRowId = (Int32)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TableRowId];
			AgentId = (Int32)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.AgentId];
			WorkspaceArtifactId = (Int32)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceArtifactId];
			ExportJobArtifactId = (Int32)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ExportJobArtifactId];
			String objectTypeString = (String)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.ObjectType];
			ObjectType = (Constant.Enums.SupportedObjects)Enum.Parse(typeof(Constant.Enums.SupportedObjects), objectTypeString);
			WorkspaceResourceGroupArtifactId = (Int32)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.WorkspaceResourceGroupArtifactId];
			QueueStatus = (Int32)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.QueueStatus];
			MetaData = (String)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.MetaData];
			TimeStampUtc = (DateTime)exportWorkerResultsTableDataRow[Constant.Sql.ColumnsNames.ExportWorkerResultsTable.TimeStampUtc];
		}
	}
}