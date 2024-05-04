using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using BusinessLogicLayer.Services.Contracts;

namespace BusinessLogicLayer.Services
{
    public class QuestionsService : IQuestionsService
    {

        private readonly IGenericRepository<Question> _repository;

        public QuestionsService(IGenericRepository<Question> repository)
        {
            _repository = repository;
        }
        public async Task<List<Question>> GetQuestion()
        {
            try
            {
                return await _repository.GetQuestion();
            }
            catch
            {
                throw;
            }
        }
    }
}
