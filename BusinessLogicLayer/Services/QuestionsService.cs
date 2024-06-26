using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using BusinessLogicLayer.Services.Contracts;

namespace BusinessLogicLayer.Services
{
    public class QuestionsService : IQuestionsService
    {
        private readonly IGenericRepository<Question> _questionRepository;

        public QuestionsService(IGenericRepository<Question> questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            var questions = await _questionRepository.GetAllAsync();
            return questions.ToList(); // Явное преобразование
        }

        public async Task<List<Question>> GetQuestion()
        {
            var questions = await _questionRepository.GetQuestion();
            return questions.ToList();
        }
    }
}
