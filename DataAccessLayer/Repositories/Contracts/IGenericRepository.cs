using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Contracts
{
    public interface IGenericRepository<TModel> where TModel : class
    {

        Task<List<Question>> GetQuestion();
    }
}
