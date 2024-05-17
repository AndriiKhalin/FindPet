using Interfaces.IEntityRepository;
using Microsoft.Extensions.FileProviders;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class UnitOfWorkRepository : IUnitOfWorkRepository
{
    private readonly FindPetDbContext _context;
    private IPetRepository? _pet;
    private IFinderRepository? _finder;
    private IOwnerRepository? _owner;
    private IAdRepository? _ad;

    private bool _disposedValue;

    public UnitOfWorkRepository(FindPetDbContext context)
    {
        _context = context;
    }

    public IPetRepository Pet
    {
        get
        {
            return _pet ??= new PetRepository(_context);
        }
    }

    public IFinderRepository Finder
    {
        get
        {
            return _finder ??= new FinderRepository(_context);
        }
    }

    public IOwnerRepository Owner
    {
        get
        {
            return _owner ??= new OwnerRepository(_context);
        }
    }

    public IAdRepository Ad => _ad ??= new AdRepository(_context);


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
        {
            return;
        }

        if (disposing)
        {
            _context.Dispose();
        }

        _disposedValue = true;
    }

    public async Task Save()
    {
        await _context.SaveChangesAsync();
    }
}