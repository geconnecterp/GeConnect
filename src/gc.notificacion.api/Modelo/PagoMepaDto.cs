namespace gc.notificacion.api.Modelo
{
    public class Collector
    {
        public int id { get; set; }
        public string email { get; set; }
        public string nickname { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string category_id { get; set; }
        public string currency_id { get; set; }
        public string description { get; set; }
        public object picture_url { get; set; }
        public string title { get; set; }
        public int quantity { get; set; }
        public int unit_price { get; set; }
    }

    public class Payer
    {
        public int id { get; set; }
        public string email { get; set; }
    }

    public class Payment
    {
        public long id { get; set; }
        public int transaction_amount { get; set; }
        public int total_paid_amount { get; set; }
        public int shipping_cost { get; set; }
        public string currency_id { get; set; }
        public string status { get; set; }
        public string status_detail { get; set; }
        public string operation_type { get; set; }
        public DateTime date_approved { get; set; }
        public DateTime date_created { get; set; }
        public DateTime last_modified { get; set; }
        public int amount_refunded { get; set; }
    }

    public class PagoMepaDto
    {
        public long id { get; set; }
        public string status { get; set; }
        public string external_reference { get; set; }
        public string preference_id { get; set; }
        public List<Payment> payments { get; set; }
        public List<object> shipments { get; set; }
        public List<object> payouts { get; set; }
        public Collector collector { get; set; }
        public string marketplace { get; set; }
        public string notification_url { get; set; }
        public DateTime date_created { get; set; }
        public DateTime last_updated { get; set; }
        public object sponsor_id { get; set; }
        public int shipping_cost { get; set; }
        public int total_amount { get; set; }
        public string site_id { get; set; }
        public int paid_amount { get; set; }
        public int refunded_amount { get; set; }
        public Payer payer { get; set; }
        public List<Item> items { get; set; }
        public bool cancelled { get; set; }
        public string additional_info { get; set; }
        public object application_id { get; set; }
        public bool is_test { get; set; }
        public string order_status { get; set; }
        public string client_id { get; set; }
    }


}
