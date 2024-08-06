List<Person> people = new List<Person>();
List<Transaction> transactions = new List<Transaction>();

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

            Person? fromPerson, toPerson;

            // Store or extract the person (from)
            if (!IsPersonAvailable(cols[1]))
            {
                fromPerson = new Person(cols[1]);
                people.Add(fromPerson);
            }
            else
            {
                fromPerson = people.Find(p => p.Name == cols[1]);
            }

            // Store or extract the person (to)
            if (!IsPersonAvailable(cols[2]))
            {
                toPerson = new Person(cols[2]);
                people.Add(toPerson);
            }
            else
            {
                toPerson = people.Find(p => p.Name == cols[2]);
            }

            // Create a transaction
            transactions.Add(
                new Transaction()
                {
                    TransactionDate = DateOnly.Parse(cols[0]),
                    FromPersonId = fromPerson?.Id ?? 0,
                    ToPersonId = toPerson?.Id ?? 0,
                    Narrative = cols[3],
                    Amount = decimal.Parse(cols[4])
                }
            );
            
        }
    }

    foreach (Person person in people)
    {
        Console.WriteLine($"ID: {person.Id}, Name: {person.Name}");
    }

    foreach (Transaction transaction in transactions)
    {
        Console.WriteLine($"Transaction Date: {transaction.TransactionDate}, From Person Id: {transaction.FromPersonId}, To Person Id: {transaction.ToPersonId}, Narrative: {transaction.Narrative}, Amount: {transaction.Amount}");
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

void ReadCSVFile(string filepath)
{
    throw new NotImplementedException("Function not ready");

}

void ReadXMLFile(string filepath)
{
    throw new NotImplementedException("Function not ready");

}

void ReadJSONFile(string filepath)
{
    throw new NotImplementedException("Function not ready");
}

// Function to determine the person...
bool IsPersonAvailable(string name)
{
    return people.Exists(p => p.Name == name);
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
}

public class Transaction
{
    public DateOnly TransactionDate { get; set; }
    public int FromPersonId { get; set; }
    public int ToPersonId { get; set; }
    public string Narrative { get; set; } = "";
    public decimal Amount { get; set; }
}

public class AccountEntry
{
    public DateOnly TransactionDate { get; set; }
    public string Narrative { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class Account
{
    public int Person { get; set; }
    public List<AccountEntry> MyProperty { get; set; } = [];
}
