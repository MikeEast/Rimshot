using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace Rimshot.Hubs
{
    public class DeviceHub : Hub
    {
        static readonly Dictionary<string, IList<ConnectedClient>> userList = new Dictionary<string, IList<ConnectedClient>>();

        public override Task OnConnected()
        {
            if(!userList.ContainsKey(Context.User.Identity.Name))
                userList.Add(Context.User.Identity.Name, new List<ConnectedClient>());
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            if (Context.User == null)
                return base.OnDisconnected();

            if (userList.ContainsKey(Context.User.Identity.Name))
            {
                var connectedClients = userList[Context.User.Identity.Name];
                var connectedClient = connectedClients.SingleOrDefault(c => c.ConnectionId == Context.ConnectionId);
                if (connectedClient != null) { 
                    Groups.Remove(Context.ConnectionId, Context.User.Identity.Name);
                    connectedClients.Remove(connectedClient);
                }
            }
                
            return base.OnDisconnected();
        }

        public void Register(object spec)
        {
            if (Context.User == null)
                return;

            if(!userList.ContainsKey(Context.User.Identity.Name))
                userList.Add(Context.User.Identity.Name, new List<ConnectedClient>());

            var connectedClients = userList[Context.User.Identity.Name];
            if (connectedClients.All(c => c.ConnectionId != Context.ConnectionId)) { 
                connectedClients.Add(new ConnectedClient
                {
                    ConnectionId = Context.ConnectionId,
                    Spec = spec
                });
                Groups.Add(Context.ConnectionId, Context.User.Identity.Name);
                Clients.OthersInGroup(Context.User.Identity.Name).deviceJoined(spec);
            }
        }

        public void OpenFileDialog(object name)
        {
            Clients.OthersInGroup(Context.User.Identity.Name).openFileDialog();
        }

        public IEnumerable<ConnectedClient> GetDevices()
        {
            if (userList.ContainsKey(Context.User.Identity.Name))
                return userList[Context.User.Identity.Name].Where(c => c.ConnectionId != Context.ConnectionId);
            return Enumerable.Empty<ConnectedClient>();
        }
    }

    public class ConnectedClient
    {
        public string ConnectionId { get; set; }
        public object Spec { get; set; }
    }
}