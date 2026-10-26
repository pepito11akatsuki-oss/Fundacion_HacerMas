using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using FundacionHacerMasWeb.Data;
using FundacionHacerMasWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FundacionHacerMasWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. LISTADO DE USUARIOS (Index)
        // ==========================================
        // GET: Mostrar lista de usuarios
        // GET: Mostrar lista de usuarios (Solo Empleados)
        public async Task<IActionResult> Index()
        {
            // Filtramos para que SOLO traiga empleados (Roles 1, 2 y 3) y excluya a Pacientes (Rol 4)
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.IdRol != 4)
                .ToListAsync();

            return View(usuarios);
        }

        // ==========================================
        // 2. DAR DE ALTA UN USUARIO (Create)
        // ==========================================
        // GET: Mostrar pantalla de alta
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol");
            return View(); // Ahora usamos la vista vacía, no pasamos modelo inicial
        }

        // POST: Guardar el nuevo usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Guardamos al usuario en la BD (Aquí PostgreSQL le asigna su ID automáticamente)
                var usuario = new Usuario
                {
                    IdRol = model.IdRol,
                    Nombres = model.Nombres,
                    Apellidos = model.Apellidos,
                    Correo = model.Correo, // Ya no forzará el arroba gracias a la vista
                    PasswordHash = model.PasswordHash,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync(); // Se genera el ID

                // 2. Lógica exclusiva si es Médico (Crear su perfil clínico)
                if (model.IdRol == 3)
                {
                    var medico = new Medico
                    {
                        IdUsuario = usuario.IdUsuario,
                        Especialidad = "Por definir",
                        Consultorio = model.Consultorio ?? "S/A",
                        // Si no pone hora, ponemos un horario por defecto
                        HoraEntrada = model.HoraEntrada ?? new TimeSpan(8, 0, 0),
                        HoraSalida = model.HoraSalida ?? new TimeSpan(16, 0, 0)
                    };
                    _context.Medicos.Add(medico);
                    await _context.SaveChangesAsync();
                }

                // 3. Le decimos al administrador qué ID se le asignó
                string prefijo = usuario.IdRol == 4 ? "P-" : "E-";
                string idGenerado = prefijo + usuario.IdUsuario.ToString("D6");

                TempData["Exito"] = $"Registro exitoso. El ID de acceso asignado es: {idGenerado}";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", model.IdRol);
            return View(model);
        }
        // ==========================================
        // 3. EDITAR UN USUARIO (Edit)
        // ==========================================
        // GET: Mostrar pantalla de edición
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // Mapeamos los datos básicos
            var model = new UsuarioViewModel
            {
                IdUsuario = usuario.IdUsuario,
                IdRol = usuario.IdRol,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Correo = usuario.Correo,
                PasswordHash = usuario.PasswordHash,
                Activo = usuario.Activo
            };

            // Si es médico (asumiendo que el ID del rol Médico es 3), buscamos sus datos clínicos
            if (usuario.IdRol == 3)
            {
                var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.IdUsuario == id);
                if (medico != null)
                {
                    model.Consultorio = medico.Consultorio;
                    model.HoraEntrada = medico.HoraEntrada;
                    model.HoraSalida = medico.HoraSalida;
                }
            }

            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", usuario.IdRol);
            return View(model);
        }

        // POST: Guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioViewModel model)
        {
            if (id != model.IdUsuario) return NotFound();
            if (ModelState.IsValid)
            {
                // 1. Buscamos el usuario (C# empieza a rastrear los cambios automáticamente)
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return NotFound();

                // Actualizamos sus datos principales
                usuario.IdRol = model.IdRol;
                usuario.Nombres = model.Nombres;
                usuario.Apellidos = model.Apellidos;
                usuario.Correo = model.Correo;
                usuario.PasswordHash = model.PasswordHash;
                usuario.Activo = model.Activo;

                // ELIMINAMOS el _context.Update(usuario); ¡Ya no es necesario!

                // 2. Lógica exclusiva para Médicos
                // 2. Lógica exclusiva para Médicos
                if (model.IdRol == 3)
                {
                    var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.IdUsuario == id);

                    if (medico == null)
                    {
                        // Si no existe en la tabla Medicos, lo creamos primero
                        medico = new Medico { IdUsuario = id, Especialidad = "Por definir" };
                        _context.Medicos.Add(medico);
                    }

                    // AHORA SÍ (que ya estamos seguros de que 'medico' no es null), le pasamos los datos
                    medico.CedulaProfesional = model.CedulaProfesional ?? "S/N";
                    medico.Consultorio = model.Consultorio ?? "S/A";

                    if (model.HoraEntrada.HasValue) medico.HoraEntrada = model.HoraEntrada.Value;
                    if (model.HoraSalida.HasValue) medico.HoraSalida = model.HoraSalida.Value;
                }

                // 3. Guardamos todo de un solo golpe
                await _context.SaveChangesAsync();

                TempData["Exito"] = "Datos actualizados correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", model.IdRol);
            return View(model);
        }

        // ==========================================
        // 4. DAR DE BAJA (Soft Delete)
        // ==========================================
        // En sistemas médicos no se borra el registro, se desactiva para no perder historial
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                usuario.Activo = false;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Usuario dado de baja (Inactivo).";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
