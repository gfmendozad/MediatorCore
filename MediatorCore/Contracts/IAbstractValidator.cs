// Esta es solo la interface que necesitas en MediatorCore
using FluentValidation;

namespace MediatorCore.Contracts;

/// <summary>
/// Interface para validadores en MediatorCore
/// </summary>
/// <typeparam name="T">Tipo a validar</typeparam>
public interface IAbstractValidator<in T> : FluentValidation.IValidator<T>
{
}