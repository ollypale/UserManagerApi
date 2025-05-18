using System.ComponentModel.DataAnnotations;

namespace UserManagerApi.Models.Requests
{
    public class UpdateUserInfoRequest
    {
        [RegularExpression("^[a-zA-Zа-яА-ЯёЁ]+$", ErrorMessage = "Поле 'Имя' должно содержать только латинские или русские буквы.")]
        public string? Name { get; set; }

        [Range(0, 2, ErrorMessage = "Пол должен быть 0 (женщина), 1 (мужчина) или 2 (неизвестно).")]
        public int? Gender { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Неверный формат даты.")]
        public DateTime? Birthday { get; set; }
    }
}
