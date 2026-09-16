using System.ComponentModel.DataAnnotations;
namespace FleetPulse.SignalRHub.Validators
{
    public static class ValidationRequestExtension
    {
        /// <summary>
        /// Validates the request object using data annotations and throws a ValidationException if the request is invalid.
        /// Use this later if you want to validate the request object without using FluentValidation.
        /// Minimal API does not validate the request object automatically, so we need to validate it manually.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <exception cref="FleetPulse.Application.Common.Exceptions.ValidationException"></exception>
        public static void ValidateRequest<T>(T request) where T : class
        {

            var annotationValidationResults = new List<ValidationResult>();
            var annotationValidationContext = new ValidationContext(request);

            if (!Validator.TryValidateObject(request, annotationValidationContext, annotationValidationResults, validateAllProperties: true))
            {
                var errors = annotationValidationResults
                    .SelectMany(
                        result => result.MemberNames.DefaultIfEmpty(nameof(T)),
                        (result, memberName) => new
                        {
                            MemberName = memberName,
                            ErrorMessage = result.ErrorMessage ?? "The request is invalid."
                        })
                    .GroupBy(x => x.MemberName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

                throw new Application.Common.Exceptions.ValidationException(errors);
            }

        }
    }
}
