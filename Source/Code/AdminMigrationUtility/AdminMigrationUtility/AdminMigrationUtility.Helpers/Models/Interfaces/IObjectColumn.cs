using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Models.Interfaces
{
	public interface IObjectColumn
	{
		Constant.Enums.ObjectFieldType ObjectFieldType { get; set; }
		String ColumnName { get; set; }
		String Data { get; set; }
		Int32 Order { get; set; }
		IEnumerable<IValidation> Validations { get; set; }

		Task<IEnumerable<String>> ValidateAsync();

		Task<Object> GetDataValueAsync();
	}
}