using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Models.Requests
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Поле 'Логин' обязательно для заполнения.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Для ввода поля 'Логин' используются только латинские буквы и цифры.")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Пароль' обязательно для заполнения.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Для ввода поля 'Пароль' используются только латинские буквы и цифры.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Поле 'Имя' обязательно для заполнения.")]
        [RegularExpression("^[a-zA-Zа-яА-ЯёЁ]+$", ErrorMessage = "Поле 'Имя' должно содержать только латинские или русские буквы.")]
        public string Name { get; set; } = null!;

        [Range(0, 2, ErrorMessage = "Пол должен быть 0 (женщина), 1 (мужчина) или 2 (неизвестно).")]
        public int Gender { get; set; } = 2;

        [DataType(DataType.Date, ErrorMessage = "Неверный формат даты.")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Поле 'Админ' обязательно для заполнения.")]
        public bool Admin { get; set; }
    }
}
