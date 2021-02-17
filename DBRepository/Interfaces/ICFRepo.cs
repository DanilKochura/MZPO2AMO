﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZPO.DBRepository
{
    public interface ICFRepo
    {
        public Task<List<CF>> GetAllCFs();
        public Task<CF> GetCFById(int id, int amoId);
        public Task<CF> GetCFByName(string name, int amoId);
        public Task<int> AddCF(CF cf);
        public Task<int> RemoveCF(CF cf);
        public Task<int> UpdateCF(CF cf);
    }
}
