namespace SimpleORM.Contracts
{
    using System.Collections.Generic;
    public interface IDbContext
    {
        bool Persist(object entity);
        T FindById<T>(int id);
        IEnumerable<T> FindAll<T>();
        IEnumerable<T> FindAll<T>(string condition);
        T FindFirst<T>();
        T FindFirst<T>(string condition);
        void Delete<T>(object entity);
        void DeleteById<T>(int id);
    }
}
