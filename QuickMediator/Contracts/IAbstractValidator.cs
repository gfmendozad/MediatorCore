// Esta es solo la interface que necesitas en QuickMediator
using FluentValidation;

namespace QuickMediator.Contracts;

/// <summary>
/// Interface para validadores en QuickMediator
/// </summary>
/// <typeparam name="T">Tipo a validar</typeparam>
public interface IAbstractValidator<in T> : FluentValidation.IValidator<T>
{
}