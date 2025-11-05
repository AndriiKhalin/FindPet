using FindPet.DataAccessLayer.Data;
using FindPet.DataAccessLayer.Interfaces.IEntityRepository;
using FindPet.Domain.Entities;

namespace FindPet.DataAccessLayer.Repositories.EntityRepository;

public class UserRepository : BaseRepository<User>, IUserRepository<User>
{
    private readonly FindPetDbContext _context;

    public UserRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> IsExistAsync(string userName)
    {
        return await IsExistAsync(x => x.Name == userName);
    }

    public async Task<User?> GetUserAsync(string userName)
    {
        return await GetSingleByConditionAsync(x => x.Name == userName);
    }
}