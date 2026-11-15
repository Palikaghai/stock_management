using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers;

public class AccountController : Controller
{
    private const long MaxProfilePictureSize = 5 * 1024 * 1024;
    private const long MaxProfilePictureRequestSize = MaxProfilePictureSize + 64 * 1024;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment environment,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked due to multiple failed sign-in attempts. Please try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email address already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            UserName = model.Email,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["SuccessMessage"] = "Welcome aboard! Your account has been created successfully.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        return View(CreateProfileViewModel(user));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxProfilePictureRequestSize)]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return ProfileView(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var pictureExtension = model.ProfilePicture == null
            ? null
            : await ValidateProfilePictureAsync(model.ProfilePicture);
        if (model.ProfilePicture != null && pictureExtension == null)
        {
            return ProfileView(model);
        }

        var emailExists = await _userManager.FindByEmailAsync(model.Email);
        if (emailExists != null && !string.Equals(emailExists.Id, user.Id, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.Email), "This email address is already associated with another account.");
            return ProfileView(model);
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();

        var emailUpdated = !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase);
        if (emailUpdated)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ProfileView(model);
            }
        }

        var username = model.UserName.Trim();
        if (!string.Equals(user.UserName, username, StringComparison.Ordinal))
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, username);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ProfileView(model);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return ProfileView(model);
        }

        if (model.ProfilePicture != null && pictureExtension != null)
        {
            var newFileName = await SaveProfilePictureAsync(model.ProfilePicture, pictureExtension);
            var existingPictureClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(claim => claim.Type == ApplicationUser.ProfilePictureClaimType);
            var newPictureClaim = new Claim(ApplicationUser.ProfilePictureClaimType, newFileName);
            var claimResult = existingPictureClaim == null
                ? await _userManager.AddClaimAsync(user, newPictureClaim)
                : await _userManager.ReplaceClaimAsync(user, existingPictureClaim, newPictureClaim);

            if (!claimResult.Succeeded)
            {
                DeleteProfilePictureFile(newFileName);
                foreach (var error in claimResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ProfileView(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            if (existingPictureClaim != null && existingPictureClaim.Value != newFileName)
            {
                DeleteProfilePictureFile(existingPictureClaim.Value);
            }
        }
        else
        {
            await _signInManager.RefreshSignInAsync(user);
        }

        TempData["SuccessMessage"] = "Your profile has been updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProfilePicture()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var pictureClaim = (await _userManager.GetClaimsAsync(user))
            .FirstOrDefault(claim => claim.Type == ApplicationUser.ProfilePictureClaimType);
        if (pictureClaim == null)
        {
            return RedirectToAction(nameof(Profile));
        }

        var result = await _userManager.RemoveClaimAsync(user, pictureClaim);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = $"Your profile picture could not be removed: {string.Join(" ", result.Errors.Select(error => error.Description))}";
            return RedirectToAction(nameof(Profile));
        }

        await _signInManager.RefreshSignInAsync(user);
        DeleteProfilePictureFile(pictureClaim.Value);
        TempData["SuccessMessage"] = "Your profile picture has been removed.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public IActionResult ProfilePicture()
    {
        var fileName = User.FindFirstValue(ApplicationUser.ProfilePictureClaimType);
        if (!TryGetProfilePictureLocation(fileName, out var path, out var contentType))
        {
            return NotFound();
        }

        Response.Headers.CacheControl = "private, max-age=3600";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, contentType);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            return View("Profile", CreateProfileViewModel(currentUser));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Profile", CreateProfileViewModel(user));
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Your password was changed successfully.";
        return RedirectToAction(nameof(Profile));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    private ProfileViewModel CreateProfileViewModel(ApplicationUser user) => new()
    {
        FullName = user.FullName ?? string.Empty,
        UserName = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber,
        ProfilePictureUrl = GetProfilePictureUrl()
    };

    private IActionResult ProfileView(ProfileViewModel model)
    {
        model.ProfilePictureUrl = GetProfilePictureUrl();
        return View(nameof(Profile), model);
    }

    private string? GetProfilePictureUrl()
    {
        var fileName = User.FindFirstValue(ApplicationUser.ProfilePictureClaimType);
        return TryGetProfilePictureLocation(fileName, out _, out _)
            ? Url.Action(nameof(ProfilePicture), new { v = fileName })
            : null;
    }

    private async Task<string?> ValidateProfilePictureAsync(IFormFile picture)
    {
        if (picture.Length == 0)
        {
            ModelState.AddModelError(nameof(ProfileViewModel.ProfilePicture), "Choose an image to upload.");
            return null;
        }

        if (picture.Length > MaxProfilePictureSize)
        {
            ModelState.AddModelError(nameof(ProfileViewModel.ProfilePicture), "The profile picture must be 5 MB or smaller.");
            return null;
        }

        var header = new byte[12];
        await using var stream = picture.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory(), HttpContext.RequestAborted);
        var extension = GetImageExtension(header, bytesRead);
        if (extension == null)
        {
            ModelState.AddModelError(nameof(ProfileViewModel.ProfilePicture), "Upload a valid JPG, PNG, GIF, or WEBP image.");
        }

        return extension;
    }

    private static string? GetImageExtension(byte[] header, int bytesRead)
    {
        if (bytesRead >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff)
        {
            return ".jpg";
        }

        if (bytesRead >= 8 &&
            header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4e && header[3] == 0x47 &&
            header[4] == 0x0d && header[5] == 0x0a && header[6] == 0x1a && header[7] == 0x0a)
        {
            return ".png";
        }

        if (bytesRead >= 6)
        {
            var signature = Encoding.ASCII.GetString(header, 0, 6);
            if (signature is "GIF87a" or "GIF89a")
            {
                return ".gif";
            }
        }

        if (bytesRead >= 12 &&
            Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
            Encoding.ASCII.GetString(header, 8, 4) == "WEBP")
        {
            return ".webp";
        }

        return null;
    }

    private async Task<string> SaveProfilePictureAsync(IFormFile picture, string extension)
    {
        var directory = ProfilePictureDirectory;
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(directory, fileName);
        await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
        await picture.CopyToAsync(stream, HttpContext.RequestAborted);
        return fileName;
    }

    private void DeleteProfilePictureFile(string fileName)
    {
        if (!TryGetProfilePictureLocation(fileName, out var path, out _))
        {
            return;
        }

        try
        {
            System.IO.File.Delete(path);
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Could not remove a profile picture file.");
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(exception, "Could not remove a profile picture file.");
        }
    }

    private bool TryGetProfilePictureLocation(string? fileName, out string path, out string contentType)
    {
        path = string.Empty;
        contentType = string.Empty;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var fileId = Path.GetFileNameWithoutExtension(fileName);
        if (!Guid.TryParseExact(fileId, "N", out _) ||
            Path.GetFileName(fileName) != fileName ||
            extension is not (".jpg" or ".png" or ".gif" or ".webp"))
        {
            return false;
        }

        path = Path.Combine(ProfilePictureDirectory, fileName);
        if (!System.IO.File.Exists(path))
        {
            return false;
        }

        contentType = extension switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => throw new InvalidOperationException("Unsupported profile picture format.")
        };
        return true;
    }

    private string ProfilePictureDirectory => Path.Combine(_environment.ContentRootPath, "App_Data", "ProfilePictures");
}
