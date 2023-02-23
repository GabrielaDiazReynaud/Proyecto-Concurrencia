using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.FileIO;
using ServicioSucursales.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServicioSucursales
{
    public class SucursalesDatabase
    {


        private static readonly HttpClient httpClient = new HttpClient();
        public static List<SucursalesDto> CSVDocument()
        {
            List<SucursalesDto> listSucursales = new List<SucursalesDto>();
            var path = @"C:/Users/gabyd/source/repos/ProyectoConcurrencia/ServicioSucursales/sucursales.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {

                    string[] values = csvParser.ReadFields();


                    SucursalesDto sucursalTemp = new SucursalesDto();
                    sucursalTemp.ID =values[0];
                    sucursalTemp.Country = values[1];
                    sucursalTemp.State = values[2];
                    listSucursales.Add(sucursalTemp);
           
                }
            }
            return listSucursales;
        }

        public static List<CarDto> CarCSVDocument()
        {
            List<CarDto> listCarros = new List<CarDto>();
            var path = @"C:/Users/gabyd/source/repos/ProyectoConcurrencia/ServicioSucursales/cars.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {

                    string[] values = csvParser.ReadFields();
                    CarDto carTemp = new CarDto();
                    carTemp.id = values[0];
                    carTemp.make = values[1];
                    carTemp.model = values[2];
                    carTemp.year = int.Parse(values[3]);
                    carTemp.Id_Sucursal = values[4];
                    listCarros.Add(carTemp);

                }
                
            }
            return listCarros;
        }

        public static List<EmpleadoDto> EmpleadosCSVDocument()
        {
            List<EmpleadoDto> listEmpleados = new List<EmpleadoDto>();
            var path = @"C:/Users/gabyd/source/repos/ProyectoConcurrencia/ServicioSucursales/employees.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    string[] values = csvParser.ReadFields();
                    EmpleadoDto empleadoTemp = new EmpleadoDto();
                    empleadoTemp.username = values[0];
                    empleadoTemp.first_name = values[1];
                    empleadoTemp.last_name = values[2];
                    empleadoTemp.ID = values[3];
                    empleadoTemp.Id_Sucursal = values[4];

                    listEmpleados.Add(empleadoTemp);

                }

            }
            return listEmpleados;
        }
    }
}
