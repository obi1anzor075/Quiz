﻿using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(DataStoreDbContext dbContext) : base(dbContext)
        {
        }

    }
}
