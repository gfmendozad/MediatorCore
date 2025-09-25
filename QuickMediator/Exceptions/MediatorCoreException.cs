namespace QuickMediator.Exceptions;

/// <summary>
/// Excepción base para todos los errores relacionados con QuickMediator
/// </summary>
public abstract class MediatorCoreException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia con un mensaje de error
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    protected MediatorCoreException(string message) : base(message) 
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia con un mensaje de error y excepción interna
    /// </summary>
    /// <param name="message">Mensaje que describe el error</param>
    /// <param name="innerException">Excepción que causó el error actual</param>
    protected MediatorCoreException(string message, Exception innerException) : base(message, innerException) 
    {
    }
}