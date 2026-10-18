# Declaración de Uso de Inteligencia Artificial

En el desarrollo de la plataforma web para la clínica **"Fundación Hacer Más"**, se utilizó Inteligencia Artificial como herramienta de asistencia para la programación, depuración y diseño. A continuación, se detallan los usos específicos durante el ciclo de vida del software:

### 1. Generación y Estructuración de Código (Backend)
- Apoyo en la creación de Controladores en C# bajo el patrón MVC (Modelo-Vista-Controlador).
- Estructuración de la lógica de autenticación y autorización por roles (Administrador, Recepcionista, Médico, Paciente) usando `ClaimsIdentity` y Cookies.

### 2. Resolución de Errores (Debugging)
- Diagnóstico y solución de excepciones de conexión con la base de datos PostgreSQL mediante Entity Framework Core (ej. resolución del error `DbUpdateException` al registrar pacientes nuevos).
- Corrección de problemas de enrutamiento (Errores HTTP 404) al enviar parámetros nulos en el módulo de Notas Clínicas.

### 3. Diseño de Interfaz (Frontend)
- Generación de código HTML5 y uso de clases de Bootstrap 5 para construir interfaces responsivas y limpias (ej. la pantalla de Login y el panel de tarjetas en la vista `MisCitas.cshtml` para el paciente).

### 4. Lógica de Negocio
- Asistencia en la creación del algoritmo de validación para la agenda médica, asegurando que las citas se programen en intervalos exactos de 30 minutos y evitando el empalme de horarios en la base de datos.
