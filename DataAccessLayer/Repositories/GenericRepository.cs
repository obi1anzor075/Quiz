using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.DataContext;
using DataAccessLayer.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : class
    {
        private readonly DataStoreDbContext _dbContext;
        private readonly ILogger<GenericRepository<TModel>> _logger;

        public GenericRepository(DataStoreDbContext dbContext, ILogger<GenericRepository<TModel>> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<TModel>> GetQuestion()
        {
            _logger.LogInformation("Getting questions from database.");
            try
            {
                return await _dbContext.Set<TModel>().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting questions from database.");
                throw;
            }
        }
    }
}
