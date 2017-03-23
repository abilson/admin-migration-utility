using AdminMigrationUtility.Helpers.Models.AdminObject;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using AdminMigrationUtility.Helpers.Validation;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Models.AdminObject
{
	[TestFixture]
	public class UserTests
	{
		public Int32 MockUserArtifactID;
		public Mock<ISqlQueryHelper> MockSqlQueryHelper;
		public Mock<IRsapiRepositoryGroup> MockRSAPIRepositoryGroup;
		public Mock<IGenericRepository<kCura.Relativity.Client.DTOs.User>> MockUserRepo;
		public Mock<IGenericRepository<Client>> MockClientRepo;
		public Mock<IGenericRepository<Group>> MockGroupRepo;
		public Mock<IDBContext> MockDbContext;
		public Mock<ILoginProfileManager> MockLoginManager;
		public Mock<IArtifactQueries> MockArtifactQueries;
		public Mock<IHelper> MockHelper;
		public String MockGroupName;
		public String MockClientName;

		#region Setup

		[SetUp]
		public void Init()
		{
			MockUserArtifactID = 12345;
			MockGroupName = "MockGroupName";
			MockClientName = "MockClientName";

			MockArtifactQueries = new Mock<IArtifactQueries>();
			MockArtifactQueries
				.Setup(x => x.FindUserByEmailAddressAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<String>()))
				.Returns(Task.FromResult((Int32?)null));

			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();

			// Init User Reop
			MockUserRepo = new Mock<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(MockBehavior.Strict);
			MockUserRepo.Setup(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>())).Returns(MockUserArtifactID);

			//Init RDO Repo
			var mockGroupResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>
				{
					new Result<Group>
					{
						Artifact = new Group() {
							Name = MockGroupName
						}
					}
				}
			};
			var mockClientResult = new QueryResultSet<Client>
			{
				Success = true,
				Results = new List<Result<Client>>
				{
					new Result<Client>
					{
						Artifact = new Client() {
							Name = MockClientName
						}
					}
				}
			};

			MockGroupRepo = new Mock<IGenericRepository<Group>>();
			MockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(mockGroupResult);

			MockClientRepo = new Mock<IGenericRepository<Client>>();
			MockClientRepo.Setup(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>())).Returns(mockClientResult);

			MockRSAPIRepositoryGroup = new Mock<IRsapiRepositoryGroup>();
			MockRSAPIRepositoryGroup.Setup(x => x.UserRepository).Returns(MockUserRepo.Object);
			MockRSAPIRepositoryGroup.Setup(x => x.GroupRepository).Returns(MockGroupRepo.Object);
			MockRSAPIRepositoryGroup.Setup(x => x.ClientRepository).Returns(MockClientRepo.Object);

			// Init DB Context
			var numberOfSuccessfullyUpdateRows = 1;
			var mockChoiceResult = new DataTable();
			mockChoiceResult.Columns.Add("ArtifactID", typeof(Int32));
			mockChoiceResult.Rows.Add(1234);
			MockDbContext = new Mock<IDBContext>();
			MockDbContext.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<String>(), It.IsAny<IEnumerable<SqlParameter>>())).Returns(mockChoiceResult);
			MockDbContext.Setup(x => x.ExecuteNonQuerySQLStatement(It.IsAny<String>(), It.IsAny<IEnumerable<SqlParameter>>())).Returns(numberOfSuccessfullyUpdateRows);

			MockSqlQueryHelper
				.Setup(x => x.QueryChoiceArtifactId(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<String>()))
				.Returns(Task.FromResult(mockChoiceResult));

			// Init Login Manager
			var userArtifactID = 12345;
			MockLoginManager = new Mock<ILoginProfileManager>();
			MockLoginManager.Setup(x => x.GetLoginProfileAsync(It.IsAny<Int32>())).Returns(Task.FromResult(new LoginProfile() { UserId = userArtifactID }));
			MockLoginManager.Setup(x => x.SaveLoginProfileAsync(It.IsAny<LoginProfile>())).Returns(Task.FromResult<Task>(null));

			// Init IHelper
			MockHelper = new Mock<IHelper>();
			MockHelper.Setup(x => x.GetServicesManager().CreateProxy<ILoginProfileManager>(It.IsAny<ExecutionIdentity>())).Returns(MockLoginManager.Object);
		}

		#endregion Setup

		#region TearDown

		[TearDown]
		public void Cleanup()
		{
			MockUserArtifactID = 0;
			MockUserRepo = null;
			MockDbContext = null;
			MockLoginManager = null;
		}

		#endregion TearDown

		#region Tests

		[Test]
		public async Task PasswordValidationAutomaticallyAddedAfterValidateAsyncCalled()
		{
			var user = new UserAdminObject();
			var columns = await GetUserDictionary();
			await user.MapObjectColumnsAsync(columns);

			var passwordColumns = (await user.GetColumnsAsync()).Where(x => x.ObjectFieldType == Constant.Enums.ObjectFieldType.PasswordLoginMethod);
			Assert.IsFalse(await ValidationExistsInSelectedColumns(passwordColumns, typeof(ValidationPasswordLoginMethod)));
			await user.ValidateAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockDbContext.Object, MockSqlQueryHelper.Object);
			Assert.IsTrue(await ValidationExistsInSelectedColumns(passwordColumns, typeof(ValidationPasswordLoginMethod)));
		}

		[Test]
		public async Task CodeValidationAutomaticallyAddedAfterValidateAsyncCalled()
		{
			var user = new UserAdminObject();
			var columns = await GetUserDictionary();
			await user.MapObjectColumnsAsync(columns);

			var codeNameColumns = new List<IObjectColumn> { user.Type, user.DocumentSkip, user.DefaultSelectedFileType, user.SkipDefaultPreference, user.DocumentViewer };
			Assert.IsFalse(await ValidationExistsInSelectedColumns(codeNameColumns, typeof(ValidationCodeName)));
			await user.ValidateAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockDbContext.Object, MockSqlQueryHelper.Object);
			Assert.IsTrue(await ValidationExistsInSelectedColumns(codeNameColumns, typeof(ValidationCodeName)));
		}

		[Test]
		public async Task GroupValidationAutomaticallyAddedAfterValidateAsyncCalled()
		{
			var user = new UserAdminObject();
			var columns = await GetUserDictionary();
			await user.MapObjectColumnsAsync(columns);

			var codeNameColumns = new List<IObjectColumn> { user.Groups };
			Assert.IsFalse(await ValidationExistsInSelectedColumns(codeNameColumns, typeof(ValidationGroup)));
			await user.ValidateAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockDbContext.Object, MockSqlQueryHelper.Object);
			Assert.IsTrue(await ValidationExistsInSelectedColumns(codeNameColumns, typeof(ValidationGroup)));
		}

		[Test]
		public async Task NoViolationsReturnedForValidUserModel()
		{
			var user = new UserAdminObject();
			var columns = await GetUserDictionary();
			await user.MapObjectColumnsAsync(columns);

			var validationResult = await user.ValidateAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockDbContext.Object, MockSqlQueryHelper.Object);
			Assert.IsTrue(!validationResult.Any());
			Assert.IsTrue(user.Validated);
		}

		[Test]
		public async Task ViolationsForMultipleFieldsAreReturnedCorrectly()
		{
			var user = new UserAdminObject();
			var columns = await GetUserDictionary();
			await user.MapObjectColumnsAsync(columns);

			//Get the required Fields
			var requiredFields = (await user.GetColumnsAsync()).Where(x => x.Validations.Count(y => y.GetType() == typeof(ValidationNotNull)) > 0);

			//Blank out their Data property
			foreach (var field in requiredFields)
			{
				field.Data = String.Empty;
			}

			// Validate the user
			var validationResults = await user.ValidateAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			// Make sure each blanked out required column exists in the returned validation violations
			foreach (var field in requiredFields)
			{
				Assert.IsTrue(validationResults.FirstOrDefault(x => x.Contains(field.ColumnName)) != null);
			}
		}

		[Test]
		public async Task ViolationReturnedWhenUserNotImported()
		{
			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await CorruptUserValues(userValues);
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			Assert.IsTrue(result != null);
			Assert.IsTrue(result.Any());
		}

		[Test]
		public async Task UserNotCreatedWhenViolationsExist()
		{
			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();

			// Remove Auth Fields to force a violation
			await CorruptUserValues(userValues);

			await user.MapObjectColumnsAsync(userValues);

			await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			MockUserRepo.Verify(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>()), Times.Never());
			MockUserRepo.Verify(x => x.Create(It.IsAny<kCura.Relativity.Client.DTOs.User>()), Times.Never());
		}

		[Test]
		public async Task LoginMethodNotCreatedWhenViolationsExist()
		{
			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();

			// Remove Auth Fields to force a violation
			await CorruptUserValues(userValues);

			await user.MapObjectColumnsAsync(userValues);

			await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			MockLoginManager.Verify(x => x.SaveLoginProfileAsync(It.IsAny<LoginProfile>()), Times.Never());
		}

		[Test]
		public async Task ViolationReturnedForUserWithNullEmailAddress()
		{
			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);
			user.EmailAddress.Data = null;

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			Assert.IsTrue(result.Any(x => x.Contains(Constant.Messages.Violations.NullValue)));
		}


		[Test]
		public async Task UserNotAttemptedToBeImportedWhenUserAlreadyExists()
		{
			MockArtifactQueries
				.Setup(x => x.FindUserByEmailAddressAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<String>()))
				.Returns(Task.FromResult((Int32?)MockUserArtifactID));

			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			var expectedMsg = String.Format(Constant.ErrorMessages.UserAlreadyExists, (String)await user.EmailAddress.GetDataValueAsync());
			Assert.IsTrue(result.Contains(expectedMsg));

			// Create Single should not be called
			MockUserRepo.Verify(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>()), Times.Never);
		}

		[Test]
		public async Task UserIsNotPromptedToDeletePartiallyCreatedUserWhenRsapiCreateSingleSaysUserAlreadyExists()
		{
			MockUserRepo
				.Setup(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>()))
				.Throws(new Exception("The entered E-Mail Address is already associated with a user"));

			MockArtifactQueries
				.SetupSequence(x => x.FindUserByEmailAddressAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<String>()))
				.Returns(Task.FromResult((Int32?)null))                 // First the user doesn't exist
				.Returns(Task.FromResult((Int32?)MockUserArtifactID));  // After Create single throws, we want the 2nd call to find the user

			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			// Violations should be returned
			Assert.IsFalse(result.Any(x => x.Contains(Constant.ErrorMessages.UserPartiallyImportedPrepend)));
		}

		[Test]
		public async Task UserIsPromptedToDeletePartiallyCreatedUserWhenCreateThrowsButUserWasStillPartiallyCreated()
		{
			MockUserRepo
				.Setup(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>()))
				.Throws(new Exception());

			MockArtifactQueries
				.SetupSequence(x => x.FindUserByEmailAddressAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<String>()))
				.Returns(Task.FromResult((Int32?)null))					// First the user doesn't exist
				.Returns(Task.FromResult((Int32?)MockUserArtifactID));	// After Create single throws, we want the 2nd call to find the user

			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			// Violations should be returned
			Assert.IsTrue(result.Any(x => x.Contains(Constant.ErrorMessages.UserPartiallyImportedPrepend)));
		}

		[Test]
		public async Task UserIsPromptedToDeletePartiallyCreatedUserWhenKeywordsAndNotesUpdateThrowsButUserWasStillPartiallyCreated()
		{
			MockSqlQueryHelper
				.Setup(x => x.UpdateKeywordAndNotes(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<String>()))
				.Throws(new Exception());

			MockArtifactQueries
				.SetupSequence(x => x.FindUserByEmailAddressAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<String>()))
				.Returns(Task.FromResult((Int32?)null))                 // First the user doesn't exist
				.Returns(Task.FromResult((Int32?)MockUserArtifactID));  // After UpdateKeywordsAndNotes throws, we want the 2nd call to find the user

			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			// Violations should be returned
			Assert.IsTrue(result.Any(x => x.Contains(Constant.ErrorMessages.UserPartiallyImportedPrepend)));
		}

		[Test]
		public async Task UserIsPromptedToDeletePartiallyCreatedUserWhenSaveLoginMethodThrowsButUserWasStillPartiallyCreated()
		{
			// An exception being thrown from the SaveLoginProfileAsync() method indicates the login method was unable to be saved
			MockLoginManager.Setup(x => x.SaveLoginProfileAsync(It.IsAny<LoginProfile>())).Throws(new Exception());

			MockArtifactQueries
				.SetupSequence(x => x.FindUserByEmailAddressAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<String>()))
				.Returns(Task.FromResult((Int32?)null))                 // First the user doesn't exist
				.Returns(Task.FromResult((Int32?)MockUserArtifactID));  // After SaveLoginMethod throws, we want the 2nd call to find the user

			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			// Violations should be returned
			Assert.IsTrue(result.Any(x => x.Contains(Constant.ErrorMessages.UserPartiallyImportedPrepend)));
		}

		[Test]
		public async Task NoViolationsReturnedWhenUserSuccessfullyImported()
		{
			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			var result = await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			MockUserRepo.Verify(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>()), Times.AtLeastOnce);
			Assert.IsTrue(!result.Any());
		}

		[Test]
		public async Task UserArtifactIDIsSetAfterSuccessfullyImported()
		{
			var user = new UserAdminObject();
			var userValues = await GetUserDictionary();
			await user.MapObjectColumnsAsync(userValues);

			await user.ImportAsync(new APIOptions(), MockRSAPIRepositoryGroup.Object, MockArtifactQueries.Object, MockHelper.Object, MockDbContext.Object, MockSqlQueryHelper.Object);

			Assert.AreEqual(MockUserArtifactID, user.ArtifactId);
		}

		#endregion Tests

		#region PrivateMethods

		private async Task<Dictionary<String, String>> GetUserDictionary()
		{
			return await Task.Run(() =>
			{
				var columns = new Dictionary<String, String>()
				{
				{Constant.DelmitedFileColumnNames.UserAdminObject.FirstName,"John"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.LastName,"Smith"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.EmailAddress,"jsmith@email.com"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.Groups, MockGroupName},
				{Constant.DelmitedFileColumnNames.UserAdminObject.Type,"Internal"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.Client, MockClientName},
				{Constant.DelmitedFileColumnNames.UserAdminObject.RelativityAccess,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.DocumentSkip,"1"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.BetaUser,"false"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.ChangeSettings,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.KeyboardShortcuts,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.ItemListPageLength,"200"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.DefaultSelectedFileType,"Doc"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.SkipDefaultPreference,"1"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.EnforceViewerCompatibility,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.AdvancedSearchPublicByDefault,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.NativeViewerCacheAhead,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.Keywords,"Test Test Test"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.Notes,"Best User"},
				{Constant.DelmitedFileColumnNames.LoginMethod.WindowsAccount,"Random Value"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.CanChangePassword,"true"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.MaximumPasswordAgeInDays,"30"},
				{Constant.DelmitedFileColumnNames.UserAdminObject.UserMustChangePasswordOnNextLogin,"false"}
				};
				return columns;
			});
		}

		private async Task CorruptUserValues(Dictionary<String, String> userFields)
		{
			await Task.Run(() =>
			{
				if (userFields.ContainsKey(Constant.DelmitedFileColumnNames.UserAdminObject.FirstName))
				{
					userFields.Remove(Constant.DelmitedFileColumnNames.UserAdminObject.FirstName);
				}
			});
		}

		private async Task<bool> ValidationExistsInSelectedColumns(IEnumerable<IObjectColumn> columns, Type validationType)
		{
			var passwordValidationIsMissing = false;
			await Task.Run(() =>
			{
				var count = 0;
				var columnArray = columns.ToArray();

				while (count < columnArray.Length && passwordValidationIsMissing == false)
				{
					if (columnArray[count].Validations.Count(x => x.GetType() == validationType) > 0)
					{
						passwordValidationIsMissing = true;
					}
					count++;
				}
			});

			return passwordValidationIsMissing;
		}

		#endregion PrivateMethods
	}
}