using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecolectorService
{
    public class Recolector : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private readonly EventingBasicConsumer _consumer;


        public Recolector()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("recolector-queue", false, false, false, null);
            _consumer = new EventingBasicConsumer(_channel);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.Received += async (model, content) =>
            {
                var body = content.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var transaccion = JsonConvert.DeserializeObject<TransaccionDto>(json);

                //  var Result = await ObtenerClima(fecha, cancellationToken);
                MandarMensajeValidacion(transaccion);
            };

            _channel.BasicConsume("recolector-queue", true, _consumer);
            return Task.CompletedTask;
        }

        private List<int> divisionCargaArchivo(int size)
        {
            List<int> division = new List<int>();
            if (size <= 50)
            {
                division.Add(size);
            }
            else
            {
                int cantidad = size / 50;
                int total = 0;
                for(int x=0; x<cantidad; x++)
                {
                        division.Add(50);
                        total += 50;
                }

                if (total != size)
                {
                    division.Add((size - total));
                }

            }

            return division;

        }

        private void MandarMensajeValidacion(TransaccionDto transaccion)
        {
            List<SalesDto> listSales = new List<SalesDto>();

            var path = @"C:/Users/gabyd/source/repos/ProyectoConcurrencia/RecolectorService/sales.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {

                    string[] fields = csvParser.ReadFields();


                    SalesDto saleTemp = new SalesDto();
                    saleTemp.username = fields[0];
                    saleTemp.car_id = fields[1];
                    saleTemp.price = fields[2];
                    saleTemp.vin = fields[3];
                    saleTemp.buyer_first_name = fields[4];
                    saleTemp.buyer_last_name = fields[5];
                    saleTemp.buyer_id = fields[6];
                    saleTemp.Id_Sucursal = fields[7];

                    listSales.Add(saleTemp);
                }
            }

            List<int> carga = divisionCargaArchivo(listSales.Count());
            

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };
           
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                   
                    Parallel.For(0, carga.Count(), (i) =>
                    {
                        int inicio = 0;

                        for(int x=0; x < i; x++)
                        {
                            inicio += carga[x];

                        }
                       
                        var temp = listSales.GetRange(inicio, carga[i]);
                        ValidationItem vI = new ValidationItem();
                        vI.sales = temp;
                        vI.transaccionInfo = transaccion;
                        var json = JsonConvert.SerializeObject(vI);
                        channel.QueueDeclare("validacion-queue", false, false, false, null);
                        var body = Encoding.UTF8.GetBytes(json);
                        channel.BasicPublish(string.Empty, "validacion-queue", null, body);

                    }
                    );
                     
                   
                }
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
