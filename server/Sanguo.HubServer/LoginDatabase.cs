using Sanguo.Core.Data;
using Sanguo.Core.Protocol.Hub;
using System.Data;
using System.Threading.Tasks;

namespace Sanguo.HubServer
{
    public class LoginDatabase:DataBase
    {
        public LoginDatabase(string databasePath) : base(databasePath) { }


        public async Task<bool> IsRegisteredAsync(string username)
        {
            _command.Reset();
            _command.CommandText = string.Format(SQLiteCommands.SelectUsers, username);
            return await _command.ExecuteScalarAsync() != null;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            _command.Reset();
            _command.CommandText = string.Format(
                SQLiteCommands.AddUser,
                1,
                request.Username,
                request.HashedPw,
                100,
                request.AvatarUri);
            return await _command.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> LoginAsync(LoginRequest request)
        {
            _command.Reset();
            _command.CommandText = string.Format(SQLiteCommands.SelectUsers, request.Username);
            var reader = await _command.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            var match = false;
            foreach (DataRow item in dt.Rows)
            {
                if ((string)item.ItemArray[2] == request.HashedPw)
                    match = true;
            }
            return match;
        }

        internal void RunCommand(string cmd)
        {
            _command.Reset();
            _command.CommandText = cmd;
            _command.ExecuteNonQuery();
        }
    }
}
