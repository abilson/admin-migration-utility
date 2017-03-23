using System;
using System.Collections.Generic;
using System.Data;

namespace AdminMigrationUtility.Parser.Interfaces
{
	public interface IParser : IDisposable
	{
		bool EndOfData { get; }

		IEnumerable<String> ParseColumns();

		IDataReader ParseData();

		bool Read();

		object this[string name] { get; }

		long LineNumber { get; }
	}
}