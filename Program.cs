using System.Transactions;
using System.Xml;

using Newtonsoft.Json;

using NLog;
using NLog.Config;
using NLog.Targets;

List<Person> people = [];
List<Transaction> transactions = [];
List<Account> accounts = [];
List<TxnJSONFormat>? jsonTransactions = [];

// This variable is used to for logging and exceptions
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
    // transactions = ReadCSVFile("data/Transactions2014.csv");
    // transactions = ReadCSVFile("data/DodgyTransactions2015.csv");
    // transactions = ReadJSONFile("data/Transactions2013.json");
    transactions = ReadXMLFile("data/Transactions2012.xml");

    // Process command line arguments
    switch (args.Length)
    {
        case 0:
            logger.Info("Ended as no arguments were passed to program");
            return;
        case 2:
            if (args[0].ToLower() == "list")
                switch (args[1].ToLower())
                {
                    case "all":
                        ListAll();
                        break;
                    default:
                        ListAccount(args[1]);
                        break;
                }
            break;
        case 4:
            throw new NotImplementedException("We have not implemented the four argument process yet.");
        default:
            throw new ArgumentException("Too many or too few arguments!\nList All or List <personname> commands allowed.");
    }
}
catch (InvalidDataException e)
{
    errorMessage = $"Invalid Data Exception: {e.Message}";
    LogAndDisplayError(errorMessage);
}
catch (FileNotFoundException e)
{
    errorMessage = $"File Not Found Exception: {e.Message}";
    LogAndDisplayError(errorMessage);
}
catch (DirectoryNotFoundException e)
{
    errorMessage = $"Directory Not Found Exception: {e.Message}";
    LogAndDisplayError(errorMessage);
}
catch (NotImplementedException e)
{
    errorMessage = $"Not Implemented Exception: {e.Message}";
    LogAndDisplayError(errorMessage);
}
catch (IOException e)
{
    errorMessage = $"I/O Exception: {e.Message}";
    LogAndDisplayError(errorMessage);
}
catch (ArgumentException e)
{
    errorMessage = $"Argument Exception: {e.Message} {e.StackTrace}";
    LogAndDisplayError(errorMessage);
}
catch (Exception e)
{
    errorMessage = $"Exception: {e.StackTrace}";
    LogAndDisplayError(errorMessage);
}

logger.Info("Ended...");

/*
    Functions
*/
void LogAndDisplayError(string errorMessage)
{
    logger.Error(errorMessage);
    Console.WriteLine(errorMessage);
}

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

void CreateTransactionAndUpdateAccount
(
    DateOnly date,
    int fromPrsnId,
    int toPrsnId,
    string desc,
    decimal amt,
    List<Transaction> txns)
{
    // Create a transaction
    Transaction transaction = new Transaction()
    {
        TransactionDate = date,
        FromPersonId = fromPrsnId,
        ToPersonId = toPrsnId,
        Narrative = desc,
        Amount = amt
    };
    txns.Add(transaction);
    logger.Info($"Transaction created: {transaction.TransactionDate}, from: {transaction.FromPersonId}, to: {transaction.ToPersonId}, {transaction.Narrative}, {transaction.Amount}");

    // Update the accounts
    Account fromAccount = GetOrCreateAccount(fromPrsnId);
    fromAccount.Debit += transaction.Amount;
    logger.Info($"Updated fromAccount amount owed: {fromAccount.Debit}");
    Account toAccount = GetOrCreateAccount(toPrsnId);
    toAccount.Credit += transaction.Amount;
    logger.Info($"Updated fromAccount amount due: {fromAccount.Credit}");
}

List<Transaction> ReadCSVFile(string filePath)
{
    logger.Info($"ReadCSVFile with file: {filePath}");
    List<Transaction> txns = [];

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

            DateOnly date = DateOnly.Parse(cols[0]);
            decimal amt = decimal.Parse(cols[4]);
            
            CreateTransactionAndUpdateAccount(date, fromPersonId, toPersonId, cols[3], amt, txns);
        }
    }

    return txns;
}

