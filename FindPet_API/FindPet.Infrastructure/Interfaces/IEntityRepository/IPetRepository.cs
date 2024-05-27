using FindPet.Domain.Entities;

namespace FindPet.Infrastructure.Interfaces.IEntityRepository;

public interface IPetRepository : IBaseRepository<Pet>
{
    Task<bool> IsExistAsync(string petName);

}