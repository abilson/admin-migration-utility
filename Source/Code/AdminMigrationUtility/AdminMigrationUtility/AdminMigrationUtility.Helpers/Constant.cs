using System;

namespace AdminMigrationUtility.Helpers
{
	public class Constant
	{
		public class Relativity
		{
			public const string SupportedVersion = "9.4.0.0";
		}

		public class CodeTypeName
		{
			public static readonly string UserType = "UserType";
			public static readonly string DocumentSkip = "DocumentSkip";
			public static readonly string DefaultSelectedFileType = "DefaultSelectedFileType";
			public static readonly string SkipDefaultPreference = "SkipDefaultPreference";
			public static readonly string DocumentViewer = "DocumentViewer";
		}

		public class Tables
		{
			public static readonly String ImportWorkerQueue = "AdminMigrationUtility_ImportWorkerQueue";
			public static readonly String ImportManagerQueue = "AdminMigrationUtility_ImportManagerQueue";
			public static readonly String ExportWorkerQueue = "AdminMigrationUtility_ExportWorkerQueue";
			public static readonly String ExportManagerQueue = "AdminMigrationUtility_ExportManagerQueue";
			public static readonly String ImportErrorLog = "AdminMigrationUtility_ImportErrorLog";
			public static readonly String ExportErrorLog = "AdminMigrationUtility_ExportErrorLog";
		}

		public class Names
		{
			public static readonly String ApplicationName = "Admin Migration Utility";
			public static readonly String TablePrefix = "AdminMigrationUtility_";
			public static readonly String ImportManagerHoldingTablePrefix = $"{TablePrefix}ImportManagerHoldingTable_";
			public static readonly String ExportWorkerResultsTablePrefix = $"{TablePrefix}ExportWorkerResultsTable_";

			public class Rdos
			{
				public const String EXPORT_UTILITY_JOB = "Export Utility";
				public const String IMPORT_UTILITY_JOB = "Import Utility";
			}
		}

		public class Guids
		{
			public class Application
			{
				public static readonly Guid ApplicationGuid = new Guid("2E3E2751-18CF-4382-84D4-11605EFA4AEB");
			}

			public class Tabs
			{
				public static readonly Guid ExportUtilityJob = new Guid("2C79646B-CD6A-43C2-9D98-5D8BA98D021C");
				public static readonly Guid ImportUtilityJob = new Guid("8010A1BC-FBE1-4EE6-8342-0DFADF7CBECA");
				public static readonly Guid ExportManagerQueue = new Guid("D9407EB4-65D7-48E0-857C-52793490FFC6");
				public static readonly Guid ImportManagerQueue = new Guid("3E909448-5726-4895-9D26-34F5E20DE77F");
				public static readonly Guid ExportWorkerQueue = new Guid("26CA5B7E-6464-4A11-B691-C2DEA7DF1B50");
				public static readonly Guid ImportWorkerQueue = new Guid("A2FC8EB1-55AF-44EC-9F00-3978DC652584");
			}

			public class ObjectType
			{
				public static readonly Guid ExportUtilityJob = new Guid("4D6074CE-98E2-4592-83E8-6057A122C28A");
				public static readonly Guid ImportUtilityJob = new Guid("F770DA85-1ADC-48EC-B289-68427DD3B0CA");
				public static readonly Guid ExportUtilityJobErrors = new Guid("C5FC35FE-2A85-4E08-AC49-98A02ABF0D88");
				public static readonly Guid ImportUtilityJobErrors = new Guid("FE1250CD-03FF-4C06-95DC-A78935084345");
			}

			public class Field
			{
				public class ExportUtilityJob
				{
					public static readonly Guid SystemCreatedOn = new Guid("76B0D43F-F2A0-4E09-9D55-98C9389163FD");
					public static readonly Guid SystemLastModifiedOn = new Guid("72C91F13-C8AC-44C1-8C93-85D2CCF08E6E");
					public static readonly Guid SystemCreatedBy = new Guid("64087516-C201-4F54-A194-C0E2E0517432");
					public static readonly Guid SystemLastModifiedBy = new Guid("7F9E14B5-5AD9-4C2B-BD28-501DCF657DE5");
					public static readonly Guid ArtifactId = new Guid("B21A9290-4882-40A8-9E57-5ACEFEF45FE7");
					public static readonly Guid Name = new Guid("9116C4FC-53BF-4D56-B487-4CDE5B088F32");
					public static readonly Guid ObjectType = new Guid("32979F56-82A0-4C16-A40F-C62B43E2E355");
					public static readonly Guid Status = new Guid("175B7D3A-E8D5-47A1-B3BC-8C7C5555FA08");
					public static readonly Guid ExportFile = new Guid("2FCE0262-2642-4249-BD15-9966E935D512");
					public static readonly Guid ExportFileFileIcon = new Guid("22144076-33D5-4512-8F8C-8AEA3A4F6B73");
					public static readonly Guid ExportfileFileSize = new Guid("34DA6130-0A86-4069-B0A8-5EA8796C0BA0");
					public static readonly Guid ExportFileText = new Guid("90A89C49-C351-46F8-8299-B6D00442B519");
					public static readonly Guid EmailAddresses = new Guid("051FA540-72BE-4B8B-8B50-E962B5478A1A");
					public static readonly Guid Priority = new Guid("D4710822-4A58-404E-BFFF-B270A7304E83");
					public static readonly Guid Expected = new Guid("2C8DE428-0AA4-45B0-A6D4-AFE2E663BA42");
					public static readonly Guid Exported = new Guid("4A8DBD3B-686E-43C1-8D46-F3D1790C6D2B");
					public static readonly Guid NotExported = new Guid("111A89A7-D135-4173-ADB8-F94E445674E7");
					public static readonly Guid RecordsThatHaveBeenProcessed = new Guid("CBA3619F-9AEF-47B9-A0E2-803162BCCEAD");
				}

