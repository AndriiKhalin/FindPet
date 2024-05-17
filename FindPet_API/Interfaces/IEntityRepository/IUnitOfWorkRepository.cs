namespace Interfaces.IEntityRepository;

public interface IUnitOfWorkRepository
{
    IPetRepository Pet { get; }

    IFinderRepository Finder { get; }

    IOwnerRepository Owner { get; }

    IAdRepository Ad { get; }
    Task Save();
}