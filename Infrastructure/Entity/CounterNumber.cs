using System;
using System.Collections.Generic;

namespace Infrastructure.Entity;

public partial class CounterNumber
{
    public long Id { get; set; }

    public string CounterCode { get; set; } = null!;

    public int Year { get; set; }

    public int Month { get; set; }

    public long Number { get; set; }

    public byte[] Version { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual Counter CounterCodeNavigation { get; set; } = null!;
}
