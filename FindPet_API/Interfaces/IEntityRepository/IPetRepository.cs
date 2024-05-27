using Models.Entities;
using System.Security.Cryptography.Xml;

namespace Interfaces.IEntityRepository;

public interface IPetRepository : IBaseRepository<Pet>
{
    Task<bool> IsExistAsync(string petName);

}