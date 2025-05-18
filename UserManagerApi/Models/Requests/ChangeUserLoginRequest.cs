using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Models.Requests
{
    public class ChangeUserLoginRequest
    {
        [Required(ErrorMessage = "Поле 'Логин' обязательно для заполнения.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Для ввода поля 'Логин' используются только латинские буквы и цифры.")]
        public string NewLogin { get; set; } = null!;
    }
}
