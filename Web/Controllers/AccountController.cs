﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPortal.Models.AccountModels;
using MusicPortal.BLL.Interfaces;
using MusicPortal.BLL.ModelsDTO;
using MusicPortal.BLL.Services;
using MusicPortal.DAL.Repositories;

namespace MusicPortal.Controllers
{
    public class AccountController : Controller
    {
        // Services 
        private readonly IAccountService _accountService;
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _appEnvironment;

        // Constructor 
        public AccountController(IAccountService AccountService, IImageService ImageService, IWebHostEnvironment AppEnvironment)
        {
            _accountService = AccountService;
            _imageService = ImageService;
            _appEnvironment = AppEnvironment;
        }

        // SignUp
        [HttpGet]
        public ActionResult SignUp() => View();

        // SignUp
        [HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUp model, IFormFile imageAvatar)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var existingUser = await _accountService.GetByLogin(model.Login);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "This login is taken!");
                    return View(model);
                }

                await CreateUser(model.Login, model.Password, imageAvatar);

                return RedirectToAction("SignIn");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while signing up.");
                return View(model);
            }
        }

        // Save Account Image
        private async Task<int> SaveImage(IFormFile image)
        {
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var imagePath = Path.Combine(_appEnvironment.WebRootPath, "images", imageName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Создание объекта Image в базе данных и возврат его Id
            var createdImage = await _imageService.Create(new ImageDTO { Path = "/images/" + imageName });
            return createdImage.Id;
        }

        // Create User
        private async Task CreateUser(string login, string password, IFormFile imageAvatar)
        {
            var imageId = await SaveImage(imageAvatar);

            // Создание DTO для изображения
            var imageDto = new ImageDTO { Id = imageId };

            // Создание DTO для пользователя
            var userDto = new UserDTO
            {
                Login = login,
                Password = password,
                Image = imageDto,
                ImageId = imageDto.Id,
            };

            // Создание пользователя
            await _accountService.Create(userDto);
        }





        [HttpGet]
        public ActionResult SignIn() => View();

        // GET: AccountController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignIn Model)
        {
            try
            {
                if ((await _accountService.GetAll()).Count == 0)
                    throw new InvalidOperationException();

                if (!ModelState.IsValid)
                    return View(Model);

                return await AuthenticateUser(Model);
            }
            catch (InvalidOperationException ex)
            {
                return RedirectToAction("SignUp", "Account");
            }
        }

        // Login User
        private async Task<IActionResult> AuthenticateUser(SignIn Model)
        {
            var userDTO = await _accountService.GetByLogin(Model.Login);

            if (userDTO == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(Model);
            }

            var isPasswordCorrect = await _accountService.ValidateUserPassword(userDTO, Model.Password);

            if (!isPasswordCorrect)
            {
                ModelState.AddModelError("", "Incorrect login or password!");
                return View(Model);
            }

            HttpContext.Session.SetString("Login", userDTO.Login);

            var imageDTO = await _imageService.GetById(userDTO.ImageId);
            string imagePath = imageDTO.Path;

            HttpContext.Session.SetString("UserId", userDTO.Id.ToString());
            HttpContext.Session.SetString("UserImage", imagePath);
            HttpContext.Session.SetString("IsAdmin", userDTO.IsAdmin.ToString());
            return RedirectToAction("Index", "Home");
        }






        public async Task<IActionResult> ConfirmUsers()
        {
            var model = new ConfirmUsers();
            model.Users = (await _accountService.GetAll()).Select(userDTO =>
                new User
                {
                    Id = userDTO.Id,
                    Login = userDTO.Login,
                    ImageId = userDTO.ImageId,
                }).ToList();

            model.ImagePaths = new Dictionary<int, string>();
            foreach (var user in model.Users)
            {
                if (user.ImageId > 0)
                {
                    var imageDTO = await _imageService.GetById(user.ImageId);
                    string imagePath = imageDTO.Path;
                    model.ImagePaths.Add(user.Id, imagePath);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> AuthUser(int id)
        {
            // Получаем пользователя по id
            var userDTO = await _accountService.GetById(id);
            if (userDTO == null)
            {
                return NotFound();
            }

            // Устанавливаем свойство IsAuth в true
            userDTO.IsAuth = true;

            // Обновляем пользователя в базе данных
            await _accountService.Update(userDTO);

            // Получаем список всех пользователей
            var usersDTO = await _accountService.GetAll();

            // Преобразование пользователей из DTO в модели представления
            var users = usersDTO.Select(u => new User { Id = u.Id, Login = u.Login }).ToList();

            // Создаем модель для представления
            var model = new ConfirmUsers
            {
                Users = users,
                User = new User { Id = userDTO.Id, Login = userDTO.Login }, // Преобразование пользователя из DTO в модель представления
                Id = id,
                ImagePaths = new Dictionary<int, string>()
            };

            // Получаем пути к изображениям для всех пользователей
            foreach (var u in usersDTO)
            {
                if (u.Image != null && u.Image.Id > 0)
                {
                    var imageDTO = await _imageService.GetById(u.Image.Id);
                    if (imageDTO != null)
                    {
                        model.ImagePaths.Add(u.Id, imageDTO.Path);
                    }
                }
            }

            // Возвращаем представление с моделью
            return View("~/Views/Account/ConfirmUsers.cshtml", model);
        }

        // GET: AccountController
        public async Task<IActionResult> Index(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _accountService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = new User
            {
                Login = user.Login,
                ImageId = user.ImageId
            };

            return View(userDto);
        }

        // GET: AccountController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AccountController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _accountService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _accountService.GetById(user.Id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Обновляем данные существующего пользователя
                    existingUser.Login = user.Login;
                    existingUser.Password = user.Password; // Предположим, что здесь нужно обновить пароль, если пользователь его изменил
                    existingUser.IsAdmin = user.IsAdmin;

                    await _accountService.Update(existingUser);

                    return RedirectToAction("ConfirmUsers");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            return View(user);
        }

        // GET: AccountController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
			await _accountService.Delete(id);
			return RedirectToAction("ConfirmUsers");
		}

	}
}
