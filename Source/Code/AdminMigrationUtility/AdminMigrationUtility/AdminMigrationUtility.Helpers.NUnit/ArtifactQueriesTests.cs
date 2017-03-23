using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Choice = kCura.Relativity.Client.DTOs.Choice;
using User = kCura.Relativity.Client.DTOs.User;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class ArtifactQueriesTests
	{
		public IArtifactQueries Sut { get; set; }
		public Mock<IGenericRepository<User>> MockUserRepository { get; set; }
		public Mock<IGenericRepository<Group>> MockGroupRepository { get; set; }
		public Mock<IGenericRepository<RDO>> MockRdoRepository { get; set; }
		public Mock<IGenericRepository<Workspace>> MockWorkspaceRepository { get; set; }
		public APIOptions RsapiApiOptions { get; set; }

		#region Setup and TearDown

		[SetUp]
		public void Setup()
		{
			Sut = new ArtifactQueries();
			MockUserRepository = new Mock<IGenericRepository<User>>();
			MockGroupRepository = new Mock<IGenericRepository<Group>>();
			MockRdoRepository = new Mock<IGenericRepository<RDO>>();
			MockWorkspaceRepository = new Mock<IGenericRepository<Workspace>>();
			RsapiApiOptions = new APIOptions();
		}

		[TearDown]
		public void TearDown()
		{
			Sut = null;
			MockUserRepository = null;
			MockGroupRepository = null;
			MockRdoRepository = null;
			MockWorkspaceRepository = null;
			RsapiApiOptions = null;
		}

		#endregion Setup and TearDown

		#region Tests

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public async Task QueryAllUsersTest_ReturnsAtleastOneUserAsync()
		{
			//Arrage
			MockUserRepository_Query_ReturnsTwoUsers();

			//Act
			var userArtifacts = await Sut.QueryAllUsersAsync(RsapiApiOptions, MockUserRepository.Object);

			//Assert
			Assert.That(userArtifacts.ToList().Count, Is.GreaterThan(0));
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public async Task QueryAllUsersTest_ReturnsTwoUsersAsync()
		{
			//Arrage
			MockUserRepository_Query_ReturnsTwoUsers();

			//Act
			var userArtifacts = await Sut.QueryAllUsersAsync(RsapiApiOptions, MockUserRepository.Object);

			//Assert
			Assert.That(userArtifacts.ToList().Count, Is.EqualTo(2));
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void QueryAllUsersTest_ReturnsZeroUsers()
		{
			//Arrage
			MockUserRepository_Query_ReturnsZeroUsers();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.QueryAllUsersAsync(RsapiApiOptions, MockUserRepository.Object));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.QueryUsersError));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.NoUsersFound));
			VerifyUserRepositoryQueryWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void QueryAllUsersTest_Fails()
		{
			//Arrage
			MockUserRepository_Query_ThrowsError();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.QueryAllUsersAsync(RsapiApiOptions, MockUserRepository.Object));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.QueryUsersError));
			VerifyUserRepositoryQueryWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void UpdateRdoTextFieldValueAsyncTest_SuccessfullyUpdates()
		{
			//Arrage
			MockRdoRepository_UpdateSingle_Works();

			//Act
			//Assert
			Assert.DoesNotThrow(async () => await Sut.UpdateRdoTextFieldValueAsync(
				rsapiApiOptions: RsapiApiOptions,
				workspaceArtifactId: -1,
				rdoRepository: MockRdoRepository.Object,
				rdoArtifactId: 123,
				jobObjectGuid: Guid.Empty,
				textFieldGuid: Guid.Empty,
				fieldValue: "textFieldValue"));
			VerifyRdoRepositoryUpdateSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void UpdateRdoTextFieldValueAsyncTest_UpdateSingleThrowsError()
		{
			//Arrage
			MockRdoRepository_UpdateSingle_ThrowsError();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.UpdateRdoTextFieldValueAsync(
			 rsapiApiOptions: RsapiApiOptions,
			 workspaceArtifactId: -1,
			 rdoRepository: MockRdoRepository.Object,
			 rdoArtifactId: 123,
			 jobObjectGuid: Guid.Empty,
			 textFieldGuid: Guid.Empty,
			 fieldValue: "textFieldValue"));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining("UpdateSingle"));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RdoTextFieldUpdateError));
			VerifyRdoRepositoryUpdateSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public async Task RetrieveRdoTextFieldValueAsyncTest_SuccessfullyReturnsTextFieldValue()
		{
			//Arrage
			Guid textFieldGuid = Guid.NewGuid();
			MockRdoRepository_ReadSingle_ReturnsRdo(textFieldGuid);

			//Act
			//Assert
			String rdoTextFieldValue = await Sut.RetrieveRdoTextFieldValueAsync(
				rsapiApiOptions: RsapiApiOptions,
				workspaceArtifactId: 123,
				rdoRepository: MockRdoRepository.Object,
				rdoArtifactId: 123,
				textFieldGuid: textFieldGuid);
			Assert.That(rdoTextFieldValue, Is.Not.Null);
			VerifyRdoRepositoryReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveRdoTextFieldValueAsyncTest_ReadSingleThrowsError()
		{
			//Arrage
			Guid textFieldGuid = Guid.NewGuid();
			MockRdoRepository_ReadSingle_ThrowsError();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveRdoTextFieldValueAsync(
				rsapiApiOptions: RsapiApiOptions,
				workspaceArtifactId: 123,
				rdoRepository: MockRdoRepository.Object,
				rdoArtifactId: 123,
				textFieldGuid: textFieldGuid));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining("ReadSingle"));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RdoTextFieldQueryError));
			VerifyRdoRepositoryReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public async Task RetrieveWorkspaceResourcePoolArtifactIdAsyncTest_SuccessfullyReturnsPositiveInteger()
		{
			//Arrage
			MockWorkspaceRepository_ReadSingle_ReturnsPositiveInteger();

			//Act
			//Assert
			Int32 resourcePoolArtifactId = await Sut.RetrieveWorkspaceResourcePoolArtifactIdAsync(
				RsapiApiOptions,
				-1,
				MockWorkspaceRepository.Object,
				workspaceArtifactId: 123);
			Assert.That(resourcePoolArtifactId, Is.GreaterThan(0));
			VerifyWorkspaceRepositoryReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveWorkspaceResourcePoolArtifactIdAsyncTest_ThrowsError()
		{
			//Arrage
			MockWorkspaceRepository_ReadSingle_ThrowsError();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveWorkspaceResourcePoolArtifactIdAsync(
				RsapiApiOptions,
				-1,
				MockWorkspaceRepository.Object,
				workspaceArtifactId: 123));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdError));
			Assert.That(adminMigrationUtilityException.ToString(), Is.Not.StringContaining(Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdNullError));
			VerifyWorkspaceRepositoryReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveWorkspaceResourcePoolArtifactIdAsyncTest_ResourceIdIsNullableInteger()
		{
			//Arrage
			MockWorkspaceRepository_ReadSingle_ReturnsNullableInteger();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveWorkspaceResourcePoolArtifactIdAsync(
				RsapiApiOptions,
				-1,
				MockWorkspaceRepository.Object,
				workspaceArtifactId: 123));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdError));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RetrieveWorkspaceResourcePoolArtifactIdNullError));
			VerifyWorkspaceRepositoryReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void CreateJobErrorRecordAsyncTest_ForExportJob_UserJobType_SuccessfullyCreated()
		{
			//Arrage
			MockRdoRepository_CreateSingle_Works(newRdoArtifactId: 123);

			//Act
			//Assert
			Assert.DoesNotThrow(async () => await Sut.CreateJobErrorRecordAsync(
			 RsapiApiOptions,
			 123,
			 MockRdoRepository.Object,
			 Constant.Guids.ObjectType.ExportUtilityJobErrors,
			 123,
			 Constant.Enums.SupportedObjects.User.ToString(),
			 "status",
			 "details"
			 ));
			VerifyRdoRepository_CreateSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void CreateJobErrorRecordAsyncTest_ForImportJob_UserJobType_SuccessfullyCreated()
		{
			//Arrage
			MockRdoRepository_CreateSingle_Works(newRdoArtifactId: 123);

			//Act
			//Assert
			Assert.DoesNotThrow(async () => await Sut.CreateJobErrorRecordAsync(
			 RsapiApiOptions,
			 123,
			 MockRdoRepository.Object,
			 Constant.Guids.ObjectType.ImportUtilityJobErrors,
			 123,
			 Constant.Enums.SupportedObjects.User.ToString(),
			 "status",
			 "details"
			 ));
			VerifyRdoRepository_CreateSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void CreateJobErrorRecordAsyncTest_ForExportJob_UserJobType_CreateSingleThrowsError()
		{
			//Arrage
			MockRdoRepository_CreateSingle_ThrowsException();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.CreateJobErrorRecordAsync(
			RsapiApiOptions,
			123,
			MockRdoRepository.Object,
			Constant.Guids.ObjectType.ExportUtilityJobErrors,
			123,
			Constant.Enums.SupportedObjects.User.ToString(),
			"status",
			"details"
			));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.CreateJobErrorRecordError));
			VerifyRdoRepository_CreateSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void CreateJobErrorRecordAsyncTest_ForImportJob_UserJobType_CreateSingleThrowsError()
		{
			//Arrage
			MockRdoRepository_CreateSingle_ThrowsException();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.CreateJobErrorRecordAsync(
			RsapiApiOptions,
			123,
			MockRdoRepository.Object,
			Constant.Guids.ObjectType.ExportUtilityJobErrors,
			123,
			Constant.Enums.SupportedObjects.User.ToString(),
			"status",
			"details"
			));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.CreateJobErrorRecordError));
			VerifyRdoRepository_CreateSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public void QueryGroupArtifactIdsUserIsPartOfAsyncTest_SuccessfullyReturnsUsers(Int32 groupCount)
		{
			//Arrage
			MockUserRepository_ReadSingle_ReturnsUser(groupCount: groupCount);
			const Int32 eddsWorkspaceArtifactId = 123;
			const Int32 userArtifactId = 123;
			List<Int32> groupArtifactIdList = null;

			//Act
			//Assert
			Assert.That(async () =>
			groupArtifactIdList = await Sut.QueryGroupArtifactIdsUserIsPartOfAsync(
				RsapiApiOptions,
				eddsWorkspaceArtifactId,
				MockUserRepository.Object,
				userArtifactId
				),
			Throws.Nothing);
			Assert.That(groupArtifactIdList, Is.Not.Null);
			Assert.That(groupArtifactIdList.Count, Is.EqualTo(groupCount));
			VerifyUserRepository_ReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void QueryGroupArtifactIdsUserIsPartOfAsyncTest_ExceptionWasThrown()
		{
			//Arrage
			MockUserRepository_ReadSingle_ThrowsException();
			const Int32 eddsWorkspaceArtifactId = 123;
			const Int32 userArtifactId = 123;

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.QueryGroupArtifactIdsUserIsPartOfAsync(
				 RsapiApiOptions,
				 eddsWorkspaceArtifactId,
				 MockUserRepository.Object,
				 userArtifactId
				 ));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.QueryGroupArtifactIdsUserIsPartOfError));
			VerifyUserRepository_ReadSingleWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		[TestCase(0)]
		[TestCase(1)]
		[TestCase(5)]
		public void QueryGroupsNamesForArtifactIdsAsyncTest_SuccessfullyReturnsUsers(Int32 groupCount)
		{
			//Arrage
			MockGroupRepository_Query_ReturnsGroups(groupCount: groupCount);
			const Int32 eddsWorkspaceArtifactId = 123;
			List<Int32> groupArtifactIdList = new List<Int32>();
			for (Int32 i = 1; i <= groupCount; i++)
			{
				groupArtifactIdList.Add(i);
			}
			IEnumerable<String> groupNameList = null;

			//Act
			//Assert
			Assert.That(async () =>
			groupNameList = await Sut.QueryGroupsNamesForArtifactIdsAsync(
				RsapiApiOptions,
				eddsWorkspaceArtifactId,
				MockGroupRepository.Object,
				groupArtifactIdList
				),
			Throws.Nothing);
			Assert.That(groupNameList, Is.Not.Null);
			Assert.That(groupNameList.Count, Is.EqualTo(groupCount));
			VerifyGroupRepository_QueryWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void QueryGroupsNamesForArtifactIdsAsyncTest_ExceptionWasThrown()
		{
			//Arrage
			MockGroupRepository_Query_ThrowsException();
			const Int32 eddsWorkspaceArtifactId = 123;
			List<Int32> groupArtifactIdList = new List<Int32>();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.QueryGroupsNamesForArtifactIdsAsync(
				 RsapiApiOptions,
				 eddsWorkspaceArtifactId,
				 MockGroupRepository.Object,
				 groupArtifactIdList
				 ));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining("Query"));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.QueryGroupsNamesForArtifactIdsError));
			VerifyGroupRepository_QueryWasCalled(1);
		}

		#endregion Tests

		#region Mocks

		private void MockUserRepositoryQueryWithUserData(QueryResultSet<User> userQueryResultSet)
		{
			MockUserRepository
				.Setup(x => x.Query(It.IsAny<Query<User>>(), It.IsAny<Int32>()))
				.Returns(userQueryResultSet);
		}

		private void MockUserRepository_Query_ReturnsTwoUsers()
		{
			QueryResultSet<User> userQueryResultSet = new QueryResultSet<User>
			{
				Success = true
			};
			Result<User> userResult1 = new Result<User>
			{
				Artifact = RetrieveSampleUserArtifact("firstName1", "lastName1", "email1@email.com"),
				Success = true
			};
			Result<User> userResult2 = new Result<User>
			{
				Artifact = RetrieveSampleUserArtifact("firstName1", "lastName1", "email1@email.com"),
				Success = true
			};
			userQueryResultSet.Results.Add(userResult1);
			userQueryResultSet.Results.Add(userResult2);

			MockUserRepositoryQueryWithUserData(userQueryResultSet);
		}

		private void MockUserRepository_Query_ReturnsZeroUsers()
		{
			QueryResultSet<User> userQueryResultSet = new QueryResultSet<User>
			{
				Success = true
			};

			MockUserRepositoryQueryWithUserData(userQueryResultSet);
		}

		private void MockUserRepository_Query_ThrowsError()
		{
			QueryResultSet<User> userQueryResultSet = new QueryResultSet<User>
			{
				Success = false
			};

			MockUserRepositoryQueryWithUserData(userQueryResultSet);
		}

		private void MockUserRepository_ReadSingle_ReturnsUser(Int32 groupCount)
		{
			User userDto = new User
			{
				FirstName = "FirstName",
				LastName = "LastName",
				EmailAddress = "email1@email.com",
				Groups = new FieldValueList<Group>()
			};

			for (Int32 i = 1; i <= groupCount; i++)
			{
				Group group = new Group
				{
					Name = $"Group{i}"
				};
				userDto.Groups.Add(group);
			}

			MockUserRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Returns(userDto);
		}

		private void MockGroupRepository_Query_ReturnsGroups(Int32 groupCount)
		{
			QueryResultSet<Group> groupQueryResultSet = new QueryResultSet<Group>
			{
				Success = true
			};

			for (Int32 i = 1; i <= groupCount; i++)
			{
				String groupName = $"Group{i}";

				Result<Group> groupResult = new Result<Group>
				{
					Artifact = RetrieveSampleGroupArtifact(groupName),
					Success = true
				};

				groupQueryResultSet.Results.Add(groupResult);
			}

			MockGroupRepository
				.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>()))
				.Returns(groupQueryResultSet);
		}

		private void MockUserRepository_ReadSingle_ThrowsException()
		{
			MockUserRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Throws<Exception>();
		}

		private void MockGroupRepository_Query_ThrowsException()
		{
			MockGroupRepository
				.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>()))
				.Throws<Exception>();
		}

		private void MockWorkspaceRepository_ReadSingle_ReturnsPositiveInteger()
		{
			Workspace workspaceArtifact = new Workspace
			{
				ResourcePoolID = 123
			};

			MockWorkspaceRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Returns(workspaceArtifact);
		}

		private void MockWorkspaceRepository_ReadSingle_ThrowsError()
		{
			MockWorkspaceRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Throws<Exception>();
		}

		private void MockWorkspaceRepository_ReadSingle_ReturnsNullableInteger()
		{
			Workspace workspaceArtifact = new Workspace
			{
				ResourcePoolID = null
			};

			MockWorkspaceRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Returns(workspaceArtifact);
		}

		private void MockRdoRepository_UpdateSingle_Works()
		{
			MockRdoRepository
				.Setup(x => x.UpdateSingle(It.IsAny<RDO>()));
		}

		private void MockRdoRepository_UpdateSingle_ThrowsError()
		{
			MockRdoRepository
				.Setup(x => x.UpdateSingle(It.IsAny<RDO>()))
				.Throws<Exception>();
		}

		private void MockRdoRepository_ReadSingle_ReturnsRdo(Guid textFieldGuid)
		{
			RDO rdo = new RDO(123)
			{
				Fields = new List<FieldValue>
				{
					new FieldValue("textField")
					{
						Value = "TextFieldValue",
						Guids = new List<Guid> { textFieldGuid }
					}
				}
			};
			MockRdoRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Returns(rdo);
		}

		private void MockRdoRepository_ReadSingle_ThrowsError()
		{
			MockRdoRepository
				.Setup(x => x.ReadSingle(It.IsAny<Int32>()))
				.Throws<Exception>();
		}

		private void MockRdoRepository_CreateSingle_Works(Int32 newRdoArtifactId)
		{
			MockRdoRepository
				.Setup(x => x.CreateSingle(It.IsAny<RDO>()))
				.Returns(newRdoArtifactId);
		}

		private void MockRdoRepository_CreateSingle_ThrowsException()
		{
			MockRdoRepository
				.Setup(x => x.CreateSingle(It.IsAny<RDO>()))
				.Throws<AdminMigrationUtilityException>();
		}

		#endregion Mocks

		#region Verify

		private void VerifyUserRepositoryQueryWasCalled(Int32 timesCalled)
		{
			MockUserRepository
				.Verify(x => x.Query(
					It.IsAny<Query<User>>(),
					It.IsAny<Int32>())
				, Times.Exactly(timesCalled));
		}

		private void VerifyRdoRepositoryUpdateSingleWasCalled(Int32 timesCalled)
		{
			MockRdoRepository
				.Verify(x => x.UpdateSingle(
					It.IsAny<RDO>())
					, Times.Exactly(timesCalled));
		}

		private void VerifyRdoRepositoryReadSingleWasCalled(Int32 timesCalled)
		{
			MockRdoRepository
				.Verify(x => x.ReadSingle(
					It.IsAny<Int32>())
				, Times.Exactly(timesCalled));
		}

		private void VerifyWorkspaceRepositoryReadSingleWasCalled(Int32 timesCalled)
		{
			MockWorkspaceRepository
				.Verify(x => x.ReadSingle(
					It.IsAny<Int32>())
				, Times.Exactly(timesCalled));
		}

		private void VerifyRdoRepository_CreateSingleWasCalled(Int32 timesCalled)
		{
			MockRdoRepository
				.Verify(x => x.CreateSingle(
					It.IsAny<RDO>())
				, Times.Exactly(timesCalled));
		}

		private void VerifyUserRepository_ReadSingleWasCalled(Int32 timesCalled)
		{
			MockUserRepository
				.Verify(x => x.ReadSingle(
					It.IsAny<Int32>())
				, Times.Exactly(timesCalled));
		}

		private void VerifyGroupRepository_QueryWasCalled(Int32 timesCalled)
		{
			MockGroupRepository
				.Verify(x => x.Query(
					It.IsAny<Query<Group>>(),
					It.IsAny<Int32>())
				, Times.Exactly(timesCalled));
		}

		#endregion Verify

		#region TestHelpers

		private User RetrieveSampleUserArtifact(String firstName, String lastName, String emailAddress)
		{
			User userArtifact = new User
			{
				ArtifactTypeName = ArtifactTypeNames.User,
				TextIdentifier = $"{firstName} {lastName}",
				FirstName = firstName,
				LastName = lastName,
				EmailAddress = emailAddress,
				Type = new Choice { Name = "Type" },
				Client = new Client { Name = "Client" },
				RelativityAccess = true,
				DocumentSkip = new Choice { Name = "DocumentSkip" },
				BetaUser = true,
				ChangeSettings = true,
				KeyboardShortcuts = true,
				ItemListPageLength = 12345,
				DefaultSelectedFileType = new Choice { Name = "DefaultSelectedFileType" },
				SkipDefaultPreference = new Choice { Name = "SkipDefaultPreference" },
				EnforceViewerCompatibility = true,
				AdvancedSearchPublicByDefault = true,
				NativeViewerCacheAhead = true,
				CanChangeDocumentViewer = true,
				ShowFilters = true,
				DocumentViewer = new Choice { Name = "DocumentViewer" }
			};

			return userArtifact;
		}

		private Group RetrieveSampleGroupArtifact(String groupName)
		{
			Group groupArtifact = new Group
			{
				ArtifactTypeName = ArtifactTypeNames.Group,
				Name = $"{groupName}"
			};

			return groupArtifact;
		}

		#endregion TestHelpers
	}
}