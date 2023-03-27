using System;
using System.Collections.Generic;

namespace Infrastructure.Entity;

public partial class Counter
{
    public string CounterCode { get; set; } = null!;

    public string Format { get; set; } = null!;

    public byte[] Version { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual ICollection<CounterFormat> CounterFormats { get; } = new List<CounterFormat>();

    public virtual ICollection<CounterNumber> CounterNumbers { get; } = new List<CounterNumber>();
}