				public class ImportUtilityJob
				{
					public static readonly Guid SystemCreatedOn = new Guid("6ECCE784-68F6-4E24-A9FE-B227AEF66590");
					public static readonly Guid SystemLastModifiedOn = new Guid("84DDD037-301E-4044-A818-162A8752104D");
					public static readonly Guid SystemCreatedBy = new Guid("FE5ABC9B-D13E-48CF-8DC5-5BC281252641");
					public static readonly Guid SystemLastModifiedBy = new Guid("20EFB40C-5A13-4740-B373-9A1176C0B4FB");
					public static readonly Guid ArtifactId = new Guid("ED486598-D522-4655-B43E-76B44036D575");
					public static readonly Guid Name = new Guid("C3C52CDB-D985-456E-80E3-08FD45D2A20C");
					public static readonly Guid ObjectType = new Guid("0EAAA924-9C13-4125-8BD8-04CD03B49B99");
					public static readonly Guid Status = new Guid("E4EAE50B-49A5-4B59-AAD9-6E2F72AF2459");
					public static readonly Guid ImportFile = new Guid("47C69CDE-085A-4865-98D7-1520579A2E44");
					public static readonly Guid ImportFileFileIcon = new Guid("2F7C9E28-D7D8-47AE-B259-5A5F966F7E09");
					public static readonly Guid ImportfileFileSize = new Guid("B4C1BB5C-14EC-4414-BF55-D38DD38EC9F6");
					public static readonly Guid ImportFileText = new Guid("8D876548-26EB-4DA8-B4E3-B7E8F1ECCE7B");
					public static readonly Guid EmailAddresses = new Guid("FAFD61AA-15D1-44D9-92CB-06471482B6AF");
					public static readonly Guid SubmittedForMigration = new Guid("81EAB151-CFD3-4DEF-AB50-6AA12A355274");
					public static readonly Guid Priority = new Guid("0C60042D-3ACC-432F-8C9A-81C8B8372344");
					public static readonly Guid Expected = new Guid("8AD4148D-2893-4648-A837-891EBB7849AB");
					public static readonly Guid Imported = new Guid("0C5C6934-7159-404F-88F1-0A2325885E1B");
					public static readonly Guid NotImported = new Guid("6AB15C7B-D0D0-43A4-AFF5-F8F4072B2667");
				}

				public class ExportUtilityJobErrors
				{
					public static readonly Guid Name = new Guid("5FA8A50C-9E6F-4DDD-93DB-5DF36C8A069C");
					public static readonly Guid ExportUtilityJob = new Guid("DD1C9C43-25A1-4605-B8F2-930CCD300C75");
					public static readonly Guid ObjectType = new Guid("2CE02EE1-EB27-4AD1-B87A-ECB5B553C1C4");
					public static readonly Guid Status = new Guid("58A726E6-B65A-415B-B36E-34AD00D3ABF3");
					public static readonly Guid Details = new Guid("60D27D29-4D0B-4CB9-A5B8-366BB689F55B");
				}

				public class ImportUtilityJobErrors
				{
					public static readonly Guid Name = new Guid("5DBE812B-649B-41F7-8281-5BBBBC27A048");
					public static readonly Guid ImportUtilityJob = new Guid("C9B4C4A8-9260-4454-A56C-60D7C7D52FC6");
					public static readonly Guid Type = new Guid("817F5A36-6652-4E28-A445-BB5392766650");
					public static readonly Guid Message = new Guid("7D37436B-2961-45A0-AD1B-8FD2677ECBDB");
					public static readonly Guid Details = new Guid("40F195F5-B041-43B2-8B4E-02A839E75A4D");
					public static readonly Guid LineNumber = new Guid("5E5DC283-0B16-4122-9B06-EF45C6D0EA1D");
					public static readonly Guid ObjectType = new Guid("F8647E05-B9C5-48B0-9CAC-D052B5E12A76");
				}
			}

