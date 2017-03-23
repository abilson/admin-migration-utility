using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Validation.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationClient : IValidation
	{
		private APIOptions _apiOptions;
		private IGenericRepository<Client> _clientRepository;
		private ClientDelegate _callback;

		public ValidationClient(APIOptions apiOptions, IGenericRepository<Client> repository, ClientDelegate callback)
		{
			_apiOptions = apiOptions;
			_clientRepository = repository;
			_callback = callback;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				var query = new ArtifactQueries();
				var clientQueryResult = await query.GetClientAsync(_apiOptions, _clientRepository, input);
				if (clientQueryResult.Success && clientQueryResult.Results.Count > 0)
				{
					_callback(new Client(clientQueryResult.Results[0].Artifact.ArtifactID));
				}
				else
				{
					validationMessage = String.Format(Constant.Messages.Violations.ClientDoesntExist, input);
				}
			}
			return validationMessage;
		}
	}
}