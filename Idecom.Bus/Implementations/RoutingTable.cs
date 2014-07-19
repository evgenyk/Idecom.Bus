namespace Idecom.Bus.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IRoutingTable<TTarget> where TTarget : class
    {
        void RouteTypes(IEnumerable<Type> types, TTarget routeTarget);
        TTarget ResolveRouteFor(Type type);
        IEnumerable<TTarget> GetDestinations();
        void RouteType(Type type, TTarget routeTarget);
    }

    public class RoutingTable<TTarget> : IRoutingTable<TTarget> where TTarget : class
    {
        readonly Dictionary<Type, TTarget> _routes;

        public RoutingTable()
        {
            _routes = new Dictionary<Type, TTarget>();
        }

        public void RouteTypes(IEnumerable<Type> types, TTarget routeTarget)
        {
            foreach (var type in types)
            {
                if (_routes.ContainsKey(type))
                    throw new Exception(string.Format("Can not assign type {0} to an endpoint {1} as it has already been mapped to {2}", type.FullName, routeTarget, _routes[type]));
                _routes.Add(type, routeTarget);
            }
        }

        public void RouteType(Type type, TTarget routeTarget)
        {
            if (_routes.ContainsKey(type))
                throw new Exception(string.Format("Can not assign type {0} to an endpoint {1} as it has already been mapped to {2}", type.FullName, routeTarget, _routes[type]));
            _routes.Add(type, routeTarget);
        }

        public TTarget ResolveRouteFor(Type type)
        {
            var route = _routes.ContainsKey(type) ? _routes[type] : default(TTarget);
            return route;
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