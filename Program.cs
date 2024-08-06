List<Person> people = [];
List<Transaction> transactions = [];
List<Account> accounts = [];

try
{
    using (StreamReader reader = new("data/Transactions2014.csv"))
    {
        // Ignore the header record
        reader.ReadLine();

        // Read the transactions
        while (reader.Peek() >= 0)
        {
            string line = reader.ReadLine() ?? "";
            string[] cols = line.Split(",");

            // Store or extract the person (from)
            int fromPersonId = GetOrCreatePerson(cols[1]);

            // Store or extract the person (to)
            int toPersonId = GetOrCreatePerson(cols[2]);

            // Create a transaction
            Transaction transaction = new Transaction()
            {
                TransactionDate = DateOnly.Parse(cols[0]),
                FromPersonId = fromPersonId,
                ToPersonId = toPersonId,
                Narrative = cols[3],
                Amount = decimal.Parse(cols[4])
            };
            transactions.Add(transaction);

            // Update the accounts
            Account FromAccount = GetOrCreateAccount(fromPersonId);
            FromAccount.Debit += transaction.Amount;
            Account ToAccount = GetOrCreateAccount(toPersonId);
            ToAccount.Credit += transaction.Amount;
        }
    }

    // foreach (Person person in people)
    // {
    //     Console.WriteLine(person);
    // }

    foreach (Transaction transaction in transactions)
    {
        Console.WriteLine(transaction);
    }

    foreach (Account account in accounts)
    {
        Person? person = people.Find(p => p.Id == account.AccountId) ?? null;
        Console.WriteLine($"{person?.Name} {account}");
    }
}
catch (NotImplementedException e)
{
    Console.WriteLine(e.Message);
}
catch (IOException e)
{
    Console.WriteLine("Cannot read the file");
    Console.WriteLine(e.Message);
}

/*
    Functions
*/
int GetOrCreatePerson(string name)
{
    Person? person = people.Find(p => p.Name == name);
    if (person is null) {
        person = new Person(name);
        people.Add(person);
    }

    return person.Id;
}

Account GetOrCreateAccount(int personId)
{
    Account? account  = accounts.Find(acct => acct.AccountId == personId);
    if (account is null)
    {
        account = new Account(){ AccountId = personId, };
        accounts.Add(account);
    }

    // Although the warning states that it may be null, this is not true...
    return account;
}

public class Person
{
    private static int _id = 0;
    public Person(string name)
    {
        Id = ++_id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; set; } = "";

    public override string ToString()
    {
        return $"Id: {Id} Name: {Name}";
    }
}

public class Transaction
{
    public DateOnly TransactionDate { get; set; }
    public int FromPersonId { get; set; }
    public int ToPersonId { get; set; }
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }

    public override string ToString()
    {
        return $"Transaction Date: {TransactionDate}, From Person Id: {FromPersonId}, To Person Id: {ToPersonId}, Narrative: {Narrative}, Amount: {Amount:C2}";
    }
}

// TODO: Create an account entry class

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
