using FluentValidation;
using Mediator;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FleetPulse.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators == null || !_validators.Any())
            {
                return await next(request, cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

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
