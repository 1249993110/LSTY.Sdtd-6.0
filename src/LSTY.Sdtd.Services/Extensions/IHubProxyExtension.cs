using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LSTY.Sdtd.Services.Extensions
{
    public static class IHubProxyExtension
    {
        private static string GetDelegateName(Delegate @delegate)
        {
            string fullName = @delegate.Method.Name;
            return fullName.Substring(fullName.LastIndexOf('.') + 1);
        }

        public static IDisposable On(this IHubProxy proxy, Action onData)
        {
            return proxy.On(GetDelegateName(onData), onData);
        }

        public static IDisposable On<T>(this IHubProxy proxy, Action<T> onData)
        {
            return proxy.On(GetDelegateName(onData), onData);
        }

        public static IDisposable On<T1, T2>(this IHubProxy proxy, Action<T1, T2> onData)
        {
            return proxy.On(GetDelegateName(onData), onData);
        }

        public static IDisposable On<T1, T2, T3>(this IHubProxy proxy, Action<T1, T2, T3> onData)
        {
            return proxy.On(GetDelegateName(onData), onData);
        }

        public static IDisposable On<T1, T2, T3, T4>(this IHubProxy proxy, Action<T1, T2, T3, T4> onData)
        {
            return proxy.On(GetDelegateName(onData), onData);
        }
    }
}
