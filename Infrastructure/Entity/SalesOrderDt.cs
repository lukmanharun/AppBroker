using System;
using System.Collections.Generic;

namespace Infrastructure.Entity;

public partial class SalesOrderDt
{
    public long Id { get; set; }

    public string SalesOrderCode { get; set; } = null!;

    public string Item { get; set; } = null!;

    public decimal Qty { get; set; }

    public decimal Price { get; set; }

    public decimal Amount { get; set; }

    public byte[] Version { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual SalesOrderHd SalesOrderCodeNavigation { get; set; } = null!;
}
