﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioSucursales.Models
{
    public class EmpleadoDto
    {
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string ID { get; set; }
        public string Id_Sucursal { get; set; }
    }
}
