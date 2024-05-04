﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccessLayer.Models;


namespace BusinessLogicLayer.Services.Contracts
{
    public interface IDatumService
    {
        Task<List<Datum>> GetDatums();
    }
}
