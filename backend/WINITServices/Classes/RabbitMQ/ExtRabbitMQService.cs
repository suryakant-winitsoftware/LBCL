using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WINITServices.Classes.RabbitMQ
{
    public class ExtRabbitMQService : RabbitMQService
    {
        public ExtRabbitMQService(IConfiguration configuration)
        : base(configuration)
        {
            // Additional constructor logic for your custom class
            
        }
        
    }
}
