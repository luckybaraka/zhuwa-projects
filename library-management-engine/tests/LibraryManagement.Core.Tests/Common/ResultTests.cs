using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Tests.Common;

public class ResultTests
{
    private static readonly DomainError SampleError = new("test.code", "Something went wrong");

    [Fact]
    public void Success_HasValueAndNoErrors()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ExposesErrorByCode()
    {
        var result = Result.Failure<int>(SampleError);

        Assert.True(result.IsFailure);
        Assert.True(result.HasError("test.code"));
        Assert.False(result.HasError("other.code"));
    }

    [Fact]
    public void Value_OnFailure_ThrowsWithTheReason()
    {
        var result = Result.Failure<int>(SampleError);

        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Something went wrong", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Failure_WithEmptyErrorList_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure<int>(Array.Empty<DomainError>()));
    }

    [Fact]
    public void NonGenericResult_SuccessAndFailure()
    {
        Assert.True(Result.Success().IsSuccess);
        Assert.True(Result.Failure(SampleError).IsFailure);
    }
}
