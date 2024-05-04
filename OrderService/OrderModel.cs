namespace OrderService
{
    public class OrderModel
    {
            public string selectedTable { get; set; }
            public string userName { get; set; }
            public List<OrderItem> orderItems { get; set; }

        public class OrderItem
        {
            public string name { get; set; }
            public int full { get; set; }
            public int half { get; set; }
        }
    }
}
