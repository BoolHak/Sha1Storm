using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commun.Entities
{
    public class HashCache
    {
        [Key]
        public DateTime Date { get; set; }
        public long Count { get; set; }

    }
}
