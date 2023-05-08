using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Workplace
{
    public class GetCommandHandler<T> : IRequestHandler<GetCommand<T>, Unit>
    {
        public Task<Unit> Handle(GetCommand<T> request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Handler invocation for type " + typeof(T));
            return Unit.Task;
        }
    }
}