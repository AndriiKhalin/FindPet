using FindPet.Domain.Entities;

namespace FindPet.DataAccessLayer.Interfaces.IEntityRepository;

public interface IPetRepository : IBaseRepository<Pet>
{
    Task<bool> IsExistAsync(string petName);

}