using Interfaces.IEntityRepository;
using Microsoft.Extensions.FileProviders;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class UnitOfWork : IUnitOfWork
{
    private readonly FindPetDbContext _context;
    private IPetRepository? _pet;
    private IUserRepository<Finder>? _finder;
    private IUserRepository<Owner>? _owner;
    private IAdRepository? _ad;
    private IUserRepository<User> _user;

    private bool _disposedValue;


    public UnitOfWork(FindPetDbContext context)
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

    public IUserRepository<Finder> Finder
    {
        get
        {
            return _finder ??= new FinderRepository(_context);
        }
    }

    public IUserRepository<Owner> Owner
    {
        get
        {
            return _owner ??= new OwnerRepository(_context);
        }
    }



    public IAdRepository Ad => _ad ??= new AdRepository(_context);
    public IUserRepository<User> User => _user ??= new UserRepository(_context);


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

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}