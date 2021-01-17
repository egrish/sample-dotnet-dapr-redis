using System;
using System.ComponentModel.DataAnnotations;

namespace SampleStateStore_Redis.Models
{
    public class Client
    {
        [Key]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
