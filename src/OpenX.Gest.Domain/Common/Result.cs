namespace OpenX.Gest.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public IReadOnlyList<Error> Errors { get; }

    protected Result(bool isSuccess, Error error, IReadOnlyList<Error>? errors = null)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("Un risultato positivo non può contenere un errore.");
        }

        if (!isSuccess && error == Error.None && (errors == null || errors.Count == 0))
        {
            throw new InvalidOperationException("Un risultato negativo deve contenere almeno un errore.");
        }

        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? (error != Error.None ? [error] : []);
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(IEnumerable<Error> errors)
    {
        var errorList = errors.ToList();
        return new Result(false, errorList.FirstOrDefault() ?? Error.Failure("General.Error", "Errore sconosciuto"), errorList);
    }
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error, IReadOnlyList<Error>? errors = null)
        : base(isSuccess, error, errors)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? (_value ?? throw new InvalidOperationException("Valore nullo in risultato di successo."))
        : throw new InvalidOperationException("Impossibile accedere al valore di un risultato fallito.");

    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);
    public new static Result<TValue> Failure(Error error) => new(default, false, error);
    public new static Result<TValue> Failure(IEnumerable<Error> errors)
    {
        var errorList = errors.ToList();
        return new Result<TValue>(default, false, errorList.FirstOrDefault() ?? Error.Failure("General.Error", "Errore sconosciuto"), errorList);
    }

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
