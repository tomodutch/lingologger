using FluentValidation;
using LingoLogger.Discord.Bot.InteractionParameters;
using LingoLogger.Web.Models;

namespace LingoLogger.Discord.Bot.Validators;

public class LogReadParametersValidator : AbstractValidator<LogReadParameters>
{
    public LogReadParametersValidator()
    {
        var timeParser = new TimeParser();
        RuleFor(x => x.Medium).NotEmpty().WithMessage("Medium is required");
        RuleFor(x => x.Time).NotEmpty().WithMessage("Time is required");
        RuleFor(x => x.Time).Must((time) =>
        {
            try
            {
                var seconds = timeParser.ParseTimeToSeconds(time);
                return seconds > 0;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }).WithMessage("Time must be written like the following examples and be greater than 0: (i.e \"2h25m\" \"1h\" \"50m\")");
        RuleFor(x => x.Characters)
            .GreaterThan(0)
            .When(x => x.Characters.HasValue)
            .WithMessage("Characters must be greater than 0 if specified");
        RuleFor(x => x.Date).Must(createdAtString =>
        {
            var parsed = timeParser.ParseDate(createdAtString!);
            return parsed != null;
        }).When(x => x.Date != null).WithMessage("Not a valid date");
    }
}