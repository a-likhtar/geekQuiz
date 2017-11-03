using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using GeekQuiz.Models;
using SimpleInjector.Lifestyles;
using SimpleInjector.Integration.WebApi;
using SimpleInjector;

namespace GeekQuiz.App_Start
{
    public class SimpleInjectorWebApiInitializer
    {
        public static void Initialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            InitializeContainer(container);

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container); 
        }

        private static void InitializeContainer(Container container)
        {
            container.Register<TriviaContext>(Lifestyle.Scoped);
            container.Register<TriviaAnswer>(Lifestyle.Scoped);
            container.Register<TriviaQuestion>(Lifestyle.Scoped);
            container.Register<TriviaOption>(Lifestyle.Scoped);
        }
    }
}