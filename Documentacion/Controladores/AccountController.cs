using FundacionHacerMasWeb.Data;
using FundacionHacerMasWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FundacionHacerMasWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // MOSTRAR PANTALLA DE LOGIN
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya inició sesión, lo mandamos directo al panel
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // ==========================================
        // PROCESAR EL INICIO DE SESIÓN (A PRUEBA DE BALAS)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Usuario usuarioEncontrado = null;
            string textoIngresado = model.IdAcceso?.Trim().ToUpper() ?? "";

            // 1. LÓGICA PARA PACIENTES (Ej. P-000001)
            if (textoIngresado.StartsWith("P-"))
            {
                string numeroStr = textoIngresado.Replace("P-", "");
                if (int.TryParse(numeroStr, out int idPaciente))
                {
                    var pacienteDB = await _context.Pacientes.FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

                    if (pacienteDB != null)
                    {
                        usuarioEncontrado = await _context.Usuarios
                            .Include(u => u.Rol)
                            .FirstOrDefaultAsync(u => u.IdUsuario == pacienteDB.IdUsuario);
                    }
                }
            }
            // 2. LÓGICA PARA EMPLEADOS (Ej. E-000001)
            else if (textoIngresado.StartsWith("E-"))
            {
                string numeroStr = textoIngresado.Replace("E-", "");
                if (int.TryParse(numeroStr, out int idUsuario))
                {
                    usuarioEncontrado = await _context.Usuarios
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                }
            }
            // 3. LÓGICA POR CORREO ELECTRÓNICO (Respaldo)
            else
            {
                usuarioEncontrado = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Correo.ToUpper() == textoIngresado);
            }

            // ==========================================
            // DIAGNÓSTICO EXACTO DE ERRORES
            // ==========================================
            if (usuarioEncontrado == null)
            {
                ModelState.AddModelError("", "El ID de paciente o empleado no existe en la base de datos.");
                return View(model);
            }

            if (usuarioEncontrado.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "La contraseña es incorrecta.");
                return View(model);
            }

            // ==========================================
            // CREACIÓN DE SESIÓN (Omitimos 'Activo' por ahora)
            // ==========================================

            // Blindaje por si el rol no se guardó bien en las pruebas
            var nombreRol = usuarioEncontrado.Rol != null ? usuarioEncontrado.Rol.NombreRol : "Paciente";

            var claims = new System.Collections.Generic.List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioEncontrado.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuarioEncontrado.Nombres + " " + usuarioEncontrado.Apellidos),
                new Claim(ClaimTypes.Email, usuarioEncontrado.Correo ?? "Sin correo"),
                new Claim(ClaimTypes.Role, nombreRol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        // ==========================================
        // CERRAR SESIÓN
        // ==========================================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
