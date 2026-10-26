using FundacionHacerMasWeb.Data;
using FundacionHacerMasWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims; // Necesario para identificar al médico activo
using System.Threading.Tasks;

namespace FundacionHacerMasWeb.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // VISTA GENERAL (Agenda Institucional)
        // ==========================================
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Index()
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            return View(citas);
        }

        // ==========================================
        // AGENDAR GET (Carga la pantalla)
        // ==========================================
        [Authorize(Roles = "Administrador,Recepcionista")]
        public IActionResult Agendar()
        {
            var medicos = _context.Medicos
                .Include(m => m.Usuario)
                .Select(m => new {
                    IdMedico = m.IdMedico,
                    DetalleMedico = "Dr. " + m.Usuario.Nombres + " " + m.Usuario.Apellidos + " | " + m.Especialidad + " | Cons: " + m.Consultorio
                }).ToList();

            ViewData["IdMedico"] = new SelectList(medicos, "IdMedico", "DetalleMedico");
            return View();
        }

        // ==========================================
        // AGENDAR POST (Guarda Cita y Paciente con validaciones)
        // ==========================================
        // ==========================================
        // AGENDAR POST (Guarda Cita y Paciente con validaciones)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Agendar(RegistroCitaViewModel model)
        {
            if (ModelState.IsValid)
            {
                DateTime fechaUtc = DateTime.SpecifyKind(model.FechaHora, DateTimeKind.Utc);
                TimeSpan horaSeleccionada = model.FechaHora.TimeOfDay;

                var medicoAsignado = await _context.Medicos.FindAsync(model.IdMedico);

                if (medicoAsignado != null)
                {
                    if (horaSeleccionada.Minutes != 0 && horaSeleccionada.Minutes != 30)
                    {
                        ModelState.AddModelError("FechaHora", "Las citas deben programarse exactamente en intervalos de 30 minutos.");
                    }

                    if (horaSeleccionada < medicoAsignado.HoraEntrada || horaSeleccionada >= medicoAsignado.HoraSalida)
                    {
                        ModelState.AddModelError("FechaHora", $"Fuera de horario. El turno de este médico es de {medicoAsignado.HoraEntrada:hh\\:mm} a {medicoAsignado.HoraSalida:hh\\:mm}.");
                    }
                }

                if (ModelState.IsValid)
                {
                    bool horarioOcupado = await _context.Citas
                        .AnyAsync(c => c.IdMedico == model.IdMedico && c.FechaHora == fechaUtc && c.Estado != "Cancelada");

                    if (horarioOcupado)
                    {
                        ModelState.AddModelError("FechaHora", "El horario seleccionado ya se encuentra ocupado. Elija otro.");
                    }
                    else
                    {
                        try
                        {
                            // 1. SOLUCIÓN: Buscar el rol dinámicamente en lugar de forzar el número 4
                            var rolPaciente = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Paciente");
                            int idRolAsignar = rolPaciente != null ? rolPaciente.IdRol : 4;

                            // Crear el Usuario
                            var nuevoUsuario = new Usuario
                            {
                                IdRol = idRolAsignar,
                                Nombres = model.Nombres,
                                Apellidos = model.Apellidos,
                                Correo = model.Correo,
                                PasswordHash = "Paciente123",
                                Activo = true
                            };
                            _context.Usuarios.Add(nuevoUsuario);
                            await _context.SaveChangesAsync();

                            // Crear el Paciente
                            var nuevoPaciente = new Paciente
                            {
                                IdUsuario = nuevoUsuario.IdUsuario,
                                FechaNacimiento = DateTime.SpecifyKind(model.FechaNacimiento, DateTimeKind.Utc),
                                ContactoEmergencia = model.ContactoEmergencia,
                                TelefonoEmergencia = model.TelefonoEmergencia
                            };
                            _context.Pacientes.Add(nuevoPaciente);
                            await _context.SaveChangesAsync();

                            // Crear la Cita
                            var nuevaCita = new Cita
                            {
                                IdPaciente = nuevoPaciente.IdPaciente,
                                IdMedico = model.IdMedico,
                                FechaHora = fechaUtc,
                                MotivoConsulta = model.MotivoConsulta,
                                Estado = "Programada",
                                IntentosReagendamiento = 0
                            };
                            _context.Citas.Add(nuevaCita);
                            await _context.SaveChangesAsync();

                            string idFormateado = "P-" + nuevoPaciente.IdPaciente.ToString("D6");
                            TempData["MensajeExito"] = $"Registro exitoso. El ID de acceso para este paciente es: {idFormateado}";

                            return RedirectToAction(nameof(Index));
                        }
                        catch (DbUpdateException ex)
                        {
                            // SOLUCIÓN: Si la base de datos se queja, mostramos el error exacto en la vista
                            string errorExacto = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            ModelState.AddModelError("", "Error en la Base de Datos: " + errorExacto);
                        }
                    }
                }
            }

            // Si hay error, recargamos la lista de médicos para la vista
            var medicosList = _context.Medicos.Include(m => m.Usuario).Select(m => new { IdMedico = m.IdMedico, DetalleMedico = "Dr. " + m.Usuario.Nombres + " " + m.Usuario.Apellidos + " | " + m.Especialidad + " | Cons: " + m.Consultorio }).ToList();
            ViewData["IdMedico"] = new SelectList(medicosList, "IdMedico", "DetalleMedico", model.IdMedico);
            return View(model);
        }

        // ==========================================
        // REAGENDAR
        // ==========================================
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Reagendar(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            if (cita.IntentosReagendamiento >= 3)
            {
                TempData["Error"] = "Esta cita ya fue reagendada el máximo de 3 veces permitido. Debe cancelarla y crear una nueva.";
                return RedirectToAction(nameof(Index));
            }

            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> Reagendar(int id, DateTime nuevaFecha)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            if (cita.IntentosReagendamiento >= 3)
            {
                ModelState.AddModelError("", "Límite de 3 reagendamientos alcanzado.");
                return View(cita);
            }

            DateTime fechaUtc = DateTime.SpecifyKind(nuevaFecha, DateTimeKind.Utc);

            bool horarioOcupado = await _context.Citas
                .AnyAsync(c => c.IdMedico == cita.IdMedico && c.FechaHora == fechaUtc && c.IdCita != id && c.Estado != "Cancelada");

            if (horarioOcupado)
            {
                ModelState.AddModelError("FechaHora", "El nuevo horario seleccionado ya está ocupado por otro paciente.");
                return View(cita);
            }

            cita.FechaHora = fechaUtc;
            cita.IntentosReagendamiento += 1;

            _context.Update(cita);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cita reagendada con éxito. Intento {cita.IntentosReagendamiento} de 3.";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // BÚSQUEDA POR PACIENTE (Admin y Recepcionista)
        // ==========================================
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> BuscarPaciente(string idPacienteBusqueda)
        {
            var citas = new System.Collections.Generic.List<Cita>();

            if (!string.IsNullOrEmpty(idPacienteBusqueda))
            {
                string numeroStr = idPacienteBusqueda.ToUpper().Replace("P-", "").Trim();

                if (int.TryParse(numeroStr, out int idReal))
                {
                    citas = await _context.Citas
                        .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                        .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                        .Where(c => c.IdPaciente == idReal)
                        .OrderByDescending(c => c.FechaHora)
                        .ToListAsync();

                    if (!citas.Any())
                    {
                        ViewBag.MensajeInfo = "No se encontró historial clínico para el paciente ingresado.";
                    }
                }
                else
                {
                    ViewBag.MensajeError = "Formato inválido. Por favor, ingrese un ID con formato correcto (Ej. P-000001).";
                }
            }

            ViewData["BusquedaActual"] = idPacienteBusqueda;
            return View(citas);
        }

        // ==========================================
        // API INTERNA: Buscador de Horarios Dinámicos
        // ==========================================
        [HttpGet]
        public async Task<JsonResult> ObtenerHorariosDisponibles(int idMedico, DateTime fecha)
        {
            var medico = await _context.Medicos.FindAsync(idMedico);
            if (medico == null) return Json(new { error = "Médico no encontrado" });

            DateTime fechaUtc = DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);
            DateTime diaSiguienteUtc = fechaUtc.AddDays(1);

            var citasDelDia = await _context.Citas
                .Where(c => c.IdMedico == idMedico &&
                            c.FechaHora >= fechaUtc &&
                            c.FechaHora < diaSiguienteUtc &&
                            c.Estado != "Cancelada")
                .Select(c => c.FechaHora)
                .ToListAsync();

            var horasOcupadas = citasDelDia.Select(c => c.TimeOfDay).ToList();

            var horarios = new System.Collections.Generic.List<object>();
            var horaActual = medico.HoraEntrada;

            while (horaActual < medico.HoraSalida)
            {
                bool ocupado = horasOcupadas.Contains(horaActual);

                horarios.Add(new
                {
                    hora = horaActual.ToString(@"hh\:mm"),
                    ocupado = ocupado
                });

                horaActual = horaActual.Add(TimeSpan.FromMinutes(30));
            }

            return Json(horarios);
        }

        // ==========================================
        // AGENDA DEL DÍA (Exclusivo para el Médico)
        // ==========================================
        [Authorize(Roles = "Medico")]
        public async Task<IActionResult> AgendaDia()
        {
            // 1. Identificamos qué usuario (médico) tiene la sesión iniciada
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdString, out int idUsuario))
            {
                // Buscamos su perfil de médico
                var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.IdUsuario == idUsuario);

                if (medico != null)
                {
                    // Traemos solo SUS citas, ordenadas por fecha
                    var misCitas = await _context.Citas
                        .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                        .Where(c => c.IdMedico == medico.IdMedico && c.Estado != "Cancelada")
                        .OrderBy(c => c.FechaHora)
                        .ToListAsync();

                    return View(misCitas);
                }
            }
            return View(new System.Collections.Generic.List<Cita>());
        }

        // ==========================================
        // NOTAS CLÍNICAS (Exclusivo para el Médico)
        // ==========================================
        // ==========================================
        // NOTAS CLÍNICAS GET (Carga los datos del paciente)
        // ==========================================
        [Authorize(Roles = "Medico")]
        public async Task<IActionResult> NotasClinicas(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null) return NotFound();

            var model = new NotasClinicasViewModel
            {
                IdCita = cita.IdCita,
                IdPacienteExpediente = "P-" + cita.IdPaciente.ToString("D6"),
                NombrePaciente = cita.Paciente.Usuario.Nombres + " " + cita.Paciente.Usuario.Apellidos,
                FechaNacimiento = cita.Paciente.FechaNacimiento.ToString("dd/MM/yyyy"),
                MotivoConsulta = cita.MotivoConsulta
            };

            return View(model);
        }

        // ==========================================
        // NOTAS CLÍNICAS POST (Guarda y finaliza la consulta)
        // ==========================================
        // ==========================================
        // NOTAS CLÍNICAS GET (Carga los datos del paciente)
        // ==========================================
        [HttpGet]
        [Authorize(Roles = "Medico")]
        public async Task<IActionResult> NotasClinicas(int? id)
        {
            // Si el médico entra desde el menú principal (sin ID de paciente)
            if (id == null || id == 0)
            {
                // Lo enviamos a su agenda para que elija al paciente a atender
                return RedirectToAction(nameof(AgendaDia));
            }

            var cita = await _context.Citas
                .Include(c => c.Paciente).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null) return NotFound();

            var model = new NotasClinicasViewModel
            {
                IdCita = cita.IdCita,
                IdPacienteExpediente = "P-" + cita.IdPaciente.ToString("D6"),
                NombrePaciente = cita.Paciente.Usuario.Nombres + " " + cita.Paciente.Usuario.Apellidos,
                FechaNacimiento = cita.Paciente.FechaNacimiento.ToString("dd/MM/yyyy"),
                MotivoConsulta = cita.MotivoConsulta
            };

            return View(model);
        }
        // ==========================================
        // MIS CITAS (Exclusivo para el Paciente)
        // ==========================================
        [Authorize(Roles = "Paciente")]
        public async Task<IActionResult> MisCitas()
        {
            // 1. Identificamos qué usuario (paciente) tiene la sesión iniciada
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdString, out int idUsuario))
            {
                // Buscamos su perfil de paciente en la base de datos
                var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.IdUsuario == idUsuario);

                if (paciente != null)
                {
                    // Traemos todo su historial de citas, incluyendo los datos de sus médicos
                    var misCitas = await _context.Citas
                        .Include(c => c.Medico).ThenInclude(m => m.Usuario)
                        .Where(c => c.IdPaciente == paciente.IdPaciente)
                        .OrderByDescending(c => c.FechaHora) // Muestra las más recientes/futuras primero
                        .ToListAsync();

                    return View(misCitas);
                }
            }
            // Si algo falla, regresamos una lista vacía
            return View(new System.Collections.Generic.List<Cita>());
        }
    }
}
