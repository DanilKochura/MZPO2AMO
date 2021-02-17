﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public interface IAccountRepo
    {
        public Task<List<AmoAccountAuth>> GetAllAccounts();
        public Task<AmoAccountAuth> GetAmoAccountById(int id);
        public Task<AmoAccountAuth> GetAmoAccountByName(string name);
        public Task<int> AddAmoAccount(AmoAccountAuth amoAccount);
        public Task<int> RemoveAmoAccount(AmoAccountAuth amoAccount);
        public Task<int> UpdateAmoAccount(AmoAccountAuth amoAccount);
    }
}
