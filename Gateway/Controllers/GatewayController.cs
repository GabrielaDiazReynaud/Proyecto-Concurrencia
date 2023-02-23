using Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]

    public class GatewayController: ControllerBase
    {
        private static readonly List<TransaccionDto> _transacciones = new List<TransaccionDto>();
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<string>> get(Guid id)
        {
            var t = _transacciones.FirstOrDefault(x => x.ID == id);
            var r = t.errores;
            return this.Ok(r);
        }

        [HttpGet("Inicializar")]

        public ActionResult<TransaccionDto> InicializarProceso()
        {
            TransaccionDto transaccion = new TransaccionDto();
            transaccion.ID = Guid.NewGuid();
            transaccion.errores = new List<string>();
            _transacciones.Add(transaccion);

            EnviarMensajeRecolector(transaccion);
            RecibirTransaccionFinal();
            return this.Ok(transaccion);

        }



        public void EnviarMensajeRecolector(TransaccionDto transaccion)
        {
            var json = JsonConvert.SerializeObject(transaccion);
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("recolector-queue", false, false, false, null);
                    var body = Encoding.UTF8.GetBytes(json);
                    channel.BasicPublish(string.Empty, "recolector-queue", null, body);
                }
            }
        }

        public void RecibirTransaccionFinal()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };
           var _connection = factory.CreateConnection();
           var _channel = _connection.CreateModel();
            _channel.QueueDeclare("resultado-queue", false, false, false, null);
           var _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, content) =>
            {
                var body = content.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var transaccion = JsonConvert.DeserializeObject<TransaccionDto>(json);
               foreach(TransaccionDto tr in _transacciones)
                {
                    if (tr.ID == transaccion.ID)
                    {
                        tr.errores.AddRange(transaccion.errores);
                    }
                }
              
            };

            _channel.BasicConsume("resultado-queue", true, _consumer);
        }
    }
}
