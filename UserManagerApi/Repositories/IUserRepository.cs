using UserManagerApi.Models.Entities;

namespace UserManagerApi.Repositories
{
    public interface IUserRepository
    {
        // Create
        void Add(User user);
        // Read
        List<User> GetAll();
        User? GetByLogin(string? login);
        // Update
        void Update(User user);
        // Delete
        void Delete(User user);
    }
}
