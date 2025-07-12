using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pujas.Infrastructure.interfaces
{
    public class WebSocketMessage
    {
        public string Event { get; set; }
        public object Data { get; set; }
    }
}
