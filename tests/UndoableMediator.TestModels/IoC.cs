using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UndoableMediator.Commands;

namespace UndoableMediator.TestModels
{
    public static class IoC
    {
        public static void Register(this IServiceCollection serviceCollection)
        {
            //serviceCollection.AddSingleton<ICommandHandler<SetRandomAgeCommand, int>, SetRandomAgeCommandHandler>();
        }
    }
}
