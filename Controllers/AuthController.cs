using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Authorization.Controllers
{
    public class AuthController : Controller
    {

        private readonly MyDbContext _context;
        private readonly JwtService _jwtService;
        private readonly SignInManager<AppUser> signInManager;
        private readonly RoleManager<AppRole> roleManager;
        public AuthController(MyDbContext myDbContext, JwtService jwtService, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager,IHttpContextAccessor httpContext)
        {
            this._context = myDbContext;
            this._jwtService = jwtService;
            this.roleManager = roleManager;
            this.signInManager = signInManager;

            // Assuming _context is your database context and it has been injected
            var roles = _context.Roles.ToList();  // Get the list of roles

            if (roles != null && roles.Any())
            {
                // Serialize the roles list to JSON
                var rolesJson = JsonConvert.SerializeObject(roles);
                httpContext.HttpContext.Session.SetString("roles", rolesJson);

                // Store the serialized data in session
                //HttpContext.Session.SetString("roles", rolesJson);
            }
            else
            {
                HttpContext.Session.SetString("roles", "");  // Or any placeholder when roles are empty
            }
        }


        public IActionResult LoginView()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            AppUser user = null;//this._context.Users.FirstOrDefault(u => u.Name == model.Username && u.Password == model.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            var token = _jwtService.GenerateToken(model.Username);

            return Json(token);
        }

        [Authorize()]
        //[Authorize(Policy = "Admin")]
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public IActionResult GetData()
        {
            var count = HttpContext.Session.GetInt32("requestcount") ?? 0;
            count++;
            HttpContext.Session.SetInt32("requestcount", count);

            return Ok(count);
        }

        [Authorize()]
        public IActionResult Home()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View("RegisterMe", new LoginViewModel());
        }

        public async Task<IActionResult> RegisterMe(LoginViewModel model)
        {
            var user = await signInManager.UserManager.FindByNameAsync(model.Username);

            if (user == null)  // User doesn't exist, create a new one
            {
                var identityUser = new AppUser { UserName = model.Username };

                // Create user (Identity hashes the password)
                var createResult = await signInManager.UserManager.CreateAsync(identityUser, model.Password);

                if (createResult.Succeeded)
                {
                    // Sign in the user after successful registration
                    await signInManager.SignInAsync(identityUser, isPersistent: false);
                    return RedirectToAction("Home");
                }
                else
                {
                    // Handle errors (e.g., password not meeting requirements)
                    return View("RegisterMe", createResult.Errors);
                }
            }

            // If user exists, attempt to sign in
            var loginResult = await signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (loginResult.Succeeded)
            {
                return RedirectToAction("Home");
            }

            // Handle failed login (wrong password, locked out, etc.)
            ModelState.AddModelError("", "Invalid login attempt.");
            return View("LoginView");

        }
    }

    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

    }

}