			public class Choices
			{
				public class ExportUtilityJob
				{
					public class ObjectType
					{
						public static readonly Guid Client = new Guid("FFDA3768-E6A6-4A6F-AE12-3041A65CDD9B");
						public static readonly Guid Group = new Guid("D554DDE2-E69F-4DA5-8FB4-985D2251B1E5");
						public static readonly Guid Matter = new Guid("2C810E8B-DA40-4384-BA25-566CE23D2AA0");
						public static readonly Guid User = new Guid("225A4A6A-D1CB-4C4E-8E3E-FBFA7C1069A5");
					}
				}

				public class ImportUtilityJob
				{
					public class ObjectType
					{
						public static readonly Guid Client = new Guid("A19EDA0A-D9DB-4302-AA91-6B574B13ED11");
						public static readonly Guid Group = new Guid("C46270C5-A092-4BB2-81F9-963002124F82");
						public static readonly Guid Matter = new Guid("8FC2BB7E-A21B-4580-A8ED-5C6337579C68");
						public static readonly Guid User = new Guid("0DC2B017-5083-47ED-8769-9146A7805243");
					}
				}
			}

			public class Layouts
			{
				public class ExportUtilityJob
				{
					public static Guid ExportUtilityJobLayout = new Guid("549C5679-A2FA-4DD0-A8C8-B755BD72F2D9");
				}

				public class ImportUtilityJob
				{
					public static Guid ImportUtilityJobLayout = new Guid("922822CD-3D08-428F-9C11-0180216CC243");
				}

				public class ExportUtilityJobErrors
				{
					public static Guid ExportUtilityJobErrorsLayout = new Guid("2F401E5D-4C06-4B04-B58F-AA6AC859BF46");
				}

				public class ImportUtilityJobErrors
				{
					public static Guid ImportUtilityJobErrorsLayout = new Guid("93568F24-B5FF-4078-B049-561C9FF1F48F");
				}
			}
		}

		public class Enums
		{
			public enum ObjectFieldType
			{
				Standard,
				IntegratedLoginMethod,
				PasswordLoginMethod
			}

			public enum SupportedObjects
			{
				User/*, Add these as support is enabled
				Matter,
				Group,
				Client*/
			}
		}

		public class BatchSizes
		{
			public static readonly Int32 ImportManagerIntoWorkerQueue = 500;
			public static readonly Int32 ImportWorker = 20;
			public static readonly Int32 ExportWorkerJobQueue = 100;
			public static readonly Int32 ExportManagerCSVQuery = 10;
			public static readonly Int32 UserQuery = 150;
			public static readonly Int32 ExportManagerIntoWorkerQueue = 500;
			public static readonly Int32 JobErrorQuery = 100;
			public static readonly Int32 MaximumQueueSelect = 1000;
		}
		

		public class Messages
		{
			public class Agent
			{
				public const String PRIORITY_REQUIRED = "Please enter a priority";
				public const String ARTIFACT_ID_REQUIRED = "Please enter an artifact ID";
				public const String AGENT_OFF_HOURS_NOT_FOUND = "No agent off-hours found in the configuration table.";
				public const String AGENT_OFF_HOUR_TIMEFORMAT_INCORRECT = "Please verify that the EDDS.Configuration AgentOffHourStartTime & AgentOffHourEndTime is in the following format HH:MM:SS";
			}

			public class Violations
			{
				public static readonly String GeneralColumnViolation = "Column [{0}] has the following violations: {1}";
				public static readonly String NullValue = "Null Values are not allowed.";
				public static readonly String MaxLengthExceeded = "This column exceeds the maximum length of {0} characters.";
				public static readonly String RegExMisMatch = "Incorrectly Formatted {0}.";
				public static readonly String NotConvertible = "Incorrect data type, please refer to the documentation to see the supported data type for this column. ";
				public static readonly String ClientDoesntExist = "Client with name {0} does not exist";
				public static readonly String OutOfNumericRange = "This value should be between {0} and {1}";
				public static readonly String NotANumber = "Please make sure this value is a number";
				public static readonly String NoLoginMethodSelected = "Please enter a value in either the Password Authentication Columns or Integrated Authentication Columns";
				public static readonly String MissingTwoFactorMode = "Please enter a TwoFactorMode is required.";
				public static readonly String MissingPasswordColumnValue = "Please enter a value in this field, it is required when any other field related to a User's Password Login Method is set.";
				public static readonly String InvalidTwoFactorMode = "{0} is not a valid value for 2 factor mode, only the following values are accepted: {1}";
				public static readonly String InvalidTwoFactorInfoEmailBadFormat = "{0} requires a correctly formatted email address when TwoFactorMode contains a value and is populated with anything other than '{1}'";
				public static readonly String InvalidTwoFactorInfoEmpty = "{0} requires a value when {1} is set to anything other than {2}";
				public static readonly String TwoFactorInfoShouldBeEmpty = "Please clear the {0} column when the {1} column is set to anything other than {2}.";
				public static readonly String ChoiceDoesNotExist = "{0} Choice does not exist for {1}";
				public static readonly String GroupDoesNotExist = "The following group(s) do not exist: {0}.";
				public static readonly String Exception = "Please Try again, the following error occurred when {0}: {1}";
			}
		}

