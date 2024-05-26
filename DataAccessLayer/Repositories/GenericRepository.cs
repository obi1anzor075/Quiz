using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : class
    {

        private readonly DataStoreDbContext _dbContext;
        public GenericRepository(DataStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TModel>> GetQuestion()
        {
            try
            {
                return await _dbContext.Set<TModel>().ToListAsync();
            }
            catch
            {

                throw;
            }
        }

    }
}
