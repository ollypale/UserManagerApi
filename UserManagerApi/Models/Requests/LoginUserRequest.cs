using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Models.Requests
{
    public class LoginUserRequest
    {
        [Required(ErrorMessage = "Поле 'Логин' обязательно для заполнения.")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Пароль' обязательно для заполнения.")]
        public string Password { get; set; } = null!;
    }
}
