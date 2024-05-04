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
    public class DatumService : IDatumService
    {

        private readonly IGenericRepository<Datum> _repository;

        public DatumService(IGenericRepository<Datum> repository)
        {

            _repository = repository;

        }
        public async Task<List<Datum>> GetDatums()
        {
            try
            {
                return await _repository.GetDatums();
            }
            catch
            {
                throw;
            }
        }
    }
}
