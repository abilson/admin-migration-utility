using AdminMigrationUtility.Helpers.SQL;
using AdminMigrationUtility.Helpers.Validation;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class CodeNameValidationTests
	{
		public Mock<ISqlQueryHelper> MockSqlQueryHelper;
		public Mock<IDBContext> MockDbContext;

		#region Setup

		[SetUp]
		public void Init()
		{
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockDbContext = new Mock<IDBContext>();
		}

		#endregion Setup

		#region TearDown

		[TearDown]
		public void Cleanup()
		{
			MockSqlQueryHelper = null;
			MockSqlQueryHelper = null;
		}

		#endregion TearDown

		[Test]
		public async Task VerifyMessageReturnedWhenCodeDoesNotExist()
		{
			var codeTypeName = "UserType";
			var codeName = "Internal";
			kCura.Relativity.Client.DTOs.Choice resultingChoice = null;

			MockSqlQueryHelper
				.Setup(x => x.QueryChoiceArtifactId(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<String>()))
				.Returns(Task.FromResult(new DataTable()));
			var codeNameValdation = new ValidationCodeName(MockSqlQueryHelper.Object, MockDbContext.Object, codeTypeName, x => resultingChoice = x);
			var expectedMessage = String.Format(Constant.Messages.Violations.ChoiceDoesNotExist, codeName, codeTypeName);

			var result = await codeNameValdation.ValidateAsync(codeName);
			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		public async Task VerifyMessageNotReturnedWhenCodeExists()
		{
			var codeTypeName = "UserType";
			var codeName = "Internal";
			kCura.Relativity.Client.DTOs.Choice resultingChoice = null;

			var returnData = new DataTable();
			returnData.Columns.Add("ArtifactID", typeof(Int32));
			returnData.Rows.Add(1234);

			MockSqlQueryHelper
				.Setup(x => x.QueryChoiceArtifactId(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<String>()))
				.Returns(Task.FromResult(returnData));
			var codeNameValdation = new ValidationCodeName(MockSqlQueryHelper.Object, MockDbContext.Object, codeTypeName, x => resultingChoice = x);

			var result = await codeNameValdation.ValidateAsync(codeName);
			Assert.AreEqual(null, result);
		}

		[Test]
		public async Task VerifyCallbackSetsValueOfChoice()
		{
			var codeTypeName = "UserType";
			var codeName = "Internal";
			kCura.Relativity.Client.DTOs.Choice resultingChoice = null;
			var choiceArtifactID = 1234;

			var returnData = new DataTable();
			returnData.Columns.Add("ArtifactID", typeof(Int32));
			returnData.Rows.Add(choiceArtifactID);

			MockSqlQueryHelper
				.Setup(x => x.QueryChoiceArtifactId(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<String>()))
				.Returns(Task.FromResult(returnData));
			var codeNameValdation = new ValidationCodeName(MockSqlQueryHelper.Object, MockDbContext.Object, codeTypeName, x => resultingChoice = x);

			await codeNameValdation.ValidateAsync(codeName);
			Assert.IsTrue(resultingChoice != null);
			Assert.IsTrue(resultingChoice.ArtifactID == choiceArtifactID);
		}
	}
}