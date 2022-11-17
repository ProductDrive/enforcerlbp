using DataAccess.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.UnitOfWork
{
    public interface IUnitOfWork<T> where T : class
    {
        IRepository<T> Repository { get; }

        Task<int> Save();
    }
}
