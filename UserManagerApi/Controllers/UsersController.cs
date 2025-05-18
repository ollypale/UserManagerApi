using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagerApi.Models.Entities;
using UserManagerApi.Models.Requests;
using UserManagerApi.Models.Responses;
using UserManagerApi.Repositories;

namespace UserManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        public UsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        // Create
        // 1) Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли пользователь админом (Доступно Админам)
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserRequest request)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_repo.GetByLogin(request.Login.ToLowerInvariant()) is not null)
                return Conflict("Логин уже занят.");

            var time = DateTime.Now;

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Login = request.Login.ToLowerInvariant(),
                Password = request.Password,
                Name = request.Name,
                Gender = request.Gender,
                Birthday = request.Birthday,
                Admin = request.Admin,
                CreatedOn = time,
                CreatedBy = User.Identity!.Name!,
                ModifiedOn = time,
                ModifiedBy = User.Identity!.Name!
            };

            _repo.Add(newUser);
            return CreatedAtAction(nameof(GetUserByLogin), new { login = newUser.Login }, newUser);
        }

        // Update-1
        // 2) Изменение имени, пола или даты рождения пользователя (Может менять Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))

        [Authorize]
        [HttpPut("{login}/info")]
        public IActionResult UpdateUserInfo(string login, [FromBody] UpdateUserInfoRequest request)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            if (!User.IsInRole("Admin") && User.Identity!.Name != login)
                return StatusCode(403, "Изменять пользователя может только активный администратор или сам пользователь.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;
            if (request.Gender.HasValue)
                user.Gender = (int)request.Gender!;
            if (request.Birthday.HasValue)
                user.Birthday = request.Birthday;

            user.ModifiedBy = User.Identity!.Name!;
            user.ModifiedOn = DateTime.Now;

            _repo.Update(user);

            return Ok(new
            {
                Message = "Информация о пользователе успешно обновлена.",
                Response = user
            });
        }

        // 3) Изменение пароля (Пароль может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn))

        [Authorize]
        [HttpPut("{login}/password")]
        public IActionResult ChangeUserPassword(string login, [FromBody] ChangeUserPasswordRequest request)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            if (!User.IsInRole("Admin") && User.Identity!.Name != login)
                return StatusCode(403, "Изменять пользователя может только активный администратор или сам пользователь.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (user.Password != request.OldPassword)
                return BadRequest("Старый пароль неверен.");

            user.Password = request.NewPassword;
            user.ModifiedBy = User.Identity!.Name!;
            user.ModifiedOn = DateTime.Now;

            _repo.Update(user);

            return Ok(new
            {
                Message = "Пароль успешно изменён.",
                UpdatedUser = user
            });
        }

        // 4) Изменение логина (Логин может менять либо Администратор, либо лично пользователь, если он активен(отсутствует RevokedOn), логин должен оставаться уникальным)
        
        [Authorize]
        [HttpPut("{login}/login")]
        public IActionResult ChangeUserLogin(string login, [FromBody] ChangeUserLoginRequest request)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            if (!User.IsInRole("Admin") && User.Identity!.Name != login)
                return StatusCode(403, "Изменять пользователя может только активный администратор или сам пользователь.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_repo.GetByLogin(request.NewLogin.ToLowerInvariant()) is not null)
                return Conflict("Логин уже занят.");

            user.Login = request.NewLogin.ToLowerInvariant();

            if (User.IsInRole("Admin"))
                user.ModifiedBy = User.Identity!.Name!;
            else
                user.ModifiedBy = request.NewLogin;

            user.ModifiedOn = DateTime.Now;

            _repo.Update(user);

            return Ok(new
            {
                Message = "Логин успешно изменён.",
                UpdatedUser = user,
                NewLocation = Url.Action(nameof(GetUserByLogin), new { login = user.Login })
            });
        }

        // Read
        // 5) Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по CreatedOn(Доступно Админам)
        
        [Authorize(Roles = "Admin")]
        [HttpGet("active")]
        public IActionResult GetActiveUsers()
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var activeUsers = _repo.GetAll()
                .Where(u => u.RevokedOn is null)
                .OrderBy(u => u.CreatedOn)
                .ToList();

            return Ok(activeUsers);
        }

        // 6) Запрос пользователя по логину, в списке должны быть имя, пол и дата рождения статус активный или нет (Доступно Админам)
        
        [Authorize(Roles = "Admin")]
        [HttpPost("{login}")]
        public IActionResult GetUserByLogin(string login)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            return Ok(new UserResponse(user));
        }

        // 7) Запрос пользователя по логину и паролю (Доступно только самому пользователю, если он активен (отсутствует RevokedOn))
        
        [Authorize]
        [HttpPost("login")]
        public IActionResult GetMyInfo()
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            return Ok(currentUser);
        }

        // 8) Запрос всех пользователей старше определённого возраста (Доступно Админам)
        
        [Authorize(Roles = "Admin")]
        [HttpGet("older-then/{age}")]
        public IActionResult GetUsersOlderThan(int age)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var today = DateTime.Today;

            var usersOlderThan = _repo.GetAll()
                .Where(u => u.Birthday is not null && u.RevokedOn is null)
                .Where(u => (today.Year - u.Birthday!.Value.Year) - (today < u.Birthday.Value.AddYears(today.Year - u.Birthday.Value.Year) ? 1 : 0) > age)
                .OrderBy(u => u.Birthday)
                .ToList();

            return Ok(usersOlderThan);
        }

        // Delete
        // 9) Удаление пользователя по логину полное или мягкое (При мягком удалении должна происходить простановка RevokedOn и RevokedBy) (Доступно Админам)
        
        [Authorize(Roles = "Admin")]
        [HttpDelete("{login}")]
        public IActionResult SoftDeleteUser(string login)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            var time = DateTime.Now;

            user.RevokedBy = User.Identity!.Name!;
            user.RevokedOn = time;
            user.ModifiedBy = User.Identity!.Name!;
            user.ModifiedOn = time;

            _repo.Update(user);

            return Ok("Пользователь успешно удалён.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{login}/hard")]
        public IActionResult HardDeleteUser(string login)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            _repo.Delete(user);

            return Ok("Пользователь успешно удалён.");
        }

        // Update-2
        // 10) Восстановление пользователя - Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)

        [Authorize(Roles = "Admin")]
        [HttpPut("{login}/recovery")]
        public IActionResult RecoverUser(string login)
        {
            var currentUser = TryGetAuthenticatedUser();
            if (currentUser == null)
                return Unauthorized("Вы не авторизованы или ваша учётная запись недействительна.");

            var user = _repo.GetByLogin(login.ToLowerInvariant());
            if (user is null)
                return NotFound("Пользователь не найден.");

            if (user.RevokedOn == null)
                return BadRequest("Пользователь уже активен и не требует восстановления.");

            user.RevokedBy = null;
            user.RevokedOn = null;
            user.ModifiedBy = User.Identity!.Name!;
            user.ModifiedOn = DateTime.Now;

            _repo.Update(user);

            return Ok(new
            {
                Message = "Пользователь успешно восстановлен.",
                UpdatedUser = user
            });
        }

        private User? TryGetAuthenticatedUser()
        {
            var login = User.Identity?.Name?.ToLowerInvariant();
            var user = _repo.GetByLogin(login);

            if (user is null || user.RevokedOn != null)
                return null;

            return user;
        }
    }
}
