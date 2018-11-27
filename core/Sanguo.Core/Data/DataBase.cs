using System.Data.SQLite;
using System.Threading.Tasks;
using System.Data;
using Sanguo.Core.Protocol;
using System;

namespace Sanguo.Core.Data
{
    public class DataBase
    {
        protected SQLiteConnection _connection;
        protected SQLiteCommand _command;
        protected DataBaseEventArgs _eventArgs;
        public event EventHandler<DataBaseEventArgs> OnVerifyFailed; 
        public DataBase(string databasePath)
        {
            _connection = new SQLiteConnection($"data source={databasePath}");
            _command = new SQLiteCommand { Connection = _connection };
            _eventArgs = new DataBaseEventArgs { Connection = _connection, Command = _command };
        }
        public void Open() => _connection.Open();
        public void Verify(string verifyCommand)
        {
            _command.CommandText = verifyCommand;
            try
            {
                _command.ExecuteReader();
            }
            catch (SQLiteException e)
            {
                _eventArgs.LastException = e;
                OnVerifyFailed.Invoke(this, _eventArgs);
                _eventArgs = null;
            }
        }
        public void Close() => _connection.Close();

    }
    public class DataBaseEventArgs
    {
        public SQLiteCommand Command { get; set; }
        public SQLiteConnection Connection { get; set; }
        public SQLiteException LastException { get; set; }
    }
}
