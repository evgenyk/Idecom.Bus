using System;
using System.Collections.Generic;
using System.Linq;

namespace Idecom.Bus.Implementations
{
    public interface IRoutingTable<TTarget>
    {
        void RouteTypes(IEnumerable<Type> types, TTarget routeTarget);
        TTarget ResolveRouteFor(Type type);
        IEnumerable<TTarget> GetDestinations();
    }

    public class RoutingTable<TTarget> : IRoutingTable<TTarget>
    {
        private readonly Dictionary<Type, TTarget> _routes;

        public RoutingTable()
        {
            _routes = new Dictionary<Type, TTarget>();
        }

        public void RouteTypes(IEnumerable<Type> types, TTarget routeTarget)
        {
            foreach (Type type in types)
            {
                if (_routes.ContainsKey(type))
                    throw new Exception(string.Format("Can not assign type {0} to an endpoint {1} as it has already been mapped to {2}", type.FullName, routeTarget, _routes[type]));
                _routes.Add(type, routeTarget);
            }
        }

        public TTarget ResolveRouteFor(Type type)
        {
            TTarget resolveRouteFor = _routes.ContainsKey(type) ? _routes[type] : default(TTarget);
            return resolveRouteFor;
        }

        public IEnumerable<TTarget> GetDestinations()
        {
            return _routes.Select(x => x.Value).Distinct();
        }

        public bool IsEmpty()
        {
            return !_routes.Any();
        }
    }
}