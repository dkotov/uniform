using System;
using System.Data;

namespace ServiceStack.OrmLite
{
	/// <summary>
	/// Allow for mocking and unit testing by providing non-disposing 
	/// connection factory with injectable IDbCommand and IDbTransaction proxies
	/// </summary>
	public class OrmLiteConnectionFactory : IDbConnectionFactory
	{
		public OrmLiteConnectionFactory()
			: this(null, true)
		{
		}

		public OrmLiteConnectionFactory(string connectionString)
			: this(connectionString, true)
		{
		}

		public OrmLiteConnectionFactory(string connectionString, bool autoDisposeConnection)
			: this(connectionString, autoDisposeConnection, null)
		{
		}

		public OrmLiteConnectionFactory(string connectionString, IOrmLiteDialectProvider dialectProvider)
			: this(connectionString, true, dialectProvider)
		{
		}

		public OrmLiteConnectionFactory(string connectionString, bool autoDisposeConnection, IOrmLiteDialectProvider dialectProvider)
		{
			ConnectionString = connectionString;
			AutoDisposeConnection = autoDisposeConnection;

			if (dialectProvider != null)
			{
				OrmLiteConfig.DialectProvider = dialectProvider;
			}

			this.ConnectionFilter = x => x;
		}

		public string ConnectionString { get; set; }

		public bool AutoDisposeConnection { get; set; }

		public Func<IDbConnection, IDbConnection> ConnectionFilter { get; set; }

		/// <summary>
		/// Force the IDbConnection to always return this IDbCommand
		/// </summary>
		public IDbCommand AlwaysReturnCommand { get; set; }

		/// <summary>
		/// Force the IDbConnection to always return this IDbTransaction
		/// </summary>
		public IDbTransaction AlwaysReturnTransaction { get; set; }

		private OrmLiteConnection ormLiteConnection;
		private OrmLiteConnection OrmLiteConnection
		{
			get
			{
				if (ormLiteConnection == null)
				{
					ormLiteConnection = new OrmLiteConnection(this);
				}
				return ormLiteConnection;
			}
		}

		public IDbConnection OpenDbConnection()
		{
			var connection = CreateDbConnection();
			connection.Open();

			return connection;
		}

		public IDbConnection CreateDbConnection()
		{
			if (this.ConnectionString == null)
				throw new ArgumentNullException("ConnectionString", "ConnectionString must be set");

			var connection = AutoDisposeConnection
				? new OrmLiteConnection(this)
				: OrmLiteConnection;

			return ConnectionFilter(connection);
		}
	}

	public static class OrmLiteConnectionFactoryExtensions
	{
		public static void Exec(this IDbConnectionFactory connectionFactory, Action<IDbCommand> runDbCommandsFn)
		{
			using (var dbConn = connectionFactory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				runDbCommandsFn(dbCmd);
			}
		}

		public static T Exec<T>(this IDbConnectionFactory connectionFactory, Func<IDbCommand, T> runDbCommandsFn)
		{
			using (var dbConn = connectionFactory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				return runDbCommandsFn(dbCmd);
			}
		}
	}
}