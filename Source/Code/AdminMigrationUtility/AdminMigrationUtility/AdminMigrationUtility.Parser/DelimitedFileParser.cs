﻿using AdminMigrationUtility.Parser.Interfaces;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("AdminMigrationUtility.Parser.NUnit")]

namespace AdminMigrationUtility.Parser
{
	public class DelimitedFileParser : IDataReader, IParser
	{
		internal bool _disposed;
		internal TextFieldParser _parser;
		internal Stream _fileStream;

		internal String _fileLocation;
		internal String _fieldDelimiter;
		internal String[] _columns;
		internal String[] _currentLine;

		public long LineNumber
		{
			get
			{
				long retVal = 0;
				if (_parser != null)
				{
					retVal = _parser.LineNumber;
				}
				return retVal;
			}
		}

		public int RecordsAffected
		{
			get { return 1; }
		}

		public bool IsClosed
		{
			get { return _disposed; }
		}

		public bool EndOfData
		{
			get
			{
				var retval = true;
				if (_parser != null)
				{
					retval = _parser.EndOfData;
				}
				return retval;
			}
		}

		public DelimitedFileParser(String fileLocation, String fieldDelimiter)
		{
			_fileLocation = fileLocation;
			_fieldDelimiter = fieldDelimiter;
			if (SourceExists())
			{
				_parser = new TextFieldParser(_fileLocation)
				{
					TextFieldType = FieldType.Delimited,
					Delimiters = new String[] { _fieldDelimiter },
					HasFieldsEnclosedInQuotes = true,
					TrimWhiteSpace = true
				};
			}
			ParseColumns();
		}

		public DelimitedFileParser(Stream stream, String fieldDelimiter)
		{
			_fileStream = stream;
			_fieldDelimiter = fieldDelimiter;
			if (SourceExists())
			{
				_parser = new TextFieldParser(_fileStream, Encoding.UTF8, true)
				{
					TextFieldType = FieldType.Delimited,
					Delimiters = new String[] { _fieldDelimiter },
					HasFieldsEnclosedInQuotes = true,
					TrimWhiteSpace = true
				};
			}
			ParseColumns();
		}

		public Boolean SourceExists()
		{
			if ((_fileLocation != null && !new FileInfo(_fileLocation).Exists) && _fileStream == null)
			{
				throw new Exceptions.CantAccessSourceExcepetion();
			}
			return true;
		}

		public IEnumerable<String> ParseColumns()
		{
			String[] retVal = null;
			if (SourceExists())
			{
				if (_columns != null)
				{
					retVal = _columns;
				}
				else
				{
					if (NextResult())
					{
						_columns = _currentLine;
						//Make sure headers exist in file
						if (_columns == null || _columns.Length < 1)
						{
							throw new Exceptions.NoColumnsExcepetion();
						}
						ValidateColumns(_columns);
						retVal = _columns;
					}
					else
					{
						throw new Exceptions.NoColumnsExcepetion();
					}
				}
			}
			return retVal;
		}

		internal void ValidateColumns(IEnumerable<String> columns)
		{
			//Validate Blank Columns
			foreach (var column in columns)
			{
				if (String.IsNullOrWhiteSpace(column))
				{
					throw new Exceptions.BlankColumnExcepetion();
				}
			}

			//Validate Duplicates
			var destination = new List<String>();
			foreach (var column in columns)
			{
				if (!destination.Contains(column))
				{
					destination.Add(column);
				}
				else
				{
					throw new Exceptions.DuplicateColumnsExistExcepetion();
				}
			}
		}

		public IDataReader ParseData()
		{
			return this;
		}

		public int Depth
		{
			get { return 1; }
		}

		public bool Read()
		{
			return NextResult();
		}

		public bool NextResult()
		{
			var retVal = false;
			if (!_parser.EndOfData)
			{
				var data = _parser.ReadFields();
				if (data != null)
				{
					if (_columns != null && data.Length != _columns.Length)
					{
						throw new Exceptions.NumberOfColumnsNotEqualToNumberOfDataValuesException(LineNumber);
					}
					_currentLine = data;
					retVal = true;
				}
			}
			return retVal;
		}

		public void Close()
		{
			Dispose();
		}

		public DataTable GetSchemaTable()
		{
			DataTable t = new DataTable();
			t.Rows.Add(_columns);
			return t;
		}

		public int FieldCount
		{
			get { return _columns.Length; }
		}

		public bool GetBoolean(int i)
		{
			return Boolean.Parse(_currentLine[i]);
		}

		public byte GetByte(int i)
		{
			return Byte.Parse(_currentLine[i]);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			return Char.Parse(_currentLine[i]);
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			return (IDataReader)this;
		}

		public string GetDataTypeName(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			return DateTime.Parse(_currentLine[i]);
		}

		public decimal GetDecimal(int i)
		{
			return Decimal.Parse(_currentLine[i]);
		}

		public double GetDouble(int i)
		{
			return Double.Parse(_currentLine[i]);
		}

		public Type GetFieldType(int i)
		{
			return typeof(String);
		}

		public float GetFloat(int i)
		{
			return float.Parse(_currentLine[i]);
		}

		public Guid GetGuid(int i)
		{
			return Guid.Parse(_currentLine[i]);
		}

		public short GetInt16(int i)
		{
			return Int16.Parse(_currentLine[i]);
		}

		public int GetInt32(int i)
		{
			return Int32.Parse(_currentLine[i]);
		}

		public long GetInt64(int i)
		{
			return Int64.Parse(_currentLine[i]);
		}

		public string GetName(int i)
		{
			return _columns[i];
		}

		public int GetOrdinal(string name)
		{
			int result = -1;
			for (int i = 0; i < _columns.Length; i++)
				if (_columns[i] == name)
				{
					result = i;
					break;
				}
			return result;
		}

		public string GetString(int i)
		{
			return _currentLine[i];
		}

		public object GetValue(int i)
		{
			return _currentLine[i];
		}

		public int GetValues(object[] values)
		{
			for (var i = 0; i < _currentLine.Length; i++)
			{
				values[i] = _currentLine[i];
			}
			return 1;
		}

		public bool IsDBNull(int i)
		{
			return string.IsNullOrWhiteSpace(_currentLine[i]);
		}

		public object this[string name]
		{
			get { return _currentLine[GetOrdinal(name)]; }
		}

		public object this[int i]
		{
			get { return GetValue(i); }
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_fileStream != null)
					{
						_fileStream.Dispose();
					}
					if (_parser != null)
					{
						_parser.Dispose();
					}
				}
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}