namespace QuickMediator.Contracts;

/// <summary>
/// Handler para procesar consultas que devuelven datos
/// </summary>
/// <typeparam name="TQuery">Tipo de consulta a procesar</typeparam>
/// <typeparam name="TResponse">Tipo de datos a devolver</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Procesa la consulta de forma asíncrona
    /// </summary>
    /// <param name="query">Consulta a procesar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos solicitados</returns>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}