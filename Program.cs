using NLog;
using NLog.Config;
using NLog.Targets;

List<Person> people = [];
List<Transaction> transactions = [];
List<Account> accounts = [];
string errorMessage = "";

// Setup logging... (as supplied by TechSwitch)
var config = new LoggingConfiguration();
var target = new FileTarget
{
    FileName = @"../../../logs/SupportBank.log", // To be stored in the project root..
    Layout = @"${longdate} ${level} - ${logger}: ${message}"
};
config.AddTarget("File Logger", target);
config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
LogManager.Configuration = config;

// static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
ILogger logger = LogManager.GetCurrentClassLogger();

logger.Info("Started...");

try
{
    // TODO: Remove functions with side effects...
    ReadCSVFile("data/Transactions2014.csv");

    // Process command line arguments
    switch (args.Length)
    {
        case 0:
            logger.Info("Ended as no arguments were passed to program");
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
    errorMessage = $"File Not Found Exception: {e.Message}";
    logger.Error(errorMessage);
    Console.WriteLine($"File Not Found Exception: {e.Message}");
}
catch (DirectoryNotFoundException e)
{
    errorMessage = $"Directory Not Found Exception: {e.Message}";
    logger.Error(errorMessage);
    Console.WriteLine(errorMessage);
}
catch (NotImplementedException e)
{
    errorMessage = $"Not Implemented Exception: {e.Message}";
    logger.Error(errorMessage);
    Console.WriteLine(errorMessage);
}
catch (IOException e)
{
    errorMessage = $"I/O Exception: {e.Message}";
    logger.Error(errorMessage);
    Console.WriteLine(errorMessage);
}
catch (ArgumentException e)
{
    errorMessage = $"Argument Exception: {e.Message}";
    logger.Error(errorMessage);
    Console.WriteLine(errorMessage);
}
catch (Exception e)
{
    errorMessage = $"Exception: {e.Message}";
    logger.Error(errorMessage);
    Console.WriteLine(errorMessage);
}

logger.Info("Ended...");

/*
    Functions
*/
int GetOrCreatePerson(string name)
{
    logger.Info($"GetOrCreatePerson with name: {name}");
    Person? person = people.Find(p => p.Name == name);
    if (person is null)
    {
        person = new Person(name);
        people.Add(person);
        logger.Info($"Person created with id {person.Id}");
    }

    return person.Id;
}

Account GetOrCreateAccount(int personId)
{
    logger.Info($"GetOrCreateAccount with id: {personId}");
    Account? account = accounts.Find(acct => acct.AccountId == personId);
    if (account is null)
    {
        account = new Account() { AccountId = personId, };
        accounts.Add(account);
        logger.Info($"Account created with id: {account.AccountId}");
    }

    // Although the warning states that it may be null, this is not true...
    return account;
}

void ReadCSVFile(string filePath)
{
    logger.Info($"ReadCSVFile with file: {filePath}");
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
            logger.Info($"Transaction created: {transaction.TransactionDate}, from: {transaction.FromPersonId}, to: {transaction.ToPersonId}, {transaction.Narrative}, {transaction.Amount}");

            // Update the accounts
            Account fromAccount = GetOrCreateAccount(fromPersonId);
            fromAccount.Debit += transaction.Amount;
            logger.Info($"Updated fromAccount amount owed: {fromAccount.Debit}");
            Account toAccount = GetOrCreateAccount(toPersonId);
            toAccount.Credit += transaction.Amount;
            logger.Info($"Updated fromAccount amount due: {fromAccount.Credit}");
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
