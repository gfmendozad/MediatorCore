using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using QuickMediator.Contracts;
using QuickMediator.Core;

namespace QuickMediator.Extensions;

/// <summary>
/// Extensiones para configurar QuickMediator en el contenedor de dependencias
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra QuickMediator y todos los handlers encontrados en los assemblies especificados
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="assemblies">Assemblies donde buscar handlers. Si no se especifica, usa el assembly que llama</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddMediatorCore(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // Registrar el mediador principal
        services.AddScoped<IMediator, Mediator>();
        
        // Registrar handlers y validators de todos los assemblies
        foreach (var assembly in assemblies)
        {
            RegisterHandlers(services, assembly);
            RegisterValidators(services, assembly);
        }
        
        return services;
    }

    /// <summary>
    /// Registra un behavior específico en el pipeline
    /// </summary>
    /// <typeparam name="TBehavior">Tipo del behavior a registrar</typeparam>
    /// <param name="services">Colección de servicios</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddPipelineBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TBehavior));
        return services;
    }

    /// <summary>
    /// Registra un behavior específico con un tipo concreto
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="behaviorType">Tipo del behavior a registrar</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type behaviorType)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }

    /// <summary>
    /// Registra múltiples behaviors en el pipeline
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="behaviorTypes">Tipos de behaviors a registrar</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddPipelineBehaviors(this IServiceCollection services, params Type[] behaviorTypes)
    {
        foreach (var behaviorType in behaviorTypes)
        {
            services.AddPipelineBehavior(behaviorType);
        }
        return services;
    }

    /// <summary>
    /// Registra handlers manualmente (útil para casos específicos)
    /// </summary>
    /// <typeparam name="THandler">Tipo del handler</typeparam>
    /// <param name="services">Colección de servicios</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddHandler<THandler>(this IServiceCollection services)
        where THandler : class
    {
        var handlerType = typeof(THandler);
        var interfaces = handlerType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

        foreach (var @interface in interfaces)
        {
            services.AddScoped(@interface, handlerType);
        }

        return services;
    }

    /// <summary>
    /// Registra handlers manualmente con lifetime específico
    /// </summary>
    /// <typeparam name="THandler">Tipo del handler</typeparam>
    /// <param name="services">Colección de servicios</param>
    /// <param name="lifetime">Lifetime del servicio (Scoped, Singleton, Transient)</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddHandler<THandler>(this IServiceCollection services, ServiceLifetime lifetime)
        where THandler : class
    {
        var handlerType = typeof(THandler);
        var interfaces = handlerType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                        i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

        foreach (var @interface in interfaces)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(@interface, handlerType);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(@interface, handlerType);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(@interface, handlerType);
                    break;
            }
        }

        return services;
    }

    /// <summary>
    /// Registra un validator manualmente
    /// </summary>
    /// <typeparam name="TValidator">Tipo del validator</typeparam>
    /// <param name="services">Colección de servicios</param>
    /// <returns>Colección de servicios para method chaining</returns>
    public static IServiceCollection AddValidator<TValidator>(this IServiceCollection services)
        where TValidator : class
    {
        var validatorType = typeof(TValidator);
        var interfaces = validatorType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(IAbstractValidator<>));

        foreach (var @interface in interfaces)
        {
            services.AddScoped(@interface, validatorType);
        }

        return services;
    }
    
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        RegisterCommandHandlers(services, assembly);
        RegisterQueryHandlers(services, assembly);
    }

    private static void RegisterCommandHandlers(IServiceCollection services, Assembly assembly)
    {
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                 i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))))
            .ToList();

        foreach (var handlerType in commandHandlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)));
            
            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handlerType);
            }
        }
    }

    private static void RegisterQueryHandlers(IServiceCollection services, Assembly assembly)
    {
        var queryHandlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            .ToList();

        foreach (var handlerType in queryHandlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
            
            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handlerType);
            }
        }
    }

    private static void RegisterValidators(IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IAbstractValidator<>)))
            .ToList();

        foreach (var validatorType in validatorTypes)
        {
            var interfaces = validatorType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(IAbstractValidator<>));
            
            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, validatorType);
            }
        }
    }
}