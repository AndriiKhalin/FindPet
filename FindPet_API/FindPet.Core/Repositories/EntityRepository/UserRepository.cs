using FindPet.Domain.Entities;
using FindPet.Infrastructure.Data;
using FindPet.Infrastructure.Interfaces.IEntityRepository;

namespace FindPet.Core.Repositories.EntityRepository;

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
}