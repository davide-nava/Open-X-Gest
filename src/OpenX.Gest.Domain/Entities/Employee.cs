using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Domain.Entities;

/// <summary>
/// Aggregate Root rappresentante il collaboratore e i suoi parametri contrattuali e legali svizzeri.
/// </summary>
public class Employee : BaseEntity, IAggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public decimal ContractualWeeklyHours { get; private set; }
    public StatutoryWeeklyLimit StatutoryWeeklyLimit { get; private set; }
    public Oll1Regime Oll1Regime { get; private set; }
    public LanguageCode PreferredLanguage { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Costruttore per ORM
    protected Employee() { }

    public Employee(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string department,
        decimal contractualWeeklyHours,
        StatutoryWeeklyLimit statutoryWeeklyLimit,
        Oll1Regime oll1Regime,
        LanguageCode preferredLanguage)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        FirstName = string.IsNullOrWhiteSpace(firstName) ? throw new ArgumentException("Il nome è obbligatorio.", nameof(firstName)) : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? throw new ArgumentException("Il cognome è obbligatorio.", nameof(lastName)) : lastName.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? throw new ArgumentException("L'email è obbligatoria.", nameof(email)) : email.Trim().ToLowerInvariant();
        Department = department?.Trim() ?? string.Empty;
        ContractualWeeklyHours = contractualWeeklyHours <= 0 ? 40.0m : contractualWeeklyHours;
        StatutoryWeeklyLimit = statutoryWeeklyLimit;
        Oll1Regime = oll1Regime;
        PreferredLanguage = preferredLanguage;
        IsActive = true;
    }

    public void UpdateContractualTerms(decimal weeklyHours, StatutoryWeeklyLimit limit)
    {
        if (weeklyHours is <= 0 or > 60)
            throw new ArgumentOutOfRangeException(nameof(weeklyHours), "Le ore contrattuali devono essere comprese tra 1 e 60.");

        ContractualWeeklyHours = weeklyHours;
        StatutoryWeeklyLimit = limit;
    }

    public void UpdateOll1Regime(Oll1Regime regime)
    {
        Oll1Regime = regime;
    }

    public void SetPreferredLanguage(LanguageCode language)
    {
        PreferredLanguage = language;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
