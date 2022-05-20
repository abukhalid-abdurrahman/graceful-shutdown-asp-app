using System;

namespace graceful_shutdown_asp_app.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string State { get; set; }
    }
}