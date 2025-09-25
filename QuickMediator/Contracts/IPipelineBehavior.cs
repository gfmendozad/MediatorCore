using QuickMediator.Pipeline;

namespace QuickMediator.Contracts;

/// <summary>
/// Comportamiento del pipeline para interceptar y procesar comandos/consultas
/// Permite implementar cross-cutting concerns como logging, validación, caching, etc.
/// </summary>
/// <typeparam name="TRequest">Tipo de request (comando o consulta)</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Procesa el request antes de pasarlo al siguiente paso del pipeline
    /// </summary>
    /// <param name="request">Request a procesar (comando o consulta)</param>
    /// <param name="next">Delegado para continuar con el siguiente paso del pipeline</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta procesada</returns>
    Task<TResponse> ProcessAsync(
        TRequest request, 
        RequestDelegate<TResponse> next, 
        CancellationToken cancellationToken);
}