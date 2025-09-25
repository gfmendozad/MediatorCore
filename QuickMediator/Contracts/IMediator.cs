namespace QuickMediator.Contracts;

/// <summary>
/// Mediador principal para ejecutar comandos y consultas siguiendo el patrón CQRS
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Envía un comando con respuesta específica
    /// </summary>
    /// <typeparam name="TResponse">Tipo de respuesta esperada</typeparam>
    /// <param name="command">Comando a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta del comando</returns>
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía un comando sin respuesta específica (void)
    /// </summary>
    /// <param name="command">Comando a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tarea que representa la operación asíncrona</returns>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta una consulta para obtener datos
    /// </summary>
    /// <typeparam name="TResponse">Tipo de datos a obtener</typeparam>
    /// <param name="query">Consulta a ejecutar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos solicitados</returns>
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}