List<Person> people = [];
List<Transaction> transactions = [];
List<Account> accounts = [];

// Process command line arguments

try
{
    // I don't like functions with side effects...
    ReadCSVFile("data/Transactions2014.csv");

    switch (args.Length)
    {
        case 0:
            return;
        case 2:
            if (args[0].ToLower() == "list")
                switch (args[1])
                {
                    case "All":
                        // Console.WriteLine("Call ListAll()");
                        ListAll();
                        break;
                    default:
                        // Console.WriteLine("Call ListAccount()");
                        ListAccount(args[1]);
                        break;
                }
            break;
        case 4:
            throw new NotImplementedException("We have not implemented the four argument process yet.");
        // break;
        default:
            throw new ArgumentException("Too many or too few arguments!\nList All or List <personname> commands allowed.");
    }

    // foreach (Person person in people)
    // {
    //     Console.WriteLine(person);
    // }
}
catch (FileNotFoundException e)
{
    Console.WriteLine($"File Not Found Exception: {e.Message}");
}
catch (DirectoryNotFoundException e)
{
    Console.WriteLine($"Directory Not Found Exception: {e.Message}");
}
catch (NotImplementedException e)
{
    Console.WriteLine($"Not Implemented Exception: {e.Message}");
}
catch (IOException e)
{
    Console.WriteLine($"I/O Exception: {e.Message}");
}
catch (ArgumentException e)
{
    Console.WriteLine($"Argument Exception: {e.Message}");
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e.Message}");
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

void ReadCSVFile(string filePath)
{
    // using (StreamReader reader = new("data/Transactions2014.csv"))
    using (StreamReader reader = new(filePath))
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
}

void ReadXMLFile(string filePath)
{
    throw new NotImplementedException("Function not ready");

}

void ReadJSONFile(string filePath)
{
    throw new NotImplementedException("Function not ready");
}

void ListAll()
{
    foreach (Account account in accounts)
    {
        Person? person = people.Find(p => p.Id == account.AccountId) ?? null;
        Console.WriteLine($"{person?.Name} {account}");
    }
}

void ListAccount(string accountName)
{
    // Get the person Id
    Person? person = people.Find(p => p.Name == accountName);
    if (person is null)
        throw new ArgumentException($"Account {accountName} cannot be found");

    // Create a filtered list of transactions
    List<Transaction> filteredTransactions =
        transactions.FindAll(t => t.FromPersonId == person.Id || t.ToPersonId == person.Id);

    // Output the transaction list
    foreach (Transaction transaction in filteredTransactions)
    {
        Person? fromPerson = people.Find(p => p.Id == transaction.FromPersonId);
        Person? toPerson = people.Find(p => p.Id == transaction.ToPersonId);

        // Console.WriteLine(transaction);
        Console.WriteLine($"Transaction Date: {transaction.TransactionDate}, From Person Id: {fromPerson?.Name}, To Person Id: {toPerson?.Name}, Narrative: {transaction.Narrative}, Amount: {transaction.Amount:C2}");
    }

    Account? account = accounts.Find(a => a.AccountId == person.Id);
    Console.WriteLine($"\nAmount Owed: {account.Debit:C2} Amount Due: {account.Credit:C2}");
}
