using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Commun.Entities
{
    [Index(nameof(Date), IsUnique = false, Name = "Index_Date")]
    public class Hash
    {
        [Key]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Sha1 { get; set; }
    }
}
