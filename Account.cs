using NLog;

public class Account
{
    // The account holder From person
    public int AccountId { get; set; }

    // TODO: Create a list of debit account entries
    // public List<AccountEntry> Debit { get; set; } = [];

    // TODO: Create a list of credit account entries
    // public List<AccountEntry> Credit { get; set; } = [];

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public override string ToString()
    {
        return $"Account Id: {AccountId} Amount Owed: {Debit:C2} Amount Due: {Credit:C2}";
    }
}
