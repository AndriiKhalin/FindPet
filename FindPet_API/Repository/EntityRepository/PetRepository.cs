using Models.Entities;
using System.Security.Cryptography.Xml;
using Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Repository.EntityRepository;

public class PetRepository : BaseRepository<Pet>, IPetRepository
{
    private readonly FindPetDbContext _context;

    //public TransportRepository(RentDbContext context) : base(context)
    //{
    //    _context = context;
    //}

    //public Task<List<Transport>> GetTransports()
    //{
    //    return GetAll().Result.OrderBy(x => x.CreatedUpdatedAt).ToListAsync();
    //}

    //public Task<Transport?> GetTransport(Guid id)
    //{
    //    return GetByCondition(x => x.Id == id).FirstOrDefaultAsync();
    //}


    //public async Task<IEnumerable<Order>> GetOrdersByTransport(Guid transportId)
    //{
    //    return await GetByCondition(x => x.Id == transportId).Include(x => x.Orders).SelectMany(x => x.Orders).ToListAsync();
    //}

    //public async Task<TransportCategory?> GetCategoryByTransport(Guid transportId)
    //{
    //    return await GetByCondition(x => x.Id == transportId).Include(x => x.TransportCategory).Select(x => x.TransportCategory)
    //        .FirstOrDefaultAsync();
    //}

    //public async Task<bool> TransportExists(Guid id)
    //{
    //    return await Exists(x => x.Id == id);
    //}

    //public void DeleteTransport(Guid id)
    //{
    //    Delete(id);
    //}

    //public async Task UpdateTransport(Transport transport)
    //{
    //    Update(transport);
    //}

    //public async Task CreateTransport(Transport transport)
    //{
    //    await Create(transport);
    //}

    public PetRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pet>> GetPetsAsync()
    {
        return await GetAllAsync().Result.ToListAsync();
    }

    public async Task<Pet?> GetPetAsync(Guid petId)
    {
        return await GetByConditionAsync(x => x.Id == petId).Result.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Ad>?> GetAdsByPet(Guid petId)
    {
        return await GetByConditionAsync(x => x.Id == petId).Result.Include(x => x.Ads).SelectMany(x => x.Ads).ToListAsync();
    }

    public async Task<Finder> GetFinderByPet(Guid petId)
    {
        return await GetByConditionAsync(x => x.Id == petId).Result.Include(x => x.Finder).Select(x => x.Finder).FirstOrDefaultAsync();
    }

    public async Task<Owner> GetOwnerByPet(Guid petId)
    {
        return await GetByConditionAsync(x => x.Id == petId).Result.Include(x => x.Owner).Select(x => x.Owner).FirstOrDefaultAsync();
    }

    public async Task<bool> PetExistsAsync(Guid petId)
    {
        return await ExistsAsync(x => x.Id == petId);
    }

    public async Task<bool> PetExistsAsync(string petName)
    {
        return await ExistsAsync(x => x.Nickname == petName);
    }

    public async Task DeletePetAsync(Guid petId)
    {
        await DeleteAsync(petId);
    }

    public async Task UpdatePetAsync(Pet pet)
    {
        await UpdateAsync(pet);
    }

    public async Task CreatePetAsync(Pet pet)
    {
        await CreateAsync(pet);
    }
}