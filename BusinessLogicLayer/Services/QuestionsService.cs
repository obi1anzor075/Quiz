using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using BusinessLogicLayer.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace BusinessLogicLayer.Services
{
    public class QuestionsService : IQuestionsService
    {
        private readonly IGenericRepository<Question> _repository;
        private readonly ILogger<QuestionsService> _logger;

        public QuestionsService(IGenericRepository<Question> repository, ILogger<QuestionsService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<Question>> GetQuestion()
        {
            _logger.LogInformation("Getting questions from repository.");
            try
            {
                return await _repository.GetQuestion();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting questions.");
                throw;
            }
        }
    }
}
