namespace Infrastructure.Entity;

public partial class CounterFormat
{
    public long Id { get; set; }

    public string CounterCode { get; set; } = null!;

    public int OrderId { get; set; }

    public string Value { get; set; } = null!;

    public byte[] Version { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual Counter CounterCodeNavigation { get; set; } = null!;
}