		public class ErrorMessages
		{
			public static readonly String DefaultErrorPrepend = "Error encountered: ";
			public static readonly String QueryUsersError = "An error occured when query for users.";
			public static readonly String ReadSingleUserError = "Unable to export user with the following ArtifactID: {0}";
			public static readonly String NoUsersFound = "No users found.";
			public static readonly String UserKeywordsNotesError = "@An error occured when querying for user's keywords and notes data.";
			public static readonly String UserKeywordsNotesError_UserNotFound = "@An error occured when querying for user's keywords and notes data. User not found.";
			public static readonly String UserKeywordsNotesError_MultipleUsers = @"Multiple users exist with the same userArtifactId when querying for user's keywords and notes data.";
			public static readonly String QueryAllUserAdminObjectsError = @"An error occured when querying for user admin objects.";
			public static readonly String InsertingAllUsersInToExportWorkerQueueTableError = @"An error occured when inserting all users into ExportWorker queue table.";
			public static readonly String DeletingLoadFileError = @"Unable to delete load file after download.";
			public static readonly String InsertingAUserIntoExportWorkerQueueTableError = @"An error occured when inserting a user into ExportWorker queue table.";
			public static readonly String ExportManagerJobError = @"An error occured in Export ManagerJob when quering for data.";
			public static readonly String ExportWorkerJobError = @"An error occured in Export Worker Job when quering for data.";
			public static readonly String ExportManagerJobUserError = @"An error occured in Export Manager Job when quering for all users.";
			public static readonly String ExportWorkerJobUserError = @"An error occured in Export Worker Job when quering for user data.";
			public static readonly String UpdateExportJobStatusErrror = @"An error occured when updating the status of ExportJob.";
			public static readonly String RdoTextFieldQueryError = @"An error occurred when querying for the text field of an RDO.";
			public static readonly String RdoTextFieldUpdateError = @"An error occurred while updating the text field of an RDO.";
			public static readonly String FindUserByEmailError = @"An error occurred while querying user by email.";
			public static readonly String RetrieveWorkspaceResourcePoolArtifactIdError = @"An error occurred querying for the ResourcePool ArtifactId of the workspace.";
			public static readonly String RetrieveImportJobError = @"An error occurred querying for the Import Job.";
			public static readonly String UpdateImportJobStatisticsError = @"An error occurred while updating import job statistics.";
			public static readonly String UpdateExportJobStatisticsError = @"An error occurred while updating export job statistics.";
			public static readonly String UpdateExportJobExpectedNumberError = @"An error occurred while updating export job expected number of exports.";
			public static readonly String DownloadFileError = @"An error occurred while downloading file.";
			public static readonly String RetrieveWorkspaceResourcePoolArtifactIdNullError = @"The ResourcePool ArtifactId of the workspace is NULL.";
			public static readonly String ObjectColumnDataConversionError = @"Error Casting Data:{0} from Column{1}.";
			public static readonly String KeywordsAndNotesUpdateError = @"Error updating keywords and notes field for Object {0}.";
			public static readonly String FormatMetaDataError = @"An error occured when formatting metadata.";
			public static readonly String UnableToOpenFileError = @"Unable to open downloaded file";
			public static readonly String UnableToParseFileError = @"An error occured while creating a parser to read the file.";
			public static readonly String UnableToParseLineNumberError = @"An error occured after attempting to parse load file line number {0}.";
			public static readonly String UnsupportedObjectError = @"Unsupported object selected, only the following object types are supported: {0}.";
			public static readonly String MissingColumnsError = @"The following columns are missing from your load file: {0}.";
			public static readonly String ExtraColumnsError = @"The following column(s) are not valid for the {0} import object: {1}.";
			public static readonly String EmptyFileError = @"Unable to parse File. Please make sure your data starts on the first line of the file and that it includes all of the columns listed in the template for your object.";
			public static readonly String NotValidHistoryObjectTypeGuidError = "Not a valid History Object type Guid.";
			public static readonly String CreateHistoryRecordError = "An error occured when creating history record.";
			public static readonly String UnableToReadColumnsError = @"Unable to parse columns. Please make sure your data is on the first line of the file and that it includes all of the columns listed in the template for your object.";
			public static readonly String NotValidErrorsObjectTypeGuidError = "Not a valid Job Error Object type Guid.";
			public static readonly String CreateJobErrorRecordError = "An error occured when creating a Job Error record.";
			public static readonly String QueryJobErrrorsError = "An error while querying job errors.";
			public static readonly String ClearJobFileFieldError = "An error while clearing the Export Job's file field.";
			public static readonly String ZeroUsersInEnvironmentError = "Zero users found.";
			public static readonly String ApplicationInstallationFailure = "Installation aborted: This application can only be installed successfully in one workspace.";
			public static readonly String RetrieveExistingUserLoginProfileError = "An error occured when retrieving existing user login profile.";
			public static readonly String FileContainsColumnsButNoMetaDataError = "This file has columns but does not seem to have any data to process.";
			public static readonly String ImportQueueManagerPopulatingImportWorkerQueueError = "Unable to insert lines {0}-{1} into the worker queue.";
			public static readonly String ImportQueueManagerLineNumberError = "Unable to import line number {0}. {1}";
			public static readonly String AdminObjectSerializationError = "An error occured when serializing AdminObject.";
			public static readonly String AdminObjectDeSerializationError = "An error occured when de-serializing AdminObject.";
			public static readonly String GeneralDeSerializationError = "An error occured when de-serializing this Object.";
			public static readonly String QueryGroupArtifactIdsUserIsPartOfError = "An error occured when querying for groups the user is part of.";
			public static readonly String QueryGroupsNamesForArtifactIdsError = "An error occured when querying for groups names for a Group ArtifactId list.";
			public static readonly String QueryGroupNamesUserIsPartOfError = "An error occured when querying for group names the user is part of.";
			public static readonly String CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsError = "An error occured when creating export worker results table if it doesn't already exists.";
			public static readonly String AddKeywordsNotesAuthGroupsDataToUserAdminObjectError = "An error occured when adding keywords and notes, auth and group data to UserAdminObject.";
			public static readonly String InsertIntoExportWorkerResultsTableError = @"An error occured when inserting into Export Worker Results table.";
			public static readonly String ProcessAllUsersInExportWorkerBatchError = @"An error occured when processing all users in the Export Worker Batch.";
			public static readonly String CountImportWorkerRowsError = @"An error occured while counting rows in import worker table.";
			public static readonly String UpdateQueueStatusError = @"An error occured while the Queue Status.";
			public static readonly String DeleteQueueRecordError = @"An error occured while the Deleting the Queue Records.";
			public static readonly String QueryImportManagerQueueError = @"An error occured while querying the Import Manager Queue Table.";
			public static readonly String CreateImportErrorsError = @"An error occured while recording import errors.";
			public static readonly String UpdateExportJobNumberOfPreparedRecordsError = @"An error occured while updating the number of prepared records for the export job.";
			public static readonly String ZeroRowsInExportWorkerResultsTableError = @"There are zero rows when creating an export file.";
			public static readonly String QueryBatchFromExportWorkerResultsTableError = @"An error occured when querying for a batch from export worker results table.";
			public static readonly String WritingColumnHeadersToExportFileError = @"An error occured when writing column headers to the Export file.";
			public static readonly String WritingResultRecordsToExportFileError = @"An error occured when writing result records to the Export file.";
			public static readonly String InvalidAdminObjectTypeError = @"This Admin object type is not supported by the application.";
			public static readonly String ExportColumnDataError = @"An error occured when querying for object data to be written into export file.";
			public static readonly String ExportFileUploadError = @"An error occured when uploading export file to Export Utility Job.";
			public static readonly String AttachFileToExportJobError = @"An error occured when attaching export file to Export Utility Job.";
			public static readonly String RetrieveFieldArtifactIdByGuidError = @"An error occured when retrieving field artifact id by guid.";
			public static readonly String QuerySystemAdminGroupError = @"An error occured while querying the system admin group.";
			public static readonly String UserQueryError = @"An error occured while querying the user.";
			public static readonly String RetrieveNextRecordInExportManagerQueueTableError = @"An error occured when retrieving next record in export manager queue table.";
			public static readonly String ImportWorkerLineNumberError = @"Unable to process line number {0}. {1)";
			public static readonly String RetrieveNextBatchOfRecordsInExportWorkerQueueTableError = @"An error occured when retrieving next batch of records in export worker queue table.";
			public static readonly String RemovingJobFromExportManagerQueueError = @"An error occured when removing job from export manager queue table.";
			public static readonly String CheckIfExportWorkersAreStillWorkingOnExportJobInExportWorkerQueueTableError = @"An error occured when checking if workers are still working on an export job in export worker queue table.";
			public static readonly String CreateExportFileError = @"An error occured when creating export file.";
			public static readonly String DeleteTempExportFileError = @"An error occured when deleting temp export file.";
			public static readonly String DropExportWorkerResultsTableError = @"An error occured when dropping export worker results table.";
			public static readonly String ResetUnfinishedJobsInExportManagerQueueTableError = @"An error occured when resetting unfinished jobs in export manager queue table.";
			public static readonly String ResetUnfinishedJobsInExportWorkerQueueTableError = @"An error occured when resetting unfinished jobs in export worker queue table.";
			public static readonly String UnsupportedFileType = @"Only the following file types are supported: {0}.";
			public static readonly String SetPropertiesToBeUsedInCaseOfAnExceptionInExportManagerAgentError = @"An error occured when setting properties in export manager agent.";
			public static readonly String SetPropertiesToBeUsedInCaseOfAnExceptionInExportWorkerAgentError = @"An error occured when setting properties in export worker agent.";
			public static readonly String UpdateExportJobStatusError = @"An error occured when updating export job status.";
			public static readonly String RetrieveExportJobStatusError = @"An error occured when retrieving export job status.";
			public static readonly String CheckIfUserIsAdminError = @"An error occured while checking to see if user is in the System Administrator's group.";
			public static readonly String CreateExportJobErrorRecordError = @"An error occured when creating export job error record.";
			public static readonly String RemoveBatchFromExportWorkerQueueTableError = @"An error occured when removing batch of records from export worker queue table..";
			public static readonly String DropExportWorkerBatchTableError = @"An error occured when drop export worker batch table.";
			public static readonly String ProcessingBatchOfRecordsInExportWorkerQueueTableError = @"An error occured when processing batch of records in export worker queue table.";
			public static readonly String ExportWorkerAgentError = @"An error occured in export worker agent.";
			public static readonly String IsAgentOffHoursError = @"An error occured when checking if agent is in off hours.";
			public static readonly String RetrieveOffHoursError = @"An error occured when retrieving off hours.";
			public static readonly String InsertUserDataIntoExportWorkerResultsTableError = @"An error occured when inserting user data into export worker results table.";
			public static readonly String ResetUnfishedExportWorkerJobsError = @"An error occured when resetting unfished jobs in export worker queue table.";
			public static readonly String RetrieveNextBatchInExportWorkerQueueError = @"An error occured when retrieving batch of records in export worker queue table.";
			public static readonly String InvalidEmailAddresses = @"The following email addresses are invalid: {0}.";
			public static readonly String UnableToExportObject = @"Unable to export object with the following attributes: {0}";
			public static readonly String UnableToSerializeUser = @"Serialization error. Unable to export object with the following attributes. {0}";
			public static readonly String UnableToInsertExportRecordsIntoWorkerQueue = @"Unable to insert the following users into the worker queue: {0}";
			public static readonly String UnableToInsertUserArtifactIDsIntoWorkerQueue = @"Error While inserting userIDs into worker queue";
			public static readonly String UnsupportedRelativityVersion = @"This application can only be installed in versions of Relativity equal to or greater than {0}.";
			public static readonly String UserAlreadyExists = @"The user with email address {0} is already present in this instance of Relativity.";
			public static readonly String UserPartiallyImportedPrepend = @"An error occured and this user may have only been partially imported.  Please validate the users information and retry the import if necessary. ";
			public static readonly String NotSupportedJobDeletion = @"The current Job cannot be deleted as it is currently being processed and has not completed.";
			public static readonly String NotSupportedJobEdit = @"The current Job cannot be editted as it is currently being processed and has not completed.";
			public static readonly String ExportUtilityJobDeletionDependencies = "At least one Export Utility Job record cannot be deleted as dependencies currently exist.";
			public static readonly String ImportUtilityJobDeletionDependencies = "At least one Import Utility Job record cannot be deleted as dependencies currently exist.";
			public static readonly String ApplicationCheckforWorkspaceInstallation = "An error occured while attempting to determine how many Workspaces this application is installed in.";
			public static readonly String ErrorQueryingJobErrors = "An error occured while querying job errors.";
			public static readonly String ApplicationPreUninstallRemoveWorkspaceEntries = @"An error occured while attempting to delete Workspace records from the {0} table in EDDS.";
			public static readonly String ApplicationPreUninstallDropTable = @"An error occured while attempting to drop the {0} application table in EDDS.";
		}

