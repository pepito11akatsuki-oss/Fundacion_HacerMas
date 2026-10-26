### 0. Fase de Planeación, Arquitectura y Reglas de Negocio
**Prompt ingresado:**
"Actúa como un arquitecto de software. Mi compañera Fernanda Carolina Limón Gómez y yo vamos a desarrollar una plataforma web para la clínica 'Fundación Hacer Más'. Necesito que estructures el funcionamiento general del sistema. Queremos usar C# con ASP.NET Core MVC y PostgreSQL. Define cómo se deben regir los permisos para 4 roles (Administrador, Recepcionista, Médico, Paciente) y propón la lógica central para gestionar la agenda médica evitando conflictos de horarios y limitando la reprogramación."

**Resultado generado por la IA (Estrategia adoptada):**
*   **Arquitectura Recomendada:** Uso del patrón MVC para separar la interfaz gráfica de la lógica, implementando Entity Framework Core como ORM para mapear la base de datos PostgreSQL.
*   **Gobierno y Control de Acceso:**
    *   *Administrador:* Control total del sistema, alta de personal (médicos/recepcionistas) y asignación de credenciales.
    *   *Recepcionista:* Registro de pacientes de nuevo ingreso y administración del tablero general de citas.
    *   *Médico:* Acceso restringido exclusivamente a su propia agenda diaria y habilitación del módulo de redacción de notas clínicas.
    *   *Paciente:* Portal de solo lectura para consultar historial de consultas, con bloqueo de reprogramación ilimitada para evitar desorden en la clínica.
*   **Regla de Negocio (Agenda):** Se diseñará un algoritmo que valide intervalos exactos de 30 minutos y cruce la solicitud con la tabla de citas activas del médico, rechazando automáticamente cualquier registro que genere empalme o exceda el turno laboral.

### 0.2 Diseño del Modelo Relacional y Base de Datos
**Prompt ingresado:**
"Basado en los roles y reglas de negocio de la clínica, ayúdame a diseñar las tablas principales para la base de datos en PostgreSQL utilizando Entity Framework Core. Necesito las entidades en C# para Usuario, Rol, Médico, Paciente y Cita, especificando sus atributos principales y cómo se relacionan entre sí mediante llaves foráneas."

**Resultado generado por la IA (Estrategia adoptada):**
*   **Entidades Centrales:** Creación de las clases `Usuario` (tabla base de autenticación), `Rol` (catálogo de permisos), `Medico` y `Paciente` (datos clínicos e institucionales), y `Cita` (tabla transaccional).
*   **Relaciones (Llaves Foráneas):** Se estableció que un `Rol` tiene muchos `Usuarios` (1:N), y que una `Cita` pertenece a un `Paciente` y es atendida por un `Medico`.
*   **Contexto de Datos:** Configuración del `ApplicationDbContext` vinculando las tablas mediante `DbSet<T>` y estableciendo reglas de eliminación restrictiva para evitar que borrar un médico elimine accidentalmente el historial de citas.

### 0.3 Estructura de la Interfaz y Experiencia de Usuario (UI/UX)
**Prompt ingresado:**
"Diseña la estructura base del Frontend (Layout) para ASP.NET Core usando HTML5 y Bootstrap 5. Queremos un diseño limpio e institucional para la 'Fundación Hacer Más'. El menú de navegación debe ser dinámico, es decir, que oculte o muestre botones (como 'Mi Agenda' o 'Administrar Usuarios') dependiendo del rol del usuario que haya iniciado sesión."

**Resultado generado por la IA (Estrategia adoptada):**
*   **Plantilla Principal (`_Layout.cshtml`):** Desarrollo de una estructura responsiva con barra de navegación superior (Navbar) y pie de página.
*   **Navegación Dinámica:** Implementación de sentencias Razor (ej. `@if (User.IsInRole("Médico"))`) para leer la sesión en tiempo real y renderizar únicamente los accesos autorizados, reforzando la seguridad visual del sistema.