List<Transaction> ReadXMLFile(string filePath)
{
    List<Transaction> txns = [];
    string description = "";
    decimal value = 0;
    string from = "";
    string to = "";
    string? date = "";
    int numberOfDaysSince1900 = 0;
    DateOnly txnDate = new();
    const string supportTransaction = "SupportTransaction";
    XmlReader reader = XmlReader.Create(filePath);

    while (reader.Read())
    {
        switch (reader.IsStartElement())
        {
            case true:
                switch (reader.Name.ToString())
                {
                    case supportTransaction:
                        date = reader.GetAttribute("Date");
                        numberOfDaysSince1900 = int.Parse(date);
                        // txnDate = DateOnly.FromDayNumber(numberOfDaysSince1900);
                        txnDate = DateOnly.FromDateTime(new DateTime(1900, 01, 01).AddDays(numberOfDaysSince1900));
                        break;
                    case "Description":
                        description = reader.ReadElementContentAsString();
                        break;
                    case "Value":
                        value = reader.ReadElementContentAsDecimal();
                        break;
                    case "From":
                        from = reader.ReadElementContentAsString();
                        break;
                    case "To":
                        to = reader.ReadElementContentAsString();
                        break;
                }
                break;
            default:
                if (reader.Name.ToString() == supportTransaction)
                {
                    // Console.WriteLine($"{txnDate.ToShortDateString()}, {description}, {value:C2}, {from}, {to}");
                    // Create the 'from' person
                    int fromPrsnId = GetOrCreatePerson(from);
                    
                    // Create the 'to' person
                    int toPrsnId = GetOrCreatePerson(to);

                    CreateTransactionAndUpdateAccount(txnDate, fromPrsnId, toPrsnId, description, value, txns);
                }
                break;
        }
    }

    return txns;
}

List<Transaction> ReadJSONFile(string filePath)
{
    logger.Info("ReadJsonFile with file: {filePath}");
    List<Transaction> txns = [];

    using(StreamReader reader = new(filePath))
    {
        string rawJsonData = reader.ReadToEnd();

        jsonTransactions = JsonConvert.DeserializeObject<List<TxnJSONFormat>>(rawJsonData) ?? [];

        foreach (TxnJSONFormat jsonTransaction in jsonTransactions)
        {
            logger.Info($"jsonTransaction: {jsonTransaction}");
            // Create the from and to person objects
            int fromPersonId = GetOrCreatePerson(jsonTransaction.FromAccount);
            logger.Info($"Got/Created a from person with id: {fromPersonId}");
            int toPersonId = GetOrCreatePerson(jsonTransaction.ToAccount);
            logger.Info($"Got/Created a to person with id: {fromPersonId}");

            DateOnly date = DateOnly.Parse(jsonTransaction.Date);

            CreateTransactionAndUpdateAccount(date, fromPersonId, toPersonId, jsonTransaction.Narrative, jsonTransaction.Amount, txns);

            // Console.WriteLine(jsonTransaction);
            // Transaction transaction = new Transaction
            // {
            //     TransactionDate = DateOnly.Parse(jsonTransaction.Date),
            //     FromPersonId = fromPersonId,
            //     ToPersonId = toPersonId,
            //     Narrative = jsonTransaction.Narrative,
            //     Amount = jsonTransaction.Amount
            // };
            // transactions.Add(transaction);
            // logger.Info($"Created a transaction...");
        }
    }

    return txns;
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
    Console.WriteLine($"\nAmount Owed: {account?.Debit:C2} Amount Due: {account?.Credit:C2}");
}

public class TxnJSONFormat
{
    public string Date { get; set; } = "";
    public string FromAccount { get; set; } = "";
    public string ToAccount { get; set; } = "";
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
    public override string ToString()
    {
        return $"Transaction Date: {Date} FromAccount: {FromAccount} ToAccount: {ToAccount} Narrative: {Narrative} Amount: {Amount}";
    }
}
