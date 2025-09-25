namespace QuickMediator.Pipeline;

/// <summary>
/// Delegado para la ejecución del siguiente paso en el pipeline
/// </summary>
/// <typeparam name="TResponse">Tipo de respuesta que devuelve el delegado</typeparam>
/// <returns>Tarea que representa la operación asíncrona con el resultado</returns>
public delegate Task<TResponse> RequestDelegate<TResponse>();