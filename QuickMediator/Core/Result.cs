namespace QuickMediator.Core;

/// <summary>
/// Resultado de operación que encapsula éxito o fallo con datos tipados
/// </summary>
/// <typeparam name="T">Tipo de dato en caso de éxito</typeparam>
public class Result<T>
{
    /// <summary>
    /// Datos en caso de éxito
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Mensaje de error en caso de fallo
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Excepción original en caso de fallo
    /// </summary>
    public Exception? Exception { get; private set; }

    /// <summary>
    /// Indica si la operación falló
    /// </summary>
    public bool IsFailure => !IsSuccess;

    protected Result(T data)
    {
        Data = data;
        IsSuccess = true;
    }

    protected Result(string errorMessage, Exception? exception = null)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
        IsSuccess = false;
    }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    /// <param name="data">Datos del resultado</param>
    /// <returns>Resultado exitoso</returns>
    public static Result<T> Success(T data) => new(data);

    /// <summary>
    /// Crea un resultado de fallo
    /// </summary>
    /// <param name="errorMessage">Mensaje de error</param>
    /// <param name="exception">Excepción opcional</param>
    /// <returns>Resultado de fallo</returns>
    public static Result<T> Failure(string errorMessage, Exception? exception = null) 
        => new(errorMessage, exception);

    /// <summary>
    /// Convierte implícitamente desde T a Result&lt;T&gt;
    /// </summary>
    public static implicit operator Result<T>(T data) => Success(data);

    /// <summary>
    /// Convierte implícitamente desde string a Result&lt;T&gt; (como error)
    /// </summary>
    public static implicit operator Result<T>(string errorMessage) => Failure(errorMessage);

    /// <summary>
    /// Ejecuta una acción si el resultado es exitoso
    /// </summary>
    /// <param name="action">Acción a ejecutar</param>
    /// <returns>El mismo resultado para encadenar</returns>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess && Data is not null)
            action(Data);
        return this;
    }

    /// <summary>
    /// Ejecuta una acción si el resultado es un fallo
    /// </summary>
    /// <param name="action">Acción a ejecutar</param>
    /// <returns>El mismo resultado para encadenar</returns>
    public Result<T> OnFailure(Action<string> action)
    {
        if (IsFailure && ErrorMessage is not null)
            action(ErrorMessage);
        return this;
    }
}

/// <summary>
/// Resultado simple sin datos específicos (solo éxito/fallo)
/// </summary>
public class Result : Result<Unit>
{
    private Result(Unit data) : base(data) { }
    private Result(string errorMessage, Exception? exception = null) : base(errorMessage, exception) { }

    /// <summary>
    /// Crea un resultado exitoso sin datos específicos
    /// </summary>
    /// <returns>Resultado exitoso</returns>
    public static Result Success() => new(Unit.Value);

    /// <summary>
    /// Crea un resultado de fallo sin datos específicos
    /// </summary>
    /// <param name="errorMessage">Mensaje de error</param>
    /// <param name="exception">Excepción opcional</param>
    /// <returns>Resultado de fallo</returns>
    public new static Result Failure(string errorMessage, Exception? exception = null) 
        => new(errorMessage, exception);

    /// <summary>
    /// Convierte implícitamente desde string a Result (como error)
    /// </summary>
    public static implicit operator Result(string errorMessage) => Failure(errorMessage);

    /// <summary>
    /// Convierte implícitamente desde bool a Result
    /// </summary>
    public static implicit operator Result(bool success) => success ? Success() : Failure("Operación falló");
}