namespace Shortener.Core
{
    public record Result<T, TEnum> where TEnum : struct, System.Enum
    {
          public string? Message { get; init; }
          public TEnum Status { get; init; }
          public T? Value { get; init; }
    }
}
