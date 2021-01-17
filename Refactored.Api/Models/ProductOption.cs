using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Refactored.Api.Models
{
    public class ProductOption : NewProductOption
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

    public class NewProductOption
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
