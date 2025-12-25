namespace AuthAPI.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task CommitAsync();
}
