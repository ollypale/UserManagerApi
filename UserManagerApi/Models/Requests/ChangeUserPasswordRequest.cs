using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Models.Requests
{
    public class ChangeUserPasswordRequest
    {
        [Required(ErrorMessage = "Поле 'Старый пароль' обязательно для заполнения.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Для ввода поля 'Старый пароль' используются только латинские буквы и цифры.")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Новый пароль' обязательно для заполнения.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Для ввода поля 'Новый пароль' используются только латинские буквы и цифры.")]
        public string NewPassword { get; set; } = null!;
    }
}
