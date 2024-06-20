﻿using FindPet.Domain.Entities;
using FindPet.Infrastructure.Data;
using FindPet.Infrastructure.Interfaces.IEntityRepository;
using Microsoft.EntityFrameworkCore;

namespace FindPet.Core.Repositories.EntityRepository;

public class FinderRepository : BaseRepository<Finder>, IUserRepository<Finder>
{
    private readonly FindPetDbContext _context;

    public FinderRepository(FindPetDbContext context) : base(context)
    {
        _context = context;
    }

    //public async Task<IEnumerable<Finder>> GetsAsync()
    //{
    //    return await GetAllAsync().Result.ToListAsync();
    //}

    //public async Task<Finder?> GetAsync(Guid userId)
    //{
    //    return await GetByConditionAsync(x => x.Id == userId).Result.FirstOrDefaultAsync();
    //}

    ////public async Task<IEnumerable<Ad>?> GetAdsByOwner(Guid ownerId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == ownerId).Result.Include(x => x.Ads).SelectMany(x => x.Ads).ToListAsync();
    ////}

    ////public async Task<IEnumerable<Pet>?> GetPetsByOwner(Guid ownerId)
    ////{
    ////    return await GetByConditionAsync(x => x.Id == ownerId).Result.Include(x => x.Pets).SelectMany(x => x.Pets).ToListAsync();
    ////}

    //public async Task<bool> IsExistAsync(Guid userId)
    //{
    //    return await ExistsAsync(x => x.Id == userId);
    //}

    public async Task<bool> IsExistAsync(string userName)
    {
        return await IsExistAsync(x => x.Name == userName);
    }


    public async Task<Finder?> GetUserAsync(string finderName)
    {
        return await GetByConditionAsync(x => x.Name == finderName).Result.FirstOrDefaultAsync();
    }

    //public async Task DeleteAsync(Guid userId)
    //{
    //    await DeleteAsync(userId);
    //}

    //public async Task UpdateAsync(User user)
    //{
    //    await UpdateAsync(user);
    //}

    //public async Task CreateAsync(User user)
    //{
    //    await CreateAsync(user);
    //}

}