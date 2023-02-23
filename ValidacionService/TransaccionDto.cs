using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ValidacionService
{
    public class TransaccionDto
    {
        public Guid ID { get; set; }
        public List<string> errores;
        
    }
}
