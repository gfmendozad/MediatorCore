using QuickMediator.Core;

namespace QuickMediator.Contracts;

/// <summary>
/// Representa un comando que produce una respuesta específica
/// Los comandos son operaciones que modifican el estado del sistema
/// </summary>
/// <typeparam name="TResponse">Tipo de respuesta que devuelve el comando</typeparam>
public interface ICommand<out TResponse>
{
}

/// <summary>
/// Representa un comando que no devuelve respuesta específica (void)
/// Los comandos son operaciones que modifican el estado del sistema
/// </summary>
public interface ICommand : ICommand<Unit>
{
}