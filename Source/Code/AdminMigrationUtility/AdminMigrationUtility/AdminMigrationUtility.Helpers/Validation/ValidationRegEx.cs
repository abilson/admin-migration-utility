using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationRegEx : IValidation
	{
		public String RegExPattern { get; protected set; }
		public String ValidationDescription { get; protected set; }

		public ValidationRegEx(String regExPattern, String validationDescription)
		{
			RegExPattern = regExPattern;
			ValidationDescription = validationDescription;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			return await Task.Run(() =>
			{
				if (!String.IsNullOrWhiteSpace(input))
				{
					var match = System.Text.RegularExpressions.Regex.Match(input, RegExPattern);
					if (!match.Success)
					{
						validationMessage = String.Format(Constant.Messages.Violations.RegExMisMatch, ValidationDescription);
					}
				}
				return validationMessage;
			});
		}
	}
}