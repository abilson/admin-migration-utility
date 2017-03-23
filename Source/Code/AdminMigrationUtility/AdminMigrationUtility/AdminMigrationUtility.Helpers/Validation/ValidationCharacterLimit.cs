using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationCharacterLimit : IValidation
	{
		public Int32 MaxLength { get; protected set; }

		public ValidationCharacterLimit(int maxLength)
		{
			MaxLength = maxLength;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			return await Task.Run(() =>
			{
				if (!String.IsNullOrWhiteSpace(input))
				{
					if (input.Length > MaxLength)
					{
						validationMessage = String.Format(Constant.Messages.Violations.MaxLengthExceeded, MaxLength);
					}
				}
				return validationMessage;
			});
		}
	}
}