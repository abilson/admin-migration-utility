using AdminMigrationUtility.Helpers.SQL;
using AdminMigrationUtility.Helpers.Validation.Interfaces;
using Relativity.API;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationCodeName : IValidation
	{
		public IDBContext EddsDbContext { get; protected set; }
		public String CodeTypeName { get; protected set; }

		private ChoiceDelegate _callback;
		private ISqlQueryHelper _sqlQueryHelper;

		public ValidationCodeName(ISqlQueryHelper sqlQueryHelper, IDBContext eddsDbContext, String codeTypeName, ChoiceDelegate callback)
		{
			EddsDbContext = eddsDbContext;
			CodeTypeName = codeTypeName;
			_callback = callback;
			_sqlQueryHelper = sqlQueryHelper;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			if (!String.IsNullOrWhiteSpace(input))
			{
				try
				{
					var queryResults = await _sqlQueryHelper.QueryChoiceArtifactId(EddsDbContext, CodeTypeName, input);
					if (queryResults != null && queryResults.Rows.Count > 0)
					{
						_callback(new kCura.Relativity.Client.DTOs.Choice((Int32)queryResults.Rows[0]["ArtifactID"]));
					}
					else
					{
						validationMessage = String.Format(Constant.Messages.Violations.ChoiceDoesNotExist, input, CodeTypeName);
					}
				}
				catch (Exception ex)
				{
					validationMessage = String.Format(Constant.Messages.Violations.Exception, $"querying the {input} choice for code type {CodeTypeName}", ex);
				}
			}
			return validationMessage;
		}
	}
}