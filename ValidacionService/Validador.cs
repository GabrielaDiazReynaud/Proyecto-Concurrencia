using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace ValidacionService
{
    public class Validador : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;
        private readonly EventingBasicConsumer _consumer;


        public Validador()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("validacion-queue", false, false, false, null);
            _consumer = new EventingBasicConsumer(_channel);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.Received +=async (model, content) =>
            {
                var body = content.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var itemV = JsonConvert.DeserializeObject<ValidationItem>(json);
                await Validar(itemV);
              

            };

            _channel.BasicConsume("validacion-queue", true, _consumer);
            return Task.CompletedTask;
        }



        private async Task Validar(ValidationItem vI)
        {
            var datos = vI.sales;
            var erroresTemp = new ConcurrentBag<string>();
            Regex rx = new Regex(@"^[A-HJ-NPR-Za-hj-npr-z\d]{8}[\dX][A-HJ-NPR-Za-hj-npr-z\d]{2}\d{6}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
            using (var httpClient = new HttpClient())
            {
              foreach(SalesDto venta in datos)
                {

                    var carro = await httpClient.GetStringAsync($"http://localhost:40212/ServicioSucursal/Carro/{venta.Id_Sucursal}/{venta.car_id}");
                    var empleado = await httpClient.GetStringAsync($"http://localhost:40212/ServicioSucursal/Empleado/{venta.Id_Sucursal}/{venta.username}");
                    if (empleado == "")
                    {
                        erroresTemp.Add("Error Empleado no encontrado : {" + venta.username + ",  " + venta.car_id + ", " + venta.buyer_first_name + ", " + venta.price + " }");
                    }
                    if (carro == "")
                    {
                        erroresTemp.Add("Error Carro no encontrado : {" + venta.username + ",  " + venta.car_id + ", " + venta.buyer_first_name + ", " + venta.price + " }");
                    }
                    if (string.IsNullOrEmpty(venta.buyer_last_name))
                    {
                        erroresTemp.Add("Error Apellido del comprador vacio o nulo : {" + venta.username + ",  " + venta.car_id + ", " + venta.buyer_first_name + ", " + venta.price + " }");
                    }
                    if (string.IsNullOrEmpty(venta.buyer_id))
                    {
                        erroresTemp.Add("Error ID del comprador vacio o nulo : {" + venta.username + ",  " + venta.car_id + ", " + venta.buyer_first_name + ", " + venta.price + " }");
                    }
                    MatchCollection matches = rx.Matches(venta.vin);
                    if (matches.Count== 0)
                    {
                        erroresTemp.Add("Error VIN Invalido : {" + venta.username + ",  " + venta.car_id + ", " + venta.buyer_first_name + ", " + venta.price + " }");
                    }

                }

            }
            TransaccionDto trtemp = new TransaccionDto();
            trtemp.ID = vI.transaccionInfo.ID;
            trtemp.errores = erroresTemp.ToList();
            RetornarTransaccion(trtemp);


        }


        private void RetornarTransaccion(TransaccionDto transaccionFinal)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                        var json = JsonConvert.SerializeObject(transaccionFinal);
                        channel.QueueDeclare("resultado-queue", false, false, false, null);
                        var body = Encoding.UTF8.GetBytes(json);
                        channel.BasicPublish(string.Empty, "resultado-queue", null, body);

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
