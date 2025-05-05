using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.EntidadesComunes
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Collector
    {
        public int id { get; set; }
        public string email { get; set; } = string.Empty;
        public string nickname { get; set; } = string.Empty;
    }

    public class Item
    {
        public string id { get; set; } = string.Empty;
        public string category_id { get; set; } = string.Empty;
        public string currency_id { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public object picture_url { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public int quantity { get; set; } =0;
        public int unit_price { get; set; } = 0;
    }

    public class Payer
    {
        public int id { get; set; } = 0;
        public string email { get; set; } = string.Empty;
    }

    public class Payment
    {
        public long id { get; set; }
        public int transaction_amount { get; set; }
        public int total_paid_amount { get; set; }
        public int shipping_cost { get; set; }
        public string currency_id { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string status_detail { get; set; } = string.Empty;
        public string operation_type { get; set; } = string.Empty;
        public DateTime date_approved { get; set; }
        public DateTime date_created { get; set; }
        public DateTime last_modified { get; set; }
        public int amount_refunded { get; set; }
    }

    public class OrdenDetalleResponse
    {
        public long id { get; set; }
        public string status { get; set; } = string.Empty;
        public string external_reference { get; set; } = string.Empty;
        public string preference_id { get; set; } = string.Empty;
        public List<Payment> payments { get; set; } = new List<Payment>();  
        public List<object> shipments { get; set; } = new List<object>();
        public List<object> payouts { get; set; } = new List<object>();
        public Collector collector { get; set; } = new Collector();
        public string marketplace { get; set; } = string.Empty;
        public string notification_url { get; set; } = string.Empty;
        public DateTime date_created { get; set; }
        public DateTime last_updated { get; set; }
        public object sponsor_id { get; set; } = new object();
        public int shipping_cost { get; set; }
        public int total_amount { get; set; }
        public string site_id { get; set; } = string.Empty;
        public int paid_amount { get; set; }
        public int refunded_amount { get; set; }
        public Payer payer { get; set; } = new Payer();
        public List<Item> items { get; set; } = new List<Item>();
        public bool cancelled { get; set; }
        public string additional_info { get; set; } = string.Empty;
        public object application_id { get; set; } = new object();
        public bool is_test { get; set; } 
        public string order_status { get; set; } = string.Empty;
        public string client_id { get; set; } = string.Empty;   
    }    
}
