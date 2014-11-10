using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    class ConnectionPool
    {
        private Dictionary<Type, Queue<IServiceClient>> _freeClients = new Dictionary<Type,Queue<IServiceClient>>();
        private LinkedList<IServiceClient> _allClients = new LinkedList<IServiceClient>();

        public IEnumerable<T> CreateClients<T>(int count) where T: IServiceClient, new()
        {
            return Enumerable.Range(0, count).Select(i => new T());
        }

        public ClientWrapper<T> GetClient<T>() where T : IServiceClient, new()
        {
            var type = typeof(T);
            IServiceClient serviceClient;
            lock (_freeClients)
            {
                if (!_freeClients.ContainsKey(type))
                    _freeClients[type] = new Queue<IServiceClient>();
                serviceClient = _freeClients[type].Count > 0 ? _freeClients[type].Dequeue() : null;
                if (serviceClient == null)
                {
                    serviceClient = new T();
                    _allClients.AddLast(serviceClient);
                }
            }
            return new ClientWrapper<T>(this, (T)serviceClient);
        }

        public void ReleaseClient<T>(T client) where T : IServiceClient
        {
            lock (_freeClients)
            {
                _freeClients[typeof(T)].Enqueue(client);
            }

        }
    }
}
