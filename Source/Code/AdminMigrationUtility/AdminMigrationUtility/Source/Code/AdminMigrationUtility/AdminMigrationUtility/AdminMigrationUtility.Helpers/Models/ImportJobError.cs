using System;

namespace AdminMigrationUtility.Helpers.Models
{
	public class ImportJobError
	{
		public Int32 ArtifactID { get; set; }
		public Int32? LineNumber { get; set; }
		public String Message { get; set; }
		public String Details { get; set; }
		public Constant.ImportUtilityJob.ErrorType? Type { get; set; }
	}
}