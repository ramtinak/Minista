using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationHandler.Toast
{
    public class PostActivation : IToastActivation
    {
        public string Type => "Post";
        public string GenerateToastUri()
        {
            throw new NotImplementedException();
        }
    }
}
