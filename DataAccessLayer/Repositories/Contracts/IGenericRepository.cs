﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using DataAccessLayer.Repositories.Contracts;


namespace DataAccessLayer.Repositories.Contracts
{
    public interface IGenericRepository<TModel> where TModel : class
    {
       
        Task<List<TModel>> GetDatums(); 
        //CRUD operations
    }
}