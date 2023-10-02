using Regira.DAL.Abstractions;
using System.Data;

namespace Regira.DAL.Models;

public class DbCommunicator<TDbConnection> : IDbCommunicator
    where TDbConnection : class, IDbConnection, new()
{
    private readonly bool _shouldDispose;
    public IDbConnection DbConnection { get; }

    public IDbConnection OpenDbConnection
    {
        get
        {
            if (DbConnection.State != ConnectionState.Open)
            {
                Open();
            }

            return DbConnection;
        }
    }
    public DbCommunicator(TDbConnection dbConnection)
    {
        DbConnection = dbConnection;
    }
    public DbCommunicator(string connectionString)
    {
        _shouldDispose = true;
        DbConnection = new TDbConnection
        {
            ConnectionString = connectionString
        };
    }


    public IDbConnection Open()
    {
        DbConnection.Open();
        return DbConnection;
    }
    public IDbConnection Close()
    {
        DbConnection.Close();
        return DbConnection;
    }
        

    public void Dispose()
    {
        if (_shouldDispose)
        {
            DbConnection.Dispose();
        }
    }
}