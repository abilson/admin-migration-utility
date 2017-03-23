using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Models
{
	public class ObjectColumn<T> : IObjectColumn
	{
		public Constant.Enums.ObjectFieldType ObjectFieldType { get; set; }
		public String ColumnName { get; set; }
		private String _data;

		public String Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value == null ? String.Empty : value.Trim();
			}
		}

		public Int32 Order { get; set; }
		public IEnumerable<IValidation> Validations { get; set; }

		public ObjectColumn(String columnName)
		{
			ColumnName = columnName;
			Validations = new List<IValidation>();
		}

		public async Task<IEnumerable<String>> ValidateAsync()
		{
			var violations = new List<String>();
			foreach (var validation in Validations)
			{
				try
				{
					var violation = await validation.ValidateAsync(Data);
					if (violation != null)
					{
						violations.Add(violation);
					}
				}
				catch (Exception ex)
				{
					violations.Add(ex.ToString());
				}
			}
			await FinalConvertibleValidationAsync(violations);
			return violations;
		}

		private async Task FinalConvertibleValidationAsync(List<String> violations)
		{
			// Don't validate the input data's ability to be casted if it has not passed the configured validations
			if (violations.Count == 0)
			{
				try
				{
					await GetDataValueAsync();
				}
				catch (Exception ex)
				{
					violations.Add(Constant.Messages.Violations.NotConvertible +ex.ToString());
				}
			}
		}

		public async Task<Object> GetDataValueAsync()
		{
			return await Task.Run(() =>
			{
				T retVal;
				try
				{
					if (String.IsNullOrWhiteSpace(Data))
					{
						if (typeof(T) == typeof(String))
						{
							String value = Data == null ? String.Empty : Data;
							retVal = (T)Convert.ChangeType(value, typeof(T));
						}
						else
						{
							retVal = (T)Activator.CreateInstance(typeof(T));
						}
					}
					else
					{
						if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
						{
							var underlyingType = Nullable.GetUnderlyingType(typeof(T));
							retVal = (T)Convert.ChangeType(Data, underlyingType);
						}
						else
						{
							retVal = (T)Convert.ChangeType(Data, typeof(T));
						}
					}
				}
				catch (Exception ex)
				{
					var columnName = ColumnName ?? "[Null]";
					var data = Data ?? "[Null]";
					throw new Exceptions.ObjectColumnDataConversionException(String.Format(Constant.ErrorMessages.ObjectColumnDataConversionError, data, columnName), ex);
				}

				return retVal;
			});
		}
	}
}