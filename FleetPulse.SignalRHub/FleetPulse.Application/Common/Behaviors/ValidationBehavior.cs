using FluentValidation;
using Mediator;

namespace FleetPulse.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TMessage, TResponse>
        : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
    {
        private readonly IEnumerable<IValidator<TMessage>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
            => _validators = validators;

        public async ValueTask<TResponse> Handle(TMessage request, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators == null || !_validators.Any())
            {
                return await next(request, cancellationToken);
            }

            var context = new ValidationContext<TMessage>(request);

            var validationTasks = _validators.Select(v => v.ValidateAsync(context, cancellationToken));
            var results = await Task.WhenAll(validationTasks);

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                var errors = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

                throw new Exceptions.ValidationException(errors);
            }

            return await next(request, cancellationToken);
        }
    }
}
