using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queue.Communication
{
    class ConnectionPool
    {
        private Dictionary<Type, Queue<IServiceClient>> _freeClients;
        private LinkedList<IServiceClient> _allClients;

        public IEnumerable<T> CreateClients<T>(int count) where T: IServiceClient, new()
        {
            return Enumerable.Range(0, count).Select(i => new T());
        }

        public ClientWrapper<T> GetClient<T>() where T : IServiceClient, new()
        {
            var type = typeof(T);
            if (!_freeClients.ContainsKey(type))
                _freeClients[type] = new Queue<IServiceClient>();
            var serviceClient = _freeClients[type].Dequeue();
            if (serviceClient == null)
            {
                serviceClient = new T();
                _allClients.AddLast(serviceClient);
            }
            return new ClientWrapper<T>(this, (T)serviceClient);
        }

        public void ReleaseClient<T>(T client) where T : IServiceClient
        {
            _freeClients[typeof(T)].Enqueue(client);
        }
    }
}