		public class Buttons
		{
			public const String SUBMIT = "Submit";
			public const String CANCEL = "Cancel";
			public const String VALIDATE = "Validate";
			public const String DOWNLOAD_FILE = "Download File";
		}

		public class Status
		{
			public class Queue
			{
				public const Int32 ERROR = -1;
				public const Int32 NOT_STARTED = 0;
				public const Int32 IN_PROGRESS = 1;
				public const Int32 COMPLETE = 2;
				public const Int32 CANCELLATION_REQUESTED = 3;
				public const Int32 WAITING_FOR_WORKERS_TO_FINISH = 4;
				public const Int32 FINISHING_JOB = 5;
			}

			public class Job
			{
				public const String NEW = "New";
				public const String SUBMITTED = "Submitted";
				public const String IN_PROGRESS = "In Progress";
				public const String IN_PROGRESS_MANAGER = "In Progress - Manager";
				public const String IN_PROGRESS_WORKER = "In Progress - Worker";
				public const String COMPLETED = "Completed";
				public const String COMPLETED_WITH_ERRORS = "Completed With Errors";
				public const String COMPLETED_MANAGER = "Manager Completed. Waiting for Worker.";
				public const String CANCELREQUESTED = "Cancel Requested";
				public const String CANCELLED = "Cancelled";
				public const String ERROR = "Error";
				public const String RETRY = "Error Occured, Retrying";
			}

