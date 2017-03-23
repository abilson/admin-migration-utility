using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationNumericRange : IValidation
	{
		public Int32 Minimum { get; protected set; }
		public Int32 Maximum { get; protected set; }
		public Boolean Inclusive { get; protected set; }

		public ValidationNumericRange(Int32 minimum, Int32 maximum, Boolean inclusive)
		{
			Minimum = minimum;
			Maximum = maximum;
			Inclusive = inclusive;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			return await Task.Run(() =>
			{
				if (!String.IsNullOrWhiteSpace(input))
				{
					Int32 parsedInput;
					input = input.Trim();
					if (Int32.TryParse(input, out parsedInput))
					{
						if (Inclusive)
						{
							if (parsedInput < Minimum || parsedInput > Maximum)
							{
								validationMessage = String.Format(Constant.Messages.Violations.OutOfNumericRange, Minimum, Maximum);
							}
						}
						else
						{
							if (parsedInput <= Minimum || parsedInput >= Maximum)
							{
								validationMessage = String.Format(Constant.Messages.Violations.OutOfNumericRange, Minimum, Maximum);
							}
						}
					}
					else
					{
						validationMessage = Constant.Messages.Violations.NotANumber;
					}
				}
				return validationMessage;
			});
		}
	}
}