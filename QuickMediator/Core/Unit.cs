namespace QuickMediator.Core;

/// <summary>
/// Tipo vacío para comandos que no devuelven respuesta específica
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Instancia única del tipo Unit
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Compara dos instancias de Unit (siempre son iguales)
    /// </summary>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Compara con otro objeto
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Obtiene el hash code (siempre es 0)
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Operador de igualdad
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Operador de desigualdad
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;

    /// <summary>
    /// Representación en string
    /// </summary>
    public override string ToString() => "()";
}