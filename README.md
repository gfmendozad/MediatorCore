# QuickMediator

Una librería ligera y eficiente para implementar el patrón **CQRS (Command Query Responsibility Segregation)** en .NET, inspirada en MediatR pero con nombres únicos y funcionalidades personalizadas.

## 🚀 Características

- ✅ **Separación clara** entre Commands (escritura) y Queries (lectura)
- ✅ **Pipeline Behaviors** para cross-cutting concerns (validación, logging, caching)
- ✅ **Auto-registro** de handlers y validators
- ✅ **Validación integrada** con FluentValidation
- ✅ **Completamente asíncrono**
- ✅ **Inyección de dependencias** nativa
- ✅ **Respuestas tipadas** personalizables

## 📦 Instalación

```bash
dotnet add package QuickMediator
```

**Dependencias requeridas:**
```bash
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
```

## ⚙️ Configuración Básica

### 1. Registro en Program.cs

```csharp
using QuickMediator.Extensions;
using QuickMediator.Pipeline;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Registrar QuickMediator (auto-descubre handlers y validators)
builder.Services.AddQuickMediator(typeof(Program).Assembly);

// Opcional: Agregar ValidationBehavior al pipeline
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

app.MapControllers();
app.Run();
```

### 2. Crear clase Response (opcional pero recomendado)

```csharp
namespace MiApp.Common;

public class Response<T>
{
    public Response() { }

    public Response(T data, string message = null)
    {
        Succeeded = true;
        Message = message;
        Data = data;
    }

    public Response(string message)
    {
        Succeeded = false;
        Message = message;
    }

    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public T Data { get; set; }
}
```

## 📝 Uso Básico

### Commands (Escritura)

```csharp
// Command
public class CreateUserCommand : ICommand<Response<UserDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Handler (en el mismo archivo)
public class CreateUserHandler : ICommandHandler<CreateUserCommand, Response<UserDto>>
{
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response<UserDto>> ExecuteAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = new User { Name = command.Name, Email = command.Email };
            var createdUser = await _repository.CreateAsync(user);
            
            var userDto = new UserDto { Id = createdUser.Id, Name = createdUser.Name, Email = createdUser.Email };
            
            return new Response<UserDto>(userDto, "Usuario creado exitosamente");
        }
        catch (Exception ex)
        {
            return new Response<UserDto>("Error al crear usuario")
            {
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
```

### Queries (Lectura)

```csharp
// Query
public class GetAllUsersQuery : IQuery<Response<List<UserDto>>>
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
}

// Handler (en el mismo archivo)
public class GetAllUsersHandler : IQueryHandler<GetAllUsersQuery, Response<List<UserDto>>>
{
    private readonly IUserRepository _repository;

    public GetAllUsersHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response<List<UserDto>>> HandleAsync(GetAllUsersQuery query, CancellationToken cancellationToken = default)
    {
        var users = await _repository.GetPagedAsync(query.PageNumber, query.PageSize);
        var userDtos = users.Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email }).ToList();
        
        return new Response<List<UserDto>>(userDtos, $"Se encontraron {userDtos.Count} usuarios");
    }
}
```

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Response<List<UserDto>>>> GetAll([FromQuery] GetAllUsersQuery query)
        => Ok(await mediator.QueryAsync(query));

    [HttpPost]
    public async Task<ActionResult<Response<UserDto>>> Create([FromBody] CreateUserCommand command)
        => Ok(await mediator.SendAsync(command));
}
```

## ✅ Validación con FluentValidation

### 1. Crear Validator

```csharp
using FluentValidation;
using QuickMediator.Contracts;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>, IAbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Debe ser un email válido");
    }
}
```

### 2. Registrar ValidationBehavior

```csharp
// Program.cs
builder.Services.AddQuickMediator(typeof(Program).Assembly);
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### 3. Respuesta automática con errores

```json
POST /api/users
{
  "name": "",
  "email": "invalid-email"
}

// Respuesta HTTP 200 OK:
{
  "succeeded": false,
  "message": "Error de validación",
  "errors": [
    "El nombre es requerido",
    "Debe ser un email válido"
  ],
  "data": null
}
```

## 🔧 Pipeline Behaviors

### Custom Behavior (Ejemplo: Logging)

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> ProcessAsync(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("🚀 Procesando {RequestName}", requestName);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        
        _logger.LogInformation("✅ {RequestName} completado en {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
        
        return response;
    }
}

// Registrar
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

## 📁 Estructura de Proyecto Recomendada

```
MiApp/
├── Commands/
│   ├── CreateUserCommand.cs        (Command + Handler)
│   └── UpdateUserCommand.cs        (Command + Handler)
├── Queries/
│   ├── GetAllUsersQuery.cs         (Query + Handler)
│   └── GetUserByIdQuery.cs         (Query + Handler)
├── Validators/
│   ├── CreateUserValidator.cs
│   └── UpdateUserValidator.cs
├── Behaviors/
│   └── LoggingBehavior.cs
├── Common/
│   └── Response.cs
├── Models/
│   └── UserDto.cs
└── Controllers/
    └── UsersController.cs
```

## 🎯 Interfaces Principales

| Interface | Propósito |
|-----------|-----------|
| `ICommand<T>` | Comando con respuesta |
| `ICommand` | Comando sin respuesta específica |
| `IQuery<T>` | Consulta que devuelve datos |
| `ICommandHandler<T,R>` | Handler de comando |
| `IQueryHandler<T,R>` | Handler de consulta |
| `IMediator` | Mediador principal |
| `IPipelineBehavior<T,R>` | Comportamiento del pipeline |
| `IAbstractValidator<T>` | Validador de FluentValidation |

## 🚀 Métodos del Mediator

```csharp
// Enviar comandos
await mediator.SendAsync(new CreateUserCommand { ... });
await mediator.SendAsync<UserDto>(new CreateUserCommand { ... });

// Ejecutar consultas  
await mediator.QueryAsync(new GetAllUsersQuery { ... });
await mediator.QueryAsync<List<UserDto>>(new GetAllUsersQuery { ... });
```

## 📋 Ejemplos Completos

### CRUD Básico

Ver el [ejemplo completo en GitHub](https://github.com/tu-usuario/QuickMediator-samples) con:
- ✅ Operaciones CRUD completas
- ✅ Validación automática
- ✅ Manejo de errores
- ✅ Logging behavior

## 🤝 Contribución

¿Encontraste un bug o tienes una sugerencia?

1. Crea un [issue](https://github.com/tu-usuario/QuickMediator/issues)
2. Fork el proyecto

## 📄 Licencia

Este proyecto está bajo la licencia [MIT](LICENSE).

---

**QuickMediator** - Una alternativa ligera y personalizable a MediatR 🚀