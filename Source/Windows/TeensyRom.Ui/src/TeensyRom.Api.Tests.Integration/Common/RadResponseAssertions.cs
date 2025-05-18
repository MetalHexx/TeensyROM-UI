using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace TeensyRom.Api.Tests.Integration.Common
{
    public static class RadTestResultAssertions
    {
        public static RadTestResultAssertion<TResponse> BeSuccessful<TResponse>(this ObjectAssertions assertions)
        {
            var testResult = assertions.Subject as RadTestResult<TResponse>;

            Execute.Assertion
                .ForCondition(testResult != null)
                .FailWith("Expected object to be of type RadTestResult<{0}>, but found {1}.", typeof(TResponse), assertions.Subject.GetType());

            Execute.Assertion
                .ForCondition(testResult!.Http.IsSuccessStatusCode)
                .FailWith("Expected HTTP response to be successful (2xx), but found {0}.", testResult.Http.StatusCode);

            return new RadTestResultAssertion<TResponse>(testResult);
        }

        public static RadTestResultProblemAssertion BeProblem(this ObjectAssertions assertions)
        {
            var testResult = assertions.Subject as RadTestResult<ProblemDetails>;

            Execute.Assertion
                .ForCondition(testResult?.Content != null)
                .FailWith("Expected object to be of type ProblemDetails, but found {1}.", typeof(ProblemDetails), assertions.Subject.GetType());

            Execute.Assertion
                .ForCondition(!testResult!.Http.IsSuccessStatusCode)
                .FailWith("Expected HTTP response to be a problem (4xx), but found {0}.", testResult.Http.StatusCode);

            return new RadTestResultProblemAssertion(testResult);
        }

        public static RadTestResultValidationProblemAssertion BeValidationProblem(this ObjectAssertions assertions)
        {
            var testResult = assertions.Subject as RadTestResult<ValidationProblemDetails>;

            Execute.Assertion
                .ForCondition(testResult?.Content != null)
                .FailWith("Expected object to be of type ValidationProblemDetails, but found {1}.", typeof(ValidationProblemDetails), assertions.Subject.GetType());

            Execute.Assertion
                .ForCondition(!testResult!.Http.IsSuccessStatusCode)
                .FailWith("Expected HTTP response to be a problem (4xx), but found {0}.", testResult.Http.StatusCode);

            return new RadTestResultValidationProblemAssertion(testResult);
        }
    }

    public class RadTestResultAssertion<TResponse>(RadTestResult<TResponse> result)
    {
        public RadTestResultAssertion<TResponse> WithStatusCode(HttpStatusCode statusCode)
        {
            result.Http.StatusCode.Should().Be(statusCode, $"the status code should be {statusCode}");
            return this;
        }

        public RadTestResultAssertion<TResponse> WithContentNotNull()
        {
            result.Content.Should().NotBeNull($"the content should have data");
            return this;
        }
    }

    public class RadTestResultProblemAssertion(RadTestResult<ProblemDetails> result)
    {
        public RadTestResultProblemAssertion WithMessage(string expectedTitle)
        {
            result.Content.Title.Should().Be(expectedTitle, "the message in the response content should match");
            return this;
        }

        public RadTestResultProblemAssertion WithStatusCode(HttpStatusCode statusCode)
        {
            result.Http.StatusCode.Should().Be(statusCode, $"the status code should be {statusCode}");
            result.Content.Status.Should().Be((int)statusCode, $"the status code should be {statusCode}");
            return this;
        }

        public RadTestResultProblemAssertion WithKey(string expectedKey)
        {
            result.Content.Extensions.Should().ContainKey(expectedKey);
            return this;
        }

        public RadTestResultProblemAssertion WithKeyAndValue(string expectedKey, string expectedValue)
        {
            result.Content.Extensions.Should().ContainKey(expectedKey)
                .WhoseValue!.ToString().Should().Be(expectedValue);
            return this;
        }

        public RadTestResultProblemAssertion WithDetail(string expectedDetail)
        {
            result.Content.Detail.Should().Be(expectedDetail);
            return this;
        }

        public RadTestResultProblemAssertion WithInstance(string expectedInstance)
        {
            result.Content.Instance.Should().Be(expectedInstance);
            return this;
        }

        public RadTestResultProblemAssertion WithContentType(string expectedType)
        {
            result.Content.Type.Should().Be(expectedType);
            return this;
        }
    }

    public class RadTestResultValidationProblemAssertion(RadTestResult<ValidationProblemDetails> result)
    {
        public RadTestResultValidationProblemAssertion WithTitle(string expectedTitle)
        {
            result.Content.Title.Should().Be(expectedTitle, "the message in the response content should match");
            return this;
        }

        public RadTestResultValidationProblemAssertion WithStatusCode(HttpStatusCode statusCode)
        {
            result.Http.StatusCode.Should().Be(statusCode, $"the status code should be {statusCode}");
            result.Content.Status.Should().Be((int)statusCode, $"the status code should be {statusCode}");
            return this;
        }

        public RadTestResultValidationProblemAssertion WithKey(string expectedKey)
        {
            result.Content.Errors.Should().ContainKey(expectedKey);
            return this;
        }

        public RadTestResultValidationProblemAssertion WithKeyAndValue(string expectedKey, string expectedValue)
        {
            result.Content.Errors.Should().ContainKey(expectedKey)
                .WhoseValue.Should().Contain(expectedValue);
            return this;
        }

        public RadTestResultValidationProblemAssertion WithDetail(string expectedDetail)
        {
            result.Content.Detail.Should().Be(expectedDetail);
            return this;
        }

        public RadTestResultValidationProblemAssertion WithInstance(string expectedInstance)
        {
            result.Content.Instance.Should().Be(expectedInstance);
            return this;
        }

        public RadTestResultValidationProblemAssertion WithContentType(string expectedType)
        {
            result.Content.Type.Should().Be(expectedType);
            return this;
        }
    }
}