using UserManagerApi.Models.Entities;

namespace UserManagerApi.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];
        public InMemoryUserRepository()
        {
            var now = DateTime.Now;

            _users.Add(new User
            {
                Id = Guid.NewGuid(),
                Login = "admin",
                Password = "admin123",
                Name = "Administrator",
                Gender = 2,
                Birthday = null,
                Admin = true,
                CreatedOn = now,
                CreatedBy = "system",
                ModifiedOn = now,
                ModifiedBy = "system"
            });
        }
        
        // Create
        public void Add(User user) => 
            _users.Add(user);

        // Read
        public List<User> GetAll() => _users;
        public User? GetByLogin(string? login) =>
            _users.SingleOrDefault(p => p.Login == login);

        // Update
        public void Update(User user)
        {
            var index = _users.FindIndex(u => u.Login == user.Login);
            if (index != -1)
                _users[index] = user;
        }

        // Delete
        public void Delete(User user)
        {
            var index = _users.FindIndex(u => u.Login == user.Login);
            if (index != -1)
                _users.RemoveAt(index);
        }
    }
}
