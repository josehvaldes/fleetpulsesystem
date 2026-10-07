using FluentValidation;
using Mediator;
using ValidationException = FleetPulse.Application.Common.Exceptions.ValidationException;

namespace FleetPulse.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
        : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
    {
        public async ValueTask<TResponse> Handle(TMessage request, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return await next(request, cancellationToken);
            }

            var context = new ValidationContext<TMessage>(request);

            var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errors = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

                throw new ValidationException(errors);
            }

            return await next(request, cancellationToken);
        }
    }
}
