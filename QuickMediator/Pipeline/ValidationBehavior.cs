using FluentValidation;
using QuickMediator.Pipeline;
using QuickMediator.Contracts;

namespace QuickMediator.Pipeline;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IAbstractValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IAbstractValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> ProcessAsync(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (!failures.Any()) return await next();
            {
                // Buscar un constructor que reciba (string message)
                var responseType = typeof(TResponse);
                var errorMessages = failures.Select(f => f.ErrorMessage).ToList();
                
                // Intentar crear una respuesta de error si tiene el constructor adecuado
                var messageConstructor = responseType.GetConstructor(new[] { typeof(string) });
                if (messageConstructor != null)
                {
                    var response = Activator.CreateInstance(responseType, "Error de validación");
                    
                    // Buscar propiedades comunes para establecer errores
                    var succeededProp = responseType.GetProperty("Succeeded");
                    var errorsProp = responseType.GetProperty("Errors");
                    
                    succeededProp?.SetValue(response, false);
                    errorsProp?.SetValue(response, errorMessages);
                    
                    return ((TResponse)response!)!;
                }
                
                // Si no puede crear respuesta de error, lanzar excepción
                throw new ValidationException(failures);
            }
        }

        return await next();
    }
}