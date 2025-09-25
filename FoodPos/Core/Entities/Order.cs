using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;

public class Order : BaseEntity
{
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string PaymentIntentId { get; set; }
    // id del customer en Stripe
    public string StripeCustomerId { get; set; }
    // email hacia donde se envía el recibo. Si es que no es el mismo que el del usuario
    public string ReceiptEmail { get; set; }

    //orderItems
    public ICollection<OrderItem> OrderItems { get; set; }

    // relación con user
    public int UserId { get; set; }
    public User User { get; set; }
}
