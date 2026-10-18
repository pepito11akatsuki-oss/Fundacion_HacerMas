# Fundacion_HacerMas
Proyecto de Software: Sistema de Gestión Clínica "Fundación Hacer Más"

Integrantes:

José de Jesús Escobedo Valenzuela
Fernanda Carolina Limón Gómez

Descripción del proyecto:
El proyecto consiste en el desarrollo de una plataforma web integral para la gestión de citas médicas y control de expedientes clínicos de la "Fundación Hacer Más", cuyo propósito es digitalizar, automatizar y asegurar los procesos administrativos e informáticos de la clínica. El sistema funciona mediante una arquitectura conectada a una base de datos relacional, donde se almacena la información de manera centralizada bajo un estricto control de acceso por roles (Administrador, Recepcionista, Médico y Paciente).

A través de una interfaz corporativa, responsiva e intuitiva, el personal de recepción puede registrar nuevos ingresos y programar citas mediante un algoritmo inteligente que valida la disponibilidad de los médicos en intervalos exactos de 30 minutos, evitando empalmes de horarios. Los médicos cuentan con un módulo exclusivo para visualizar su agenda diaria y capturar el registro clínico (signos vitales, diagnóstico CIE-10 y prescripción de tratamientos) directamente en el expediente digital. Finalmente, la plataforma otorga a los pacientes un portal de acceso institucional mediante un ID único (ej. P-000001) para consultar el estatus e historial de sus consultas médicas, optimizando así el flujo de trabajo, reduciendo errores de captura y mejorando la calidad de atención.

Tecnologías utilizadas:

Servidor: IIS / Kestrel (Integrados en ASP.NET Core)

Base de datos: PostgreSQL (Administrada a través de Entity Framework Core)

Backend: C# bajo el framework .NET (Arquitectura MVC - Modelo, Vista, Controlador)

Frontend: HTML5, CSS3, Bootstrap 5 y sintaxis Razor (.cshtml) para interfaces dinámicas.

GitHub: Para el control de versiones, integración del código fuente y organización colaborativa del equipo de desarrollo.