			public class JobErrors
			{
				public const String COMPLETED = "Completed";
				public const String SKIPPED = "Skipped";
				public const String ERROR = "Error";
				public static readonly String QueryUsersError = "An error occured when querying for users.";
			}
		}

		public class ImportUtilityJob
		{
			public class JobType
			{
				public const String VALIDATE = "Validate";
				public const String VALIDATE_SUBMIT = "Validate and Submit";
			}

			public enum ErrorType
			{
				JobLevel = 0,
				FileLevel = 1,
				DataLevel = 2
			}
		}

		public class TestCategories
		{
			public const String EXPORT_USERS = "Export Users";
		}

		public class RegExPatterns
		{
			public static readonly String EmailAddress = @"^(\s)*(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)(\s)*((\s)*;(\s)*(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*))*(\s)*(;)?(\s)*$";
			public static readonly String DigitRange = @"[0-200]";
		}

		public static readonly String ViolationDelimiter = ";";
		public static readonly Int32 MinimumPasswordAge = 0;
		public static readonly Int32 MaximumPasswordAget = 1000;

		public class DelmitedFileColumnNames
		{
			public class UserAdminObject
			{
				public static readonly String FirstName = "FirstName";
				public static readonly String LastName = "LastName";
				public static readonly String EmailAddress = "EmailAddress";
				public static readonly String Keywords = "Keywords";
				public static readonly String Notes = "Notes";
				public static readonly String Groups = "Groups";
				public static readonly String Type = "Type";
				public static readonly String Client = "Client";
				public static readonly String RelativityAccess = "RelativityAccess";
				public static readonly String DocumentSkip = "DocumentSkip";
				public static readonly String BetaUser = "BetaUser";
				public static readonly String ChangeSettings = "ChangeSettings";
				public static readonly String KeyboardShortcuts = "KeyboardShortcuts";
				public static readonly String ItemListPageLength = "ItemListPageLength";
				public static readonly String DefaultSelectedFileType = "DefaultSelectedFileType";
				public static readonly String SkipDefaultPreference = "SkipDefaultPreference";
				public static readonly String EnforceViewerCompatibility = "EnforceViewerCompatibility";
				public static readonly String AdvancedSearchPublicByDefault = "AdvancedSearchPublicByDefault";
				public static readonly String NativeViewerCacheAhead = "NativeViewerCacheAhead";
				public static readonly String ChangeDocumentViewer = "ChangeDocumentViewer";
				public static readonly String DocumentViewer = "DocumentViewer";
				public static readonly String UserMustChangePasswordOnNextLogin = "UserMustChangePasswordOnNextLogin";
				public static readonly String CanChangePassword = "CanChangePassword";
				public static readonly String MaximumPasswordAgeInDays = "MaximumPasswordAgeInDays";
			}

