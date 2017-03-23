using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Kepler;
using Moq;
using NUnit.Framework;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class AuthenticationHelperTests
	{
		public IAuthenticationHelper Sut { get; set; }
		public Mock<ILoginProfileManager> MockLoginProfileManager { get; set; }

		#region Setup and Teardown

		[SetUp]
		public void Setup()
		{
			MockLoginProfileManager = new Mock<ILoginProfileManager>();
			Sut = new AuthenticationHelper(MockLoginProfileManager.Object);
		}

		[TearDown]
		public void Teardown()
		{
			MockLoginProfileManager = null;
			Sut = null;
		}

		#endregion Setup and Teardown

		#region Tests

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveExistingUserLoginProfileAsyncTest()
		{
			//Arrange
			const Int32 userArtifactId = 123;
			MockGetLoginProfileAsync_ReturnsValidLoginProfile(userArtifactId);
			LoginProfile loginProfile = null;

			//Act
			//Assert
			Assert.That(async () => loginProfile = await Sut.RetrieveExistingUserLoginProfileAsync(userArtifactId), Throws.Nothing);
			Assert.That(loginProfile, Is.Not.Null);
			Assert.That(loginProfile.UserId, Is.EqualTo(userArtifactId));
			VerifyGetLoginProfileAsyncWasCalled(1, userArtifactId);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveExistingUserLoginProfileAsyncTest_WhenNullLoginProfileIsReturned()
		{
			//Arrange
			const Int32 userArtifactId = 123;
			MockGetLoginProfileAsync_ReturnsNullLoginProfile();
			LoginProfile loginProfile = null;

			//Act
			//Assert
			Assert.That(async () => loginProfile = await Sut.RetrieveExistingUserLoginProfileAsync(userArtifactId), Throws.Nothing);
			Assert.That(loginProfile, Is.Null);
			VerifyGetLoginProfileAsyncWasCalled(1, userArtifactId);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void RetrieveExistingUserLoginProfileAsyncTest_WhenExceptionIsThrown()
		{
			//Arrange
			const Int32 userArtifactId = 123;
			MockGetLoginProfileAsync_ThrowsException();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.RetrieveExistingUserLoginProfileAsync(userArtifactId));
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.RetrieveExistingUserLoginProfileError));
			VerifyGetLoginProfileAsyncWasCalled(1, userArtifactId);
		}

		#endregion Tests

		#region Mocks

		public void MockGetLoginProfileAsync_ReturnsValidLoginProfile(Int32 userArtifactId)
		{
			LoginProfile loginProfile = new LoginProfile
			{
				UserId = userArtifactId
			};

			MockLoginProfileManager
				.Setup(x => x.GetLoginProfileAsync(It.IsAny<Int32>()))
				.Returns(Task.FromResult(loginProfile));
		}

		public void MockGetLoginProfileAsync_ReturnsNullLoginProfile()
		{
			MockLoginProfileManager
				.Setup(x => x.GetLoginProfileAsync(It.IsAny<Int32>()))
				.Returns(Task.FromResult((LoginProfile)null));
		}

		public void MockGetLoginProfileAsync_ThrowsException()
		{
			MockLoginProfileManager
				.Setup(x => x.GetLoginProfileAsync(It.IsAny<Int32>()))
				.Throws<Exception>();
		}

		#endregion Mocks

		#region Verify

		public void VerifyGetLoginProfileAsyncWasCalled(Int32 timesCalled, Int32 userArtifactId)
		{
			MockLoginProfileManager
				.Verify(x => x.GetLoginProfileAsync(userArtifactId),
				Times.Exactly(timesCalled));
		}

		#endregion Verify
	}
}