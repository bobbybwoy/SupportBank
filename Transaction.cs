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
