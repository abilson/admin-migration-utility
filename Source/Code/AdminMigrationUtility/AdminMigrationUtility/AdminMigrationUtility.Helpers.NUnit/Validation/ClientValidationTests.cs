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
	public class ClientValidationTests
	{
		[Test]
		public async Task MessageReturnedWhenQueryUnsuccessful()
		{
			var clientName = "Random NonExisting Client Name";
			var retVal = new QueryResultSet<Client>() { Success = false };
			Client resultingClient = null;

			var mockClientRepo = new Mock<IGenericRepository<Client>>();
			mockClientRepo.Setup(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>())).Returns(retVal);

			var clientValidation = new ValidationClient(new APIOptions(), mockClientRepo.Object, x => resultingClient = x);

			var expectedResult = String.Format(Constant.Messages.Violations.ClientDoesntExist, clientName);
			var result = await clientValidation.ValidateAsync(clientName);

			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public async Task MessageReturnedWhenClientNotFound()
		{
			var clientName = "Random NonExisting Client Name";
			var retVal = new QueryResultSet<Client>() { Success = true };
			Client resultingClient = null;

			var mockClientRepo = new Mock<IGenericRepository<Client>>();
			mockClientRepo.Setup(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>())).Returns(retVal);

			var clientValidation = new ValidationClient(new APIOptions(), mockClientRepo.Object, x => resultingClient = x);

			var expectedResult = String.Format(Constant.Messages.Violations.ClientDoesntExist, clientName);
			var result = await clientValidation.ValidateAsync(clientName);

			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public async Task NullReturnedWhenClientPassesValidation()
		{
			var clientName = "Random Existing Client Name";
			Client resultingClient = null;
			var mockResult = new QueryResultSet<Client>
			{
				Success = true,
				Results = new List<Result<Client>>
				{
					new Result<Client>
					{
						Artifact = new Client() {
							Name = clientName
						}
					}
				}
			};

			var mockClientRepo = new Mock<IGenericRepository<Client>>();
			mockClientRepo.Setup(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>())).Returns(mockResult);

			var clientValidation = new ValidationClient(new APIOptions(), mockClientRepo.Object, x => resultingClient = x);

			var result = await clientValidation.ValidateAsync(clientName);

			Assert.AreEqual(null, result);
		}

		[Test]
		public async Task VerifyCallbackSetsValueOfClient()
		{
			var clientArtifactID = 1234;
			var clientName = "Random Existing Client Name";
			Client resultingClient = null;
			var mockResult = new QueryResultSet<Client>
			{
				Success = true,
				Results = new List<Result<Client>>
				{
					new Result<Client>
					{
						Artifact = new Client(clientArtifactID) {
							Name = clientName
						}
					}
				}
			};

			var mockClientRepo = new Mock<IGenericRepository<Client>>();
			mockClientRepo.Setup(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>())).Returns(mockResult);

			var clientValidation = new ValidationClient(new APIOptions(), mockClientRepo.Object, x => resultingClient = x);

			await clientValidation.ValidateAsync(clientName);

			Assert.IsTrue(resultingClient != null);
			Assert.IsTrue(resultingClient.ArtifactID == clientArtifactID);
		}
	}
}