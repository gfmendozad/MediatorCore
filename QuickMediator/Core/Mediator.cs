using Microsoft.Extensions.DependencyInjection;
using QuickMediator.Contracts;
using QuickMediator.Exceptions;
using QuickMediator.Pipeline;

namespace QuickMediator.Core;

/// <summary>
/// Implementación principal del mediador para el patrón CQRS
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Inicializa una nueva instancia del mediador
    /// </summary>
    /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias</param>
    /// <exception cref="ArgumentNullException">Cuando serviceProvider es null</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));
        
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new HandlerNotFoundException($"No se encontró un handler para el comando: {commandType.Name}", commandType.Name);
        }

        return await ExecuteWithPipeline(command, handlerType, handler, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        await SendAsync<Unit>(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));
        
        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
        {
            throw new HandlerNotFoundException($"No se encontró un handler para la consulta: {queryType.Name}", queryType.Name);
        }

        return await ExecuteQueryWithPipeline(query, handlerType, handler, cancellationToken);
    }

    private async Task<TResponse> ExecuteWithPipeline<TResponse>(
        ICommand<TResponse> command, 
        Type handlerType, 
        object handler, 
        CancellationToken cancellationToken)
    {
        var commandType = command.GetType(); // Usar el tipo concreto del command
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(commandType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(behaviorType).ToList();

        RequestDelegate<TResponse> handlerDelegate = async () =>
        {
            var method = handlerType.GetMethod("ExecuteAsync") 
                ?? throw new InvalidOperationException($"Método ExecuteAsync no encontrado en {handlerType.Name}");
            
            var result = method.Invoke(handler, new object[] { command, cancellationToken });
            return await (Task<TResponse>)result!;
        };

        // Construir pipeline: cada behavior envuelve al siguiente
        behaviors.Reverse();
        foreach (var behavior in behaviors)
        {
            var currentBehavior = behavior;
            var nextDelegate = handlerDelegate;
            handlerDelegate = () =>
            {
                var processMethod = currentBehavior!.GetType().GetMethod("ProcessAsync");
                var result = processMethod!.Invoke(currentBehavior, new object[] { command, nextDelegate, cancellationToken });
                return (Task<TResponse>)result!;
            };
        }

        return await handlerDelegate();
    }

    private async Task<TResponse> ExecuteQueryWithPipeline<TResponse>(
        IQuery<TResponse> query, 
        Type handlerType, 
        object handler, 
        CancellationToken cancellationToken)
    {
        var queryType = query.GetType(); // Usar el tipo concreto de la query
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(queryType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(behaviorType).ToList();

        RequestDelegate<TResponse> handlerDelegate = async () =>
        {
            var method = handlerType.GetMethod("HandleAsync")
                ?? throw new InvalidOperationException($"Método HandleAsync no encontrado en {handlerType.Name}");
            
            var result = method.Invoke(handler, new object[] { query, cancellationToken });
            return await (Task<TResponse>)result!;
        };

        // Construir pipeline: cada behavior envuelve al siguiente
        behaviors.Reverse();
        foreach (var behavior in behaviors)
        {
            var currentBehavior = behavior;
            var nextDelegate = handlerDelegate;
            handlerDelegate = () =>
            {
                var processMethod = currentBehavior!.GetType().GetMethod("ProcessAsync");
                var result = processMethod!.Invoke(currentBehavior, new object[] { query, nextDelegate, cancellationToken });
                return (Task<TResponse>)result!;
            };
        }

        return await handlerDelegate();
    }
}