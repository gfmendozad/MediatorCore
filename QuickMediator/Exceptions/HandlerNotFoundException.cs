namespace QuickMediator.Exceptions;

/// <summary>
/// Excepción lanzada cuando no se encuentra un handler para un comando o consulta específica
/// </summary>
public class HandlerNotFoundException : MediatorCoreException
{
    /// <summary>
    /// Nombre del tipo de request para el cual no se encontró handler
    /// </summary>
    public string? RequestTypeName { get; }

    /// <summary>
    /// Tipo completo del request para el cual no se encontró handler
    /// </summary>
    public Type? RequestType { get; }

    /// <summary>
    /// Inicializa una nueva instancia con un mensaje de error
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    public HandlerNotFoundException(string message) : base(message) 
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia con un mensaje de error y el nombre del tipo de request
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    /// <param name="requestTypeName">Nombre del tipo de request que causó el error</param>
    public HandlerNotFoundException(string message, string requestTypeName) : base(message) 
    {
        RequestTypeName = requestTypeName;
    }

    /// <summary>
    /// Inicializa una nueva instancia con un mensaje de error y el tipo de request
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    /// <param name="requestType">Tipo de request que causó el error</param>
    public HandlerNotFoundException(string message, Type requestType) : base(message) 
    {
        RequestType = requestType;
        RequestTypeName = requestType.Name;
    }

    /// <summary>
    /// Inicializa una nueva instancia con un mensaje de error y excepción interna
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    /// <param name="innerException">Excepción que causó el error actual</param>
    public HandlerNotFoundException(string message, Exception innerException) : base(message, innerException) 
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia completa
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    /// <param name="requestTypeName">Nombre del tipo de request que causó el error</param>
    /// <param name="innerException">Excepción que causó el error actual</param>
    public HandlerNotFoundException(string message, string requestTypeName, Exception innerException) : base(message, innerException) 
    {
        RequestTypeName = requestTypeName;
    }

    /// <summary>
    /// Inicializa una nueva instancia completa con tipo
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    /// <param name="requestType">Tipo de request que causó el error</param>
    /// <param name="innerException">Excepción que causó el error actual</param>
    public HandlerNotFoundException(string message, Type requestType, Exception innerException) : base(message, innerException) 
    {
        RequestType = requestType;
        RequestTypeName = requestType.Name;
    }
}