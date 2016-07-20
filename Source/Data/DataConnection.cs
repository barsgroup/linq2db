using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Bars2Db.Common;
using Bars2Db.Configuration;
using Bars2Db.DataProvider;
using Bars2Db.DataProvider.Oracle;
using Bars2Db.DataProvider.PostgreSQL;
using Bars2Db.Mapping;

namespace Bars2Db.Data
{
    public partial class DataConnection : ICloneable
    {
        #region System.IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region .ctor

        public DataConnection() : this(null)
        {
        }

        public DataConnection(string configurationString)
        {
            InitConfig();

            ConfigurationString = configurationString ?? DefaultConfiguration;

            if (ConfigurationString == null)
                throw new LinqToDBException("Configuration string is not provided.");

            var ci = GetConfigurationInfo(ConfigurationString);

            DataProvider = ci.DataProvider;
            ConnectionString = ci.ConnectionString;
            MappingSchema = DataProvider.MappingSchema;
        }

        public DataConnection([Properties.NotNull] string providerName, [Properties.NotNull] string connectionString)
        {
            if (providerName == null) throw new ArgumentNullException(nameof(providerName));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            var dataProvider =
                (
                    from key in _dataProviders.Keys
                    where string.Compare(key, providerName, StringComparison.InvariantCultureIgnoreCase) == 0
                    select _dataProviders[key]
                    ).FirstOrDefault();

            if (dataProvider == null)
            {
                throw new LinqToDBException("DataProvider with name '{0}' are not compatible.".Args(providerName));
            }

            InitConfig();

            DataProvider = dataProvider;
            ConnectionString = connectionString;
            MappingSchema = DataProvider.MappingSchema;
        }

        public DataConnection([Properties.NotNull] string providerName, [Properties.NotNull] IDbConnection connection)
        {
            if (providerName == null) throw new ArgumentNullException(nameof(providerName));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var dataProvider = (
                from key in _dataProviders.Keys
                where string.Compare(key, providerName, StringComparison.InvariantCultureIgnoreCase) == 0
                select _dataProviders[key]).FirstOrDefault();

            if (dataProvider == null)
            {
                throw new LinqToDBException("DataProvider with name '{0}' are not compatible.".Args(providerName));
            }

            DataProvider = dataProvider;
            MappingSchema = DataProvider.MappingSchema;
            _connection = connection;
        }

        public DataConnection([Properties.NotNull] IDataProvider dataProvider,
            [Properties.NotNull] string connectionString)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            InitConfig();

            DataProvider = dataProvider;
            MappingSchema = DataProvider.MappingSchema;
            ConnectionString = connectionString;
        }

        public DataConnection([Properties.NotNull] IDataProvider dataProvider,
            [Properties.NotNull] IDbConnection connection)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            InitConfig();

            if (!Common.Configuration.AvoidSpecificDataProviderAPI && !dataProvider.IsCompatibleConnection(connection))
                throw new LinqToDBException(
                    "DataProvider '{0}' and connection '{1}' are not compatible.".Args(dataProvider, connection));

