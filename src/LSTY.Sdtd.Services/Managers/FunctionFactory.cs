using LSTY.Sdtd.Services.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LSTY.Sdtd.Services.Managers
{
    public class FunctionFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FunctionFactory> _logger;
        private readonly Dictionary<Type, FunctionBase> _functionDict;

        public FunctionFactory(IServiceProvider serviceProvider, ILogger<FunctionFactory> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
            _functionDict = new Dictionary<Type, FunctionBase>();
        }

        public IReadOnlyCollection<FunctionBase> GetAll()
        {
            return _functionDict.Values;
        }

        public FunctionBase GetFunction(Type type)
        {
            return _functionDict[type];
        }

        public T GetFunction<T>() where T : FunctionBase
        {
            return _functionDict[typeof(T)] as T;
        }

        internal void Initialize()
        {
            try
            {
                foreach (var type in typeof(FunctionFactory).Assembly.GetExportedTypes())
                {
                    if (type.IsSubclassOf(typeof(FunctionBase)) && type.IsAbstract == false)
                    {
                        try
                        {
                            var ins = Activator.CreateInstance(type, GetParameters(type.GetConstructors()[0])) as FunctionBase;
                            _functionDict[type] = ins;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Function: {type.Name} initialization failed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FunctionFactory initialization failed");
            }
        }

        private object[] GetParameters(MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                args[i] = _serviceProvider.GetRequiredService(parameters[i].ParameterType);
            }

            return args;
        } 
    }
}
