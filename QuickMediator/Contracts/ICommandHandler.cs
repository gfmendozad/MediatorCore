using QuickMediator.Core;

namespace QuickMediator.Contracts;

/// <summary>
/// Handler para procesar comandos con respuesta específica
/// </summary>
/// <typeparam name="TCommand">Tipo de comando a procesar</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta a devolver</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Ejecuta el comando de forma asíncrona
    /// </summary>
    /// <param name="command">Comando a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta del comando</returns>
    Task<TResponse> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler para procesar comandos sin respuesta específica (void)
/// </summary>
/// <typeparam name="TCommand">Tipo de comando a procesar</typeparam>
public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand<Unit>
{
    /// <summary>
    /// Ejecuta el comando de forma asíncrona sin devolver respuesta específica
    /// </summary>
    /// <param name="command">Comando a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tarea que representa la operación asíncrona</returns>
    new Task ExecuteAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        return ((ICommandHandler<TCommand, Unit>)this).ExecuteAsync(command, cancellationToken);
    }
}