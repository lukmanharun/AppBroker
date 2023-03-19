using System;
using System.Collections.Generic;

namespace Infrastructure.Entity;

public partial class SalesOrderHd
{
    public string SalesOrderCode { get; set; } = null!;

    public DateTime SalesOrderDate { get; set; }

    public string? StatusCode { get; set; }

    public string UserId { get; set; } = null!;

    public byte[] Version { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual ICollection<SalesOrderDt> SalesOrderDts { get; } = new List<SalesOrderDt>();

    public virtual AspNetUser User { get; set; } = null!;
}
