// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using publishing.Areas.Identity.Data;
using publishing.Models;

namespace publishing.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<publishingUser> _signInManager;
        private readonly UserManager<publishingUser> _userManager;
        private readonly IUserStore<publishingUser> _userStore;
        private readonly IUserEmailStore<publishingUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly PublishingDBContext _context;
        //private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<publishingUser> userManager,
            IUserStore<publishingUser> userStore,
            SignInManager<publishingUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager,
            PublishingDBContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 

            [Required (ErrorMessage = "Введите наименование")]
            [Display(Name = "Наименование")]
            [DataType(DataType.Text)]
            [StringLength(50, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон:[1,50]")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Введите номер телефона")]
            [DataType(DataType.PhoneNumber)]
            [DisplayName("Номер телефона")]
            [RegularExpression(@"\+7-\d{3}-\d{3}-\d{2}-\d{2}", ErrorMessage = "Неверный номер телефона.Паттерн: +7-###-###-##-##")]
            public string Phone { get; set; }

            [Required(ErrorMessage = "Введите электронную почту")]
            [DataType(DataType.EmailAddress)]
            [DisplayName("Электронная почта")]
            [MaxLength(50)]
            [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Введена неверная электронная почта")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required (ErrorMessage = "Введите пароль")]
            [StringLength(100, ErrorMessage = "Длина пароля должна входить в диапазон [{2},{1}] символа(-ов)", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Подтвердите пароль")]
            [Compare("Password", ErrorMessage = "Пароли не совпадают")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            bool isPhoneAlreadyRegistered = _userManager.Users.Any(item => item.PhoneNumber == Input.Phone);
            bool isEmailAlreadyRegistered = _userManager.Users.Any(item => item.Email == Input.Email);
            if (ModelState.IsValid && !isPhoneAlreadyRegistered && !isEmailAlreadyRegistered)
            {
                var user = CreateUser();

                user.UserName = Input.Name;
                user.PhoneNumber = Input.Phone;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!await _roleManager.RoleExistsAsync("admin"))
                        await _roleManager.CreateAsync(new IdentityRole("admin"));

                    if (!await _roleManager.RoleExistsAsync("manager"))
                        await _roleManager.CreateAsync(new IdentityRole("manager"));

                    if (!await _roleManager.RoleExistsAsync("customer"))
                        await _roleManager.CreateAsync(new IdentityRole("customer"));

                    await _userManager.AddToRoleAsync(user, "admin");

                   
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _context.Customers.Add(new Customer(Input.Name, Input.Phone, Input.Email));
                    _context.SaveChanges();
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else 
            {
                string errorMessage = "";
                if (isEmailAlreadyRegistered && !isPhoneAlreadyRegistered)
                    errorMessage = "Ошибка регистрации. Вы ввели электронную почту, которая существует в системе";
                if (isPhoneAlreadyRegistered && !isEmailAlreadyRegistered)
                    errorMessage = "Ошибка регистрации. Вы ввели номер телефона, который существует в системе";
                if (isEmailAlreadyRegistered && isPhoneAlreadyRegistered)
                    errorMessage = "Ошибка регистрации. Вы ввели номер телефона и электронную почту, существующие в системе";
                if (!ModelState.IsValid && !isPhoneAlreadyRegistered && !isEmailAlreadyRegistered)
                    errorMessage = "Ошибка регистрации. Произошла неопознная ошибка. Повторите попытку";
                return RedirectToAction("Index", "Error", new { errorMessage = errorMessage});
            }
            // If we got this far, something failed, redisplay form
            return Page();
        }

        private publishingUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<publishingUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(publishingUser)}'. " +
                    $"Ensure that '{nameof(publishingUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<publishingUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<publishingUser>)_userStore;
        }
    }
}
