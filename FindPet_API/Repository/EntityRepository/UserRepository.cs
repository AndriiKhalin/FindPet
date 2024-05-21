using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Entities;

namespace Repository.EntityRepository;

public class UserRepository : BaseRepository<User>, IUserRepository<User>
{

    private readonly FindPetDbContext _context;

    public UserRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<bool> IsExistAsync(string userFirstName)
    {
        throw new NotImplementedException();
    }
}