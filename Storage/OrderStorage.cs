using System.Collections.Concurrent;
using graceful_shutdown_asp_app.Models;

namespace graceful_shutdown_asp_app.Storage
{
    public class OrderStorage
    {
        public ConcurrentBag<Order> Orders { get; set; }
    }
}