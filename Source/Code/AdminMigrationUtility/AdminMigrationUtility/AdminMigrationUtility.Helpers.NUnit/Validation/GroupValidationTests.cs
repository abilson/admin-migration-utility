using AdminMigrationUtility.Helpers.Validation;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class GroupValidationTests
	{
		[Test]
		public async Task VerifyMessageReturnedWhenSingleGroupListNotFound()
		{
			var groupName = "Fake Group";
			var mockGroupRepo = new Mock<IGenericRepository<Group>>();
			FieldValueList<Group> resultingGroupList;
			var groupValidation = new ValidationGroup(new APIOptions(), mockGroupRepo.Object, x => resultingGroupList = x);

			var mockResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>()
			};
			mockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(mockResult);

			var expectedMessage = String.Format(Constant.Messages.Violations.GroupDoesNotExist, groupName);
			var result = await groupValidation.ValidateAsync(groupName);

			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		public async Task VerifyMessageReturnedWhenSingleGroupInMultipleListNotFound()
		{
			var group1 = "Existing Group";
			var group2 = "Fake Group";
			var groupListName = String.Join(Constant.ViolationDelimiter, new List<String> { group1, group2 });
			var mockGroupRepo = new Mock<IGenericRepository<Group>>();
			FieldValueList<Group> resultingGroupList;
			var groupValidation = new ValidationGroup(new APIOptions(), mockGroupRepo.Object, x => resultingGroupList = x);

			var mockResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>
				{
					new Result<Group>
					{
						Artifact = new Group() {
							Name = group1
						}
					}
				}
			};

			mockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(mockResult);

			var expectedMessage = String.Format(Constant.Messages.Violations.GroupDoesNotExist, group2);
			var result = await groupValidation.ValidateAsync(groupListName);

			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		public async Task VerifyMessageReturnedWhenMultipleGroupsNotFound()
		{
			var group1 = "Fake Group";
			var group2 = "Fake Group2";
			var groupListName = String.Join(Constant.ViolationDelimiter, new List<String> { group1, group2 });

			var mockGroupRepo = new Mock<IGenericRepository<Group>>();
			FieldValueList<Group> resultingGroupList;
			var groupValidation = new ValidationGroup(new APIOptions(), mockGroupRepo.Object, x => resultingGroupList = x);

			var mockResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>()
			};
			mockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(mockResult);

			var expectedMessage = String.Format(Constant.Messages.Violations.GroupDoesNotExist, groupListName);
			var result = await groupValidation.ValidateAsync(groupListName);

			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		public async Task VerifyNullReturnWhenGroupExists()
		{
			var groupName = "Existing Group";

			var mockGroupRepo = new Mock<IGenericRepository<Group>>();
			FieldValueList<Group> resultingGroupList;
			var groupValidation = new ValidationGroup(new APIOptions(), mockGroupRepo.Object, x => resultingGroupList = x);

			var mockResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>
				{
					new Result<Group>
					{
						Artifact = new Group() {
							Name = groupName
						}
					}
				}
			};
			mockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(mockResult);

			var result = await groupValidation.ValidateAsync(groupName);

			Assert.AreEqual(null, result);
		}

		[Test]
		public async Task VerifyCallbackPopulatesResultingGroup()
		{
			var groupArtifactID = 1234;
			var groupName = "Existing Group";

			var mockGroupRepo = new Mock<IGenericRepository<Group>>();
			FieldValueList<Group> resultingGroupList = null;
			var groupValidation = new ValidationGroup(new APIOptions(), mockGroupRepo.Object, x => resultingGroupList = x);

			var mockResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>
				{
					new Result<Group>
					{
						Artifact = new Group(groupArtifactID) {
							Name = groupName
						}
					}
				}
			};
			mockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(mockResult);

			await groupValidation.ValidateAsync(groupName);

			Assert.IsTrue(resultingGroupList != null);
			Assert.IsTrue(resultingGroupList.Count == mockResult.Results.Count);
			Assert.IsTrue(resultingGroupList[0].ArtifactID == groupArtifactID);
		}
	}
}