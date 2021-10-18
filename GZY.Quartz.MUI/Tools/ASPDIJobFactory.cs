using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Text;

namespace GZY.Quartz.MUI.Tools
{
    public class ASPDIJobFactory : IJobFactory
    {
        private static IServiceScopeFactory _serviceProvider;
        public ASPDIJobFactory(IServiceScopeFactory  serviceScopeFactory)
        {

            _serviceProvider = serviceScopeFactory;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
               var sevice =  _serviceProvider.CreateScope();
                return sevice.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
            }
            catch (Exception ex)
            {

                throw;
            }
            //return  
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
