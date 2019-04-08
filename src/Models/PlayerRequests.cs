using Rocket.API.Player;
using Rocket.API.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleportationPlugin.Models
{
    public class PlayerRequests
    {
        public IUser Receiver { get; set; }
        public IUser Sender { get; set; }
    }
}
