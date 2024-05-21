using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Interfaces.IEntityRepository;

public interface IPetRepository : IBaseRepository<Pet>
{
    Task<IEnumerable<Pet>> GetsAsync();

    Task<Pet?> GetAsync(Guid petId);

    Task<bool> IsExistAsync(Guid petId);

    Task<bool> IsExistAsync(string petName);

    Task DeleteAsync(Guid petId);

    Task UpdateAsync(Pet pet);

    Task CreateAsync(Pet pet);
}