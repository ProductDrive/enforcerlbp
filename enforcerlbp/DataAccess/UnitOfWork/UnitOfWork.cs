
using Data.Context;
using DataAccess.Repository.Implementation;
using DataAccess.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork <T>:IUnitOfWork<T> where T :class
    {
        private readonly EnforcerContext _enforcerContext;
        private  IRepository<T> _repository;

        public UnitOfWork(EnforcerContext enforcerContext)
        {
            _enforcerContext = enforcerContext;
        }

        public IRepository<T> Repository
        {
            get
            {
                return _repository = _repository ?? new Repository<T>(_enforcerContext);
            }
        }

        public async Task<int> Save()
        {
            return await _enforcerContext.SaveChangesAsync();
        }
    }
}