			public class LoginMethod
			{
				public static readonly String WindowsAccount = "WindowsAccount";
				public static readonly String TwoFactorMode = "TwoFactorMode";
				public static readonly String TwoFactorInfo = "TwoFactorInfo";
			}
		}

		public class Sql
		{
			public class ColumnsNames
			{
				public class ImportManagerQueue
				{
					public static readonly String ID = "ID";
					public static readonly String AgentID = "AgentID";
					public static readonly String JobID = "JobID";
					public static readonly String WorkspaceArtifactID = "WorkspaceArtifactID";
					public static readonly String ObjectType = "ObjectType";
					public static readonly String JobType = "JobType";
					public static readonly String QueueStatus = "QueueStatus";
					public static readonly String Priority = "Priority";
					public static readonly String ResourceGroupID = "ResourceGroupID";
					public static readonly String CreatedBy = "CreatedBy";
					public static readonly String TimeStampUTC = "TimeStampUTC";
					public static readonly String WorkspaceName = "WorkspaceName";
				}

				public class ImportWorkerQueue
				{
					public static readonly String ID = "ID";
					public static readonly String AgentID = "AgentID";
					public static readonly String JobID = "JobID";
					public static readonly String WorkspaceArtifactID = "WorkspaceArtifactID";
					public static readonly String ObjectType = "ObjectType";
					public static readonly String JobType = "JobType";
					public static readonly String MetaData = "MetaData";
					public static readonly String ImportRowID = "ImportRowID";
					public static readonly String QueueStatus = "QueueStatus";
					public static readonly String ResourceGroupID = "ResourceGroupID";
					public static readonly String Priority = "Priority";
					public static readonly String TimeStampUTC = "TimeStampUTC";
				}

