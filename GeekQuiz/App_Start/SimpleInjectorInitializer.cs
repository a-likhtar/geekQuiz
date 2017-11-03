using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using GeekQuiz.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SimpleInjector.Advanced;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

[assembly: WebActivator.PostApplicationStartMethod(typeof(GeekQuiz.App_Start.SimpleInjectorInitializer), "Initialize")]

namespace GeekQuiz.App_Start
{
    using System.Web.Http;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using Microsoft.Owin.Security;

    public static class SimpleInjectorInitializer
    {
        /// <summary>Initialize the container and register it as Web API Dependency Resolver.</summary>
        public static void Initialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();
            
            InitializeContainer(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container);
        }
     
        private static void InitializeContainer(Container container)
        {
            //For web api
            container.Register<TriviaContext>(Lifestyle.Scoped);

            //For web mvc
            container.Register<ApplicationUserManager>(Lifestyle.Scoped);
            container.Register<ApplicationSignInManager>(Lifestyle.Scoped);
            container.Register<ApplicationDbContext>(Lifestyle.Scoped);
            container.Register<IdentityFactoryOptions<ApplicationUserManager>>(Lifestyle.Scoped);
            container.Register<IUserStore<ApplicationUser>>(() => new UserStore<ApplicationUser>(container.GetInstance<ApplicationDbContext>()), Lifestyle.Scoped);

            container.Register<IAuthenticationManager>(() =>
                AdvancedExtensions.IsVerifying(container)
                    ? new OwinContext(new Dictionary<string, object>()).Authentication
                    : HttpContext.Current.GetOwinContext().Authentication, Lifestyle.Scoped);
        }
    }
}