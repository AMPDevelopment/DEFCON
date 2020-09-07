using System;
using MySqlConnector;

namespace Defcon.Core.Database
{
    public class MySql : IDisposable
    {
        public readonly MySqlConnection Connection;

        public MySql(string host, int port, string user, string password, string database)
        {
            this.Connection = new MySqlConnection($"host={host};port={port};user id={user};password={password};database={database};");
        }
        
        public void Dispose()
        {
            this.Connection.Close();
        }
    }
}