				public class ExportManagerQueue
				{
					public static readonly String TableRowId = "ID";
					public static readonly String AgentId = "AgentID";
					public static readonly String WorkspaceArtifactId = "WorkspaceArtifactID";
					public static readonly String ExportJobArtifactId = "ExportJobArtifactID";
					public static readonly String ObjectType = "ObjectType";
					public static readonly String Priority = "Priority";
					public static readonly String WorkspaceResourceGroupArtifactId = "WorkspaceResourceGroupArtifactId";
					public static readonly String QueueStatus = "QueueStatus";
					public static readonly String ResultsTableName = "ResultsTableName";
					public static readonly String TimeStampUtc = "TimeStampUTC";
					public static readonly String WorkspaceName = "WorkspaceName";
				}

				public class ExportWorkerQueue
				{
					public static readonly String TableRowId = "ID";
					public static readonly String AgentId = "AgentID";
					public static readonly String WorkspaceArtifactId = "WorkspaceArtifactID";
					public static readonly String ExportJobArtifactId = "ExportJobArtifactID";
					public static readonly String ObjectType = "ObjectType";
					public static readonly String Priority = "Priority";
					public static readonly String WorkspaceResourceGroupArtifactId = "WorkspaceResourceGroupArtifactId";
					public static readonly String QueueStatus = "QueueStatus";
					public static readonly String ArtifactId = "ArtifactID";
					public static readonly String ResultsTableName = "ResultsTableName";
					public static readonly String MetaData = "MetaData";
					public static readonly String TimeStampUtc = "TimeStampUTC";
				}

				public class ExportWorkerQueueBatch
				{
					public static readonly String TableRowId = "ID";
				}

				public class ExportWorkerResultsTable
				{
					public static readonly String TableRowId = "ID";
					public static readonly String AgentId = "AgentID";
					public static readonly String WorkspaceArtifactId = "WorkspaceArtifactID";
					public static readonly String ExportJobArtifactId = "ExportJobArtifactID";
					public static readonly String ObjectType = "ObjectType";
					public static readonly String WorkspaceResourceGroupArtifactId = "WorkspaceResourceGroupArtifactId";
					public static readonly String QueueStatus = "QueueStatus";
					public static readonly String MetaData = "MetaData";
					public static readonly String TimeStampUtc = "TimeStampUTC";
				}
			}
		}

		public static readonly String CommaSeparator = ",";
		public static readonly String SemiColonSeparator = ";";

		public class Rsapi
		{
			public static readonly String GroupArtifactIdField = "ArtifactID";
		}

		public class ExportFile
		{
			public const String EXPORT_FILE_EXTENSION = ".csv";
			public static readonly String ExportFileNameDateTimeFormat = @"yyyy-dd-M--HH-mm-ss";
			public static readonly String ExportFileNameWithExtension = @"AdminMigrationUtility_Export_{0}_{1}_{2}" + EXPORT_FILE_EXTENSION;
			public static readonly String ExportFileFolderName = "AdminMigrationUtility";
		}

		public enum SupportedImportFileTypes
		{
			csv = 0
		}

		public static readonly String GuidReplacement = "[GUID]";
		public static readonly String EmailSubject = "Your Relativity {0} Job has completed";
		public static readonly String EmailBody = "An {0} Utility Job has completed with the following details.<br />Job Name: {1}<br />Workspace Name: {2}<br />Workspace ArtifactID: {3}<br />Status: {4}";

		public class Configuration
		{
			public class Section
			{
				public static readonly String Agents = "kCura.EDDS.Agents";
				public static readonly String Notification = "kCura.Notification";
			}

			public class Name
			{
				public static readonly String EmailFrom = "EmailFrom";
				public static readonly String AgentOffHourStartTime = "AgentOffHourStartTime";
				public static readonly String AgentOffHourEndTime = "AgentOffHourEndTime";
				public static readonly String SmtpServer = "SMTPServer";
				public static readonly String SmtpUserName = "SMTPUserName";
				public static readonly String SmtpPassword = "SMTPPassword";
				public static readonly String EncryptedSmtpPassword = "EncryptedSMTPPassword";
				public static readonly String SmtpPort = "SMTPPort";
			}
		}
	}
}