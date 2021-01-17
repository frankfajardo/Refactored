using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Refactored.Api.Models
{
    public class Product : NewProduct
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public bool IsNew 
        { 
            get
            {
                return Id == Guid.Empty;
            } 
        }
    }

    public class NewProduct
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public decimal DeliveryPrice { get; set; }
    }
}
