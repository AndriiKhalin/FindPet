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

    public async Task<bool> IsExistAsync(string userFirstName)
    {
        return await IsExistAsync(x => x.FirstName == userFirstName);
    }
}