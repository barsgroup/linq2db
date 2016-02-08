using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace LinqToDB.DataProvider.MySql
{
	using Data;

	using LinqToDB.Properties;

    public static class MySqlTools
	{
		static readonly MySqlDataProvider _mySqlDataProvider = new MySqlDataProvider();
		
		static MySqlTools()
		{
			DataConnection.AddDataProvider(_mySqlDataProvider);
		}

		public static IDataProvider GetDataProvider()
		{
			return _mySqlDataProvider;
		}

		public static void ResolveMySql([NotNull] string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			new AssemblyResolver(path, "MySql.Data");
		}

		public static void ResolveMySql([NotNull] Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			new AssemblyResolver(assembly, "MySql.Data");
		}

		#region CreateDataConnection

		public static DataConnection CreateDataConnection(string connectionString)
		{
			return new DataConnection(_mySqlDataProvider, connectionString);
		}

		public static DataConnection CreateDataConnection(IDbConnection connection)
		{
			return new DataConnection(_mySqlDataProvider, connection);
		}

		public static DataConnection CreateDataConnection(IDbTransaction transaction)
		{
			return new DataConnection(_mySqlDataProvider, transaction);
		}

		#endregion

		#region BulkCopy

		private static BulkCopyType _defaultBulkCopyType = BulkCopyType.MultipleRows;
		public  static BulkCopyType  DefaultBulkCopyType
		{
			get { return _defaultBulkCopyType;  }
			set { _defaultBulkCopyType = value; }
		}

		public static BulkCopyRowsCopied MultipleRowsCopy<T>(
			DataConnection             dataConnection,
			IEnumerable<T>             source,
			int                        maxBatchSize       = 1000,
			Action<BulkCopyRowsCopied> rowsCopiedCallback = null)
		{
			return dataConnection.BulkCopy(
				new BulkCopyOptions
				{
					BulkCopyType       = BulkCopyType.MultipleRows,
					MaxBatchSize       = maxBatchSize,
					RowsCopiedCallback = rowsCopiedCallback,
				}, source);
		}

		#endregion
	}
}
