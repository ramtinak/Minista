using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationHandler.Toast
{
    public interface IToastActivation
    {
        string Type { get; }
        string GenerateToastUri();
    }
}
