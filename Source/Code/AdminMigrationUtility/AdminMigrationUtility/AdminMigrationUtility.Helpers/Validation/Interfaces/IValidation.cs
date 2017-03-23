using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation.Interfaces
{
	public delegate void ChoiceDelegate(kCura.Relativity.Client.DTOs.Choice op);

	public delegate void ClientDelegate(kCura.Relativity.Client.DTOs.Client op);

	public delegate void GroupDelegate(kCura.Relativity.Client.DTOs.FieldValueList<kCura.Relativity.Client.DTOs.Group> op);

	public interface IValidation
	{
		Task<String> ValidateAsync(String input);
	}
}