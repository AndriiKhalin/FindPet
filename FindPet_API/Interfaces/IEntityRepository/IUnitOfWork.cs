using Models.Entities;

namespace Interfaces.IEntityRepository;

public interface IUnitOfWork
{
    IPetRepository Pet { get; }

    IUserRepository<Finder> Finder { get; }

    IUserRepository<Owner> Owner { get; }

    IUserRepository<User> User { get; }

    IAdRepository Ad { get; }
    Task Save();
}