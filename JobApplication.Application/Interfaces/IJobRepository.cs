using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces
{
    public interface IJobRepository
    {
        Task InsertAsync(Job job);
        void Update(Job job);
        IQueryable<Job> Get();
        Task<Job?> GetByIdAsync(int id);
        void Remove(Job job);
        Task SaveChangesAsync();
    }
}