            DataProvider = dataProvider;
            MappingSchema = DataProvider.MappingSchema;
            _connection = connection;
        }

        public DataConnection([Properties.NotNull] IDataProvider dataProvider,
            [Properties.NotNull] IDbTransaction transaction)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            InitConfig();

            if (!Common.Configuration.AvoidSpecificDataProviderAPI &&
                !dataProvider.IsCompatibleConnection(transaction.Connection))
                throw new LinqToDBException(
                    "DataProvider '{0}' and connection '{1}' are not compatible.".Args(dataProvider,
                        transaction.Connection));

            DataProvider = dataProvider;
            MappingSchema = DataProvider.MappingSchema;
            _connection = transaction.Connection;
            Transaction = transaction;
            _closeTransaction = false;
        }

        #endregion

        #region Public Properties

        public string ConfigurationString { get; }
        public IDataProvider DataProvider { get; }
        public string ConnectionString { get; }

        private static readonly ConcurrentDictionary<string, int> _configurationIDs;
        private static int _maxID;

        private int? _id;

        public int ID
        {
            get
            {
                if (!_id.HasValue)
                {
                    var key = MappingSchema.ConfigurationID + "." +
                              (ConfigurationString ?? ConnectionString ?? Connection.ConnectionString);
                    int id;

                    if (!_configurationIDs.TryGetValue(key, out id))
                        _configurationIDs[key] = id = Interlocked.Increment(ref _maxID);

                    _id = id;
                }

                return _id.Value;
            }
        }

        private bool? _isMarsEnabled;

        public bool IsMarsEnabled
        {
            get
            {
                if (_isMarsEnabled == null)
                    _isMarsEnabled = (bool) (DataProvider.GetConnectionInfo(this, "IsMarsEnabled") ?? false);

                return _isMarsEnabled.Value;
            }
            set { _isMarsEnabled = value; }
        }

        public static string DefaultConfiguration { get; set; }
        public static string DefaultDataProvider { get; set; }

        private static Action<TraceInfo> _onTrace = OnTraceInternal;

        public static Action<TraceInfo> OnTrace
        {
            get { return _onTrace; }
            set { _onTrace = value ?? OnTraceInternal; }
        }

        private static void OnTraceInternal(TraceInfo info)
        {
            if (info.BeforeExecute)
            {
                WriteTraceLine(info.SqlText, TraceSwitch.DisplayName);
            }
            else if (info.TraceLevel == TraceLevel.Error)
            {
                var sb = new StringBuilder();

                for (var ex = info.Exception; ex != null; ex = ex.InnerException)
                {
                    sb
                        .AppendLine()
                        .AppendFormat("Exception: {0}", ex.GetType())
                        .AppendLine()
                        .AppendFormat("Message  : {0}", ex.Message)
                        .AppendLine()
                        .AppendLine(ex.StackTrace)
                        ;
                }

                WriteTraceLine(sb.ToString(), TraceSwitch.DisplayName);
            }
            else if (info.RecordsAffected != null)
            {
                WriteTraceLine(
                    "Execution time: {0}. Records affected: {1}.\r\n".Args(info.ExecutionTime, info.RecordsAffected),
                    TraceSwitch.DisplayName);
            }
            else
            {
                WriteTraceLine("Execution time: {0}\r\n".Args(info.ExecutionTime), TraceSwitch.DisplayName);
            }
        }

        private static TraceSwitch _traceSwitch;

        public static TraceSwitch TraceSwitch
        {
            get
            {
                return _traceSwitch ?? (_traceSwitch = new TraceSwitch("DataConnection", "DataConnection trace switch",
#if DEBUG
                    "Warning"
#else
                "Off"
#endif
                    ));
            }
            set { _traceSwitch = value; }
        }

        public static void TurnTraceSwitchOn(TraceLevel traceLevel = TraceLevel.Info)
        {
            TraceSwitch = new TraceSwitch("DataConnection", "DataConnection trace switch", traceLevel.ToString());
        }

        public static Action<string, string> WriteTraceLine =
            (message, displayName) => Debug.WriteLine(message, displayName);

        #endregion

        #region Configuration

        private static IDataProvider FindProvider(string configuration,
            IEnumerable<KeyValuePair<string, IDataProvider>> ps, IDataProvider defp)
        {
            foreach (var p in ps.OrderByDescending(kv => kv.Key.Length))
                if (configuration == p.Key || configuration.StartsWith(p.Key + '.'))
                    return p.Value;

            foreach (var p in ps.OrderByDescending(kv => kv.Value.Name.Length))
                if (configuration == p.Value.Name || configuration.StartsWith(p.Value.Name + '.'))
                    return p.Value;

            return defp;
        }

        static DataConnection()
        {
            _configurationIDs = new ConcurrentDictionary<string, int>();


            OracleTools.GetDataProvider();
            PostgreSQLTools.GetDataProvider();

            var section = LinqToDBSection.Instance;

            if (section != null)
            {
                DefaultConfiguration = section.DefaultConfiguration;
                DefaultDataProvider = section.DefaultDataProvider;

                foreach (DataProviderElement provider in section.DataProviders)
                {
                    var dataProviderType = Type.GetType(provider.TypeName, true);
                    var providerInstance = (IDataProviderFactory) Activator.CreateInstance(dataProviderType);

                    if (!string.IsNullOrEmpty(provider.Name))
                        AddDataProvider(provider.Name, providerInstance.GetDataProvider(provider.Attributes));
                }
            }
        }

        private static readonly List<Func<ConnectionStringSettings, IDataProvider>> _providerDetectors =
            new List<Func<ConnectionStringSettings, IDataProvider>>();

        public static void AddProviderDetector(Func<ConnectionStringSettings, IDataProvider> providerDetector)
        {
            _providerDetectors.Add(providerDetector);
        }

        internal static bool IsMachineConfig(ConnectionStringSettings css)
        {
            string source;

            try
            {
                source = css.ElementInformation.Source;
            }
            catch (Exception)
            {
                source = "";
            }

            return source == null || source.EndsWith("machine.config", StringComparison.OrdinalIgnoreCase);
        }

        private static void InitConnectionStrings()
        {
            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
            {
                _configurations[css.Name] = new ConfigurationInfo(css);

                if (DefaultConfiguration == null && !IsMachineConfig(css))
                {
                    DefaultConfiguration = css.Name;
                }
            }
        }

        private static bool _isInitialized;
        private static readonly object _initSync = new object();

        private static void InitConfig()
        {
            if (!_isInitialized)
                lock (_initSync)
                    if (!_isInitialized)
                    {
                        InitConnectionStrings();
                        _isInitialized = true;
                    }
        }

        private static readonly ConcurrentDictionary<string, IDataProvider> _dataProviders =
            new ConcurrentDictionary<string, IDataProvider>();

        public static void AddDataProvider([Properties.NotNull] string providerName,
            [Properties.NotNull] IDataProvider dataProvider)
        {
            if (providerName == null) throw new ArgumentNullException(nameof(providerName));
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

            if (string.IsNullOrEmpty(dataProvider.Name))
                throw new ArgumentException("dataProvider.Name cant be empty.", nameof(dataProvider));

            _dataProviders[providerName] = dataProvider;
        }

        public static void AddDataProvider([Properties.NotNull] IDataProvider dataProvider)
        {
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

            AddDataProvider(dataProvider.Name, dataProvider);
        }

        public static IDataProvider GetDataProvider([Properties.NotNull] string configurationString)
        {
            InitConfig();

            return GetConfigurationInfo(configurationString).DataProvider;
        }

        private class ConfigurationInfo
        {
            private readonly ConnectionStringSettings _connectionStringSettings;

            private IDataProvider _dataProvider;

            public string ConnectionString;

            public ConfigurationInfo(string connectionString, IDataProvider dataProvider)
            {
                ConnectionString = connectionString;
                DataProvider = dataProvider;
            }

            public ConfigurationInfo(ConnectionStringSettings connectionStringSettings)
            {
                ConnectionString = connectionStringSettings.ConnectionString;

                _connectionStringSettings = connectionStringSettings;
            }

            public IDataProvider DataProvider
            {
                get { return _dataProvider ?? (_dataProvider = GetDataProvider(_connectionStringSettings)); }
                set { _dataProvider = value; }
            }

            private static IDataProvider GetDataProvider(ConnectionStringSettings css)
            {
                var configuration = css.Name;
                var providerName = css.ProviderName;
                var dataProvider = _providerDetectors.Select(d => d(css)).FirstOrDefault(dp => dp != null);

                if (dataProvider == null)
                {
                    var defaultDataProvider = DefaultDataProvider != null ? _dataProviders[DefaultDataProvider] : null;

                    if (string.IsNullOrEmpty(providerName))
                        dataProvider = FindProvider(configuration, _dataProviders, defaultDataProvider);
                    else if (_dataProviders.ContainsKey(providerName))
                        dataProvider = _dataProviders[providerName];
                    else if (_dataProviders.ContainsKey(configuration))
                        dataProvider = _dataProviders[configuration];
                    else
                    {
                        var providers =
                            _dataProviders.Where(dp => dp.Value.ConnectionNamespace == providerName).ToList();

                        switch (providers.Count)
                        {
                            case 0:
                                dataProvider = defaultDataProvider;
                                break;
                            case 1:
                                dataProvider = providers[0].Value;
                                break;
                            default:
                                dataProvider = FindProvider(configuration, providers, providers[0].Value);
                                break;
                        }
                    }
                }

                if (dataProvider != null && DefaultConfiguration == null && !IsMachineConfig(css))
                {
                    DefaultConfiguration = css.Name;
                }

                return dataProvider;
            }
        }

        private static ConfigurationInfo GetConfigurationInfo(string configurationString)
        {
            ConfigurationInfo ci;

            if (_configurations.TryGetValue(configurationString ?? DefaultConfiguration, out ci))
                return ci;

            throw new LinqToDBException("Configuration '{0}' is not defined.".Args(configurationString));
        }

        public static void SetConnectionStrings(System.Configuration.Configuration config)
        {
            foreach (ConnectionStringSettings css in config.ConnectionStrings.ConnectionStrings)
            {
                _configurations[css.Name] = new ConfigurationInfo(css);

                if (DefaultConfiguration == null && !IsMachineConfig(css))
                {
                    DefaultConfiguration = css.Name;
                }
            }
        }

        private static readonly ConcurrentDictionary<string, ConfigurationInfo> _configurations =
            new ConcurrentDictionary<string, ConfigurationInfo>();

        public static void AddConfiguration(
            [Properties.NotNull] string configuration,
            [Properties.NotNull] string connectionString,
            IDataProvider dataProvider = null)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            _configurations[configuration] = new ConfigurationInfo(
                connectionString,
                dataProvider ?? FindProvider(configuration, _dataProviders, _dataProviders[DefaultDataProvider]));
        }

        public static void SetConnectionString(
            [Properties.NotNull] string configuration,
            [Properties.NotNull] string connectionString)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            InitConfig();

            _configurations[configuration].ConnectionString = connectionString;
        }

        public static string GetConnectionString(string configurationString)
        {
            InitConfig();

            ConfigurationInfo ci;

            if (_configurations.TryGetValue(configurationString, out ci))
                return ci.ConnectionString;

            throw new LinqToDBException("Configuration '{0}' is not defined.".Args(configurationString));
        }

        #endregion

        #region Connection

        private bool _closeConnection;
        private bool _closeTransaction;
        private IDbConnection _connection;

        public IDbConnection Connection
        {
            get
            {
                if (_connection == null)
                    _connection = DataProvider.CreateConnection(ConnectionString);

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                    _closeConnection = true;
                }

                return _connection;
            }
        }

        public event EventHandler OnClosing;
        public event EventHandler OnClosed;

        public virtual void Close()
        {
            if (OnClosing != null)
                OnClosing(this, EventArgs.Empty);

            DisposeCommand();

            if (Transaction != null && _closeTransaction)
            {
                Transaction.Dispose();
                Transaction = null;
            }

            if (_connection != null && _closeConnection)
            {
                _connection.Dispose();
                _connection = null;
            }

            if (OnClosed != null)
                OnClosed(this, EventArgs.Empty);
        }

        #endregion

        #region Command

        public string LastQuery;

        internal void InitCommand(CommandType commandType, string sql, DataParameter[] parameters,
            List<string> queryHints)
        {
            if (queryHints != null && queryHints.Count > 0)
            {
                var sqlProvider = DataProvider.CreateSqlBuilder();
                sql = sqlProvider.ApplyQueryHints(sql, queryHints);
                queryHints.Clear();
            }

            DataProvider.InitCommand(this, commandType, sql, parameters);
            LastQuery = Command.CommandText;
        }

        private int? _commandTimeout;

        public int CommandTimeout
        {
            get { return _commandTimeout ?? 0; }
            set { _commandTimeout = value; }
        }

        private IDbCommand _command;

        public IDbCommand Command
        {
            get { return _command ?? (_command = CreateCommand()); }
            set { _command = value; }
        }

        public IDbCommand CreateCommand()
        {
            var command = Connection.CreateCommand();

            if (_commandTimeout.HasValue)
                command.CommandTimeout = _commandTimeout.Value;

            if (Transaction != null)
                command.Transaction = Transaction;

            return command;
        }

        public void DisposeCommand()
        {
            if (_command != null)
            {
                DataProvider.DisposeCommand(this);
                _command = null;
            }
        }

        protected internal virtual int ExecuteNonQuery()
        {
            if (TraceSwitch.Level == TraceLevel.Off)
                return Command.ExecuteNonQuery();

            if (TraceSwitch.TraceInfo)
            {
                OnTrace(new TraceInfo
                {
                    BeforeExecute = true,
                    TraceLevel = TraceLevel.Info,
                    DataConnection = this,
                    Command = Command
                });
            }

            try
            {
                var now = DateTime.Now;
                var ret = Command.ExecuteNonQuery();

                if (TraceSwitch.TraceInfo)
                {
                    OnTrace(new TraceInfo
                    {
                        TraceLevel = TraceLevel.Info,
                        DataConnection = this,
                        Command = Command,
                        ExecutionTime = DateTime.Now - now,
                        RecordsAffected = ret
                    });
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (TraceSwitch.TraceError)
                {
                    OnTrace(new TraceInfo
                    {
                        TraceLevel = TraceLevel.Error,
                        DataConnection = this,
                        Command = Command,
                        Exception = ex
                    });
                }

                throw;
            }
        }

        protected internal virtual object ExecuteScalar()
        {
            if (TraceSwitch.Level == TraceLevel.Off)
                return Command.ExecuteScalar();

            if (TraceSwitch.TraceInfo)
            {
                OnTrace(new TraceInfo
                {
                    BeforeExecute = true,
                    TraceLevel = TraceLevel.Info,
                    DataConnection = this,
                    Command = Command
                });
            }

            try
            {
                var now = DateTime.Now;
                var ret = Command.ExecuteScalar();

                if (TraceSwitch.TraceInfo)
                {
                    OnTrace(new TraceInfo
                    {
                        TraceLevel = TraceLevel.Info,
                        DataConnection = this,
                        Command = Command,
                        ExecutionTime = DateTime.Now - now
                    });
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (TraceSwitch.TraceError)
                {
                    OnTrace(new TraceInfo
                    {
                        TraceLevel = TraceLevel.Error,
                        DataConnection = this,
                        Command = Command,
                        Exception = ex
                    });
                }

                throw;
            }
        }

        protected internal virtual IDataReader ExecuteReader(CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            if (TraceSwitch.Level == TraceLevel.Off)
                return Command.ExecuteReader(commandBehavior);

            if (TraceSwitch.TraceInfo)
            {
                OnTrace(new TraceInfo
                {
                    BeforeExecute = true,
                    TraceLevel = TraceLevel.Info,
                    DataConnection = this,
                    Command = Command
                });
            }

            try
            {
                var now = DateTime.Now;
                var ret = Command.ExecuteReader(commandBehavior);

                if (TraceSwitch.TraceInfo)
                {
                    OnTrace(new TraceInfo
                    {
                        TraceLevel = TraceLevel.Info,
                        DataConnection = this,
                        Command = Command,
                        ExecutionTime = DateTime.Now - now
                    });
                }

                return ret;
            }
            catch (Exception ex)
            {
                if (TraceSwitch.TraceError)
                {
                    OnTrace(new TraceInfo
                    {
                        TraceLevel = TraceLevel.Error,
                        DataConnection = this,
                        Command = Command,
                        Exception = ex
                    });
                }

                throw;
            }
        }

        #endregion

        #region Transaction

        public IDbTransaction Transaction { get; private set; }

        public virtual DataConnectionTransaction BeginTransaction()
        {
            // If transaction is open, we dispose it, it will rollback all changes.
            //
            if (Transaction != null)
                Transaction.Dispose();

            // Create new transaction object.
            //
            Transaction = Connection.BeginTransaction();

            _closeTransaction = true;

            // If the active command exists.
            //
            if (_command != null)
                _command.Transaction = Transaction;

            return new DataConnectionTransaction(this);
        }

        public virtual DataConnectionTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            // If transaction is open, we dispose it, it will rollback all changes.
            //
            if (Transaction != null)
                Transaction.Dispose();

            // Create new transaction object.
            //
            Transaction = Connection.BeginTransaction(isolationLevel);

            _closeTransaction = true;

            // If the active command exists.
            //
            if (_command != null)
                _command.Transaction = Transaction;

            return new DataConnectionTransaction(this);
        }

        public virtual void CommitTransaction()
        {
            if (Transaction != null)
            {
                Transaction.Commit();

                if (_closeTransaction)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
            }
        }

        public virtual void RollbackTransaction()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();

                if (_closeTransaction)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
            }
        }

        #endregion

        #region MappingSchema

        public MappingSchema MappingSchema { get; private set; }

        public bool InlineParameters { get; set; }

        private List<string> _queryHints;
        public List<string> QueryHints => _queryHints ?? (_queryHints = new List<string>());

        private List<string> _nextQueryHints;
        public List<string> NextQueryHints => _nextQueryHints ?? (_nextQueryHints = new List<string>());

        public DataConnection AddMappingSchema(MappingSchema mappingSchema)
        {
            MappingSchema = new MappingSchema(mappingSchema, MappingSchema);
            _id = null;

            return this;
        }

        public

            #endregion

            #region ICloneable Members

            DataConnection(string configurationString, IDataProvider dataProvider, string connectionString,
                IDbConnection connection, MappingSchema mappingSchema)
        {
            ConfigurationString = configurationString;
            DataProvider = dataProvider;
            ConnectionString = connectionString;
            _connection = connection;
            MappingSchema = mappingSchema;
            _closeConnection = true;
        }

        public object Clone()
        {
            IDbConnection connection;
            if (_connection != null)
            {
                var cloneable = _connection as ICloneable;
                connection = cloneable != null
                    ? (IDbConnection) cloneable.Clone()
                    : DataProvider.CreateConnection(ConnectionString);
            }
            else
            {
                connection = null;
            }

            return new DataConnection(ConfigurationString, DataProvider, ConnectionString, connection, MappingSchema);
        }

        #endregion
    }
}