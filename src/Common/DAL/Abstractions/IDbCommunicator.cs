using System.Data;

namespace Regira.DAL.Abstractions;

public interface IDbCommunicator : IDisposable
{
    IDbConnection DbConnection { get; }
    IDbConnection OpenDbConnection { get; }

    IDbConnection Open();
    IDbConnection Close();
}