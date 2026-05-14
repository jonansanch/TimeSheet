using FluentAssertions;
using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using Xunit;

namespace KPG.Timesheet.Application.UnitTests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void DefaultConstructorCreatesAnEmptyErrorDictionary()
    {
        var actual = new ValidationException().Errors;

        actual.Keys.Should().BeEmpty();
    }

    [Fact]
    public void SingleValidationFailureCreatesASingleElementErrorDictionary()
    {
        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Age", "must be over 18"),
        };

        var actual = new ValidationException(failures).Errors;

        actual.Keys.Should().BeEquivalentTo(new[] { "Age" });
        actual["Age"].Should().BeEquivalentTo(new[] { "must be over 18" });
    }

    [Fact]
    public void MultipleValidationFailuresForMultiplePropertiesCreatesMultipleElementErrorDictionary()
    {
        const string ageField = "Age";
        const string passwordField = "Password";

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure(ageField, "must be 18 or older"),
            new ValidationFailure(ageField, "must be 25 or younger"),
            new ValidationFailure(passwordField, "must contain at least 8 characters"),
            new ValidationFailure(passwordField, "must contain a digit"),
            new ValidationFailure(passwordField, "must contain upper case letter"),
            new ValidationFailure(passwordField, "must contain lower case letter"),
        };

        var actual = new ValidationException(failures).Errors;

        actual.Keys.Should().BeEquivalentTo(new[] { ageField, passwordField });
        actual[ageField].Should().BeEquivalentTo(new[] { "must be 18 or older", "must be 25 or younger" });
        actual[passwordField].Should().BeEquivalentTo(new[]
        {
            "must contain at least 8 characters",
            "must contain a digit",
            "must contain upper case letter",
            "must contain lower case letter",
        });
    }
}
