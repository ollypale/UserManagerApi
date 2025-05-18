using UserManagerApi.Models.Entities;

namespace UserManagerApi.Models.Responses
{
    public class UserResponse
    {
        public UserResponse(User user)
        {
            Name = user.Name;
            Gender = user.Gender;
            Birthday = user.Birthday;
            Admin = user.Admin;
        }
        public string Name { get; set; } = null!;
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool Admin { get; set; }
    }
}
