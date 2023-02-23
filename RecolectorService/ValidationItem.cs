using System;
using System.Collections.Generic;
using System.Text;

namespace RecolectorService
{
    class ValidationItem
    {
        public TransaccionDto transaccionInfo { get; set; }
        public List<SalesDto> sales { get; set; }
    }
}
