namespace QuickMediator.Contracts;

/// <summary>
/// Representa una consulta que devuelve datos sin modificar el estado del sistema
/// Las consultas son operaciones de solo lectura
/// </summary>
/// <typeparam name="TResponse">Tipo de datos a retornar</typeparam>
public interface IQuery<out TResponse>
{
}