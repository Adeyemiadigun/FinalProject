using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.ExternalServices;
using Hangfire;

namespace Infrastructure.ExternalServices
{
    public class BackgroundJobService : IBackgroundService
    {
        public void Enqueue<T>(Expression<Action<T>> methodCall)
        {
            BackgroundJob.Enqueue<T>(methodCall);
        }
        
        public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
        {
            BackgroundJob.Schedule<T>(methodCall, delay);
        }
    }
}
