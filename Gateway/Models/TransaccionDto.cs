using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class TransaccionDto
    {
        public Guid ID { get; set; }
        public List<string> errores;
        
    }
}
