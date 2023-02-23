using Microsoft.AspNetCore.Mvc;
using ServicioSucursales.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioSucursales.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class ServicioSucursalController : ControllerBase
    {
        private static readonly List<SucursalesDto> _sucursales = SucursalesDatabase.CSVDocument();
        private static readonly List<CarDto> _carros = SucursalesDatabase.CarCSVDocument();
        private static readonly List<EmpleadoDto> _empleados = SucursalesDatabase.EmpleadosCSVDocument();

        [HttpGet ("Sucursal/{id}")]

        public ActionResult<SucursalesDto> GetSucursal(string id)
        {

            var sucursal = _sucursales.FirstOrDefault(x => x.ID == id);
            return this.Ok(sucursal);

        }
        [HttpGet("Carro/{idS}/{idC}")]

        public ActionResult<CarDto> GetCarro(string idS, string idC)
        {
            var carro = _carros.FirstOrDefault(x => x.id == idC && x.Id_Sucursal==idS);
            return this.Ok(carro);

        }
        [HttpGet("Empleado/{idS}/{username}")]

        public ActionResult<EmpleadoDto> GetEmpleado(string idS,string username)
        {
            var empleado = _empleados.FirstOrDefault(x => x.username == username && x.Id_Sucursal==idS);
            return this.Ok(empleado);

        }
    }
}
