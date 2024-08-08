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
