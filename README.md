# PropertyManagement
API desarrollada con **.NET 8**, siguiendo principios de arquitectura limpia, seguridad avanzada y prácticas de calidad empresarial.  

---

## 🔐 Autenticación y Seguridad
- JWT Authentication con access y refresh tokens  
- Autorización basada en roles (Admin/User)  
- Hashing seguro de contraseñas con BCrypt  
- Validación completa de entrada de datos  
- Cumplimiento OWASP Top 10  

## 🏗️ Arquitectura 
- Clean Architecture con separación clara de capas  
- Unit of Work Pattern para transacciones atómicas  
- Generic Repository Pattern con funcionalidad específica  
- SOLID Principles aplicados consistentemente  
- Dependency Injection organizada por capas  

## 📊 Características de Negocio
- CRUD completo para propiedades, propietarios e imágenes  
- Filtrado avanzado con paginación inteligente  
- Manejo de imágenes con validación y almacenamiento seguro  
- Trazabilidad completa de cambios y transacciones  
- Soft delete para integridad referencial  
- Historial de precios y auditoría completa  

## 🧪 Calidad y Testing
- Tests unitarios y de integración
- Tests con JWT para endpoints protegidos  
- Mocking completo con Moq y NUnit  
- Integration tests con WebApplicationFactory  

## 🏛️ Estructura del Sistema
```
PropertyManagement (Solución)
│
├── src
│   ├── PropertyManagement.API
│   │   ├── Controllers
│   │   ├── DependencyInjection
│   │   ├── appsettings.json
│   │   ├── Program.cs
│   │
│   ├── PropertyManagement.Application
│   │   ├── DependencyInjection
│   │   ├── DTOs
│   │   ├── Interfaces
│   │   ├── Mappings
│   │   └── Services
│   │
│   ├── PropertyManagement.Domain
│   │   ├── Dependencias
│   │   ├── Entities
│   │   ├── Interfaces
│   │   └── Models
│   │
│   ├── PropertyManagement.Infrastructure
│   │   ├── Data
│   │   ├── DependencyInjection
│   │   ├── Repositories
│   │   ├── UnitOfWork
│
└── tests
    └── PropertyManagement.Tests
        ├── Integration
        └── Unit
```

# 🛠️ Stack Tecnológico Completo

## Backend Core
- **.NET 8** – Framework principal  
- **Entity Framework Core 8.0** – ORM con optimizaciones  
- **SQL Server** – Base de datos principal  
- **AutoMapper 13.0** – Mapeo automático de objetos  

## Seguridad
- **JWT Bearer** – Autenticación stateless  
- **BCrypt.Net** – Hashing seguro de contraseñas  
- **Microsoft.AspNetCore.Authentication.JwtBearer**  

## Logging
- **Serilog** – Logging estructurado  

## Testing
- **NUnit 4.0** – Framework de testing  
- **Moq 4.20** – Mocking framework  
- **FluentAssertions** – Assertions mejoradas  
- **Microsoft.AspNetCore.Mvc.Testing** – Integration tests  

## ⚙️ Configuración y Autenticación

### 🔧 Configurar la conexión a la base de datos
1. Edita el archivo **`appsettings.json`**.  
2. Cambia la cadena de conexión (**`ConnectionString`**) según tu entorno.  
3. ⚡ La base de datos se creará automáticamente al iniciar la aplicación.  


### 🔑 Obtener un token de autenticación
- Ejecuta el método **`login`** con las siguientes credenciales por defecto:  
  - **Username:** `admin`  
  - **Password:** `Admin123!`  
- También puedes crear tu propio usuario desde los endpoints disponibles.  

### 🔐 Autenticación en las peticiones
1. Copia el **token JWT** obtenido en el paso anterior.  
2. Inyéctalo en el encabezado de tus peticiones.
