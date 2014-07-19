using System;
using System.Collections.Generic;
using System.Linq;

namespace Idecom.Bus.Implementations
{
    public interface IMultiRoutingTable<TTarget> where TTarget : class
    {
        void RouteTypes(IEnumerable<Type> types, TTarget routeTarget);
        IEnumerable<TTarget> ResolveRouteFor(Type type);
        IEnumerable<TTarget> GetDestinations();
        void RouteType(Type type, TTarget routeTarget);
    }

    public class PluralRoutingTable<TTarget> : IMultiRoutingTable<TTarget> where TTarget : class
    {
        private readonly Dictionary<Type, List<TTarget>> _routes;

        public PluralRoutingTable()
        {
            _routes = new Dictionary<Type, List<TTarget>>();
        }

        public void RouteTypes(IEnumerable<Type> types, TTarget routeTarget)
        {
            foreach (Type type in types)
            {
                if (_routes.ContainsKey(type))
                    _routes[type].Add(routeTarget);
                else
                    _routes.Add(type, new List<TTarget> {routeTarget});
            }
        }

        public void RouteType(Type type, TTarget routeTarget)
        {
            if (_routes.ContainsKey(type))
                _routes[type].Add(routeTarget);
            else
                _routes.Add(type, new List<TTarget> {routeTarget});
        }

        public IEnumerable<TTarget> ResolveRouteFor(Type type)
        {
            var route = _routes.ContainsKey(type) ? _routes[type] : new List<TTarget>();
            return route;
        }

        public IEnumerable<TTarget> GetDestinations()
        {
            return _routes.SelectMany(x => x.Value).Distinct();
        }

        public bool IsEmpty()
        {
            return !_routes.Any();
        }
    }
}