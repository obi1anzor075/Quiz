using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace PresentationLayer.Controllers
{
    public class GameController : Controller
    {
        private readonly DataStoreDbContext _dbContext;
        private int currentQuestionIndex = 0; // индекс текущего вопроса

        public GameController(DataStoreDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        
    }
}
