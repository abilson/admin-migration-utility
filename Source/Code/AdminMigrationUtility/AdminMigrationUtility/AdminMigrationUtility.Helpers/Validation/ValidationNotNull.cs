using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationNotNull : IValidation
	{
		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			await Task.Run(() =>
			{
				if (String.IsNullOrWhiteSpace(input))
				{
					validationMessage = Constant.Messages.Violations.NullValue;
				}
			});
			return validationMessage;
		}
	}
}