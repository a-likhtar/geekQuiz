using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using GeekQuiz.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using SimpleInjector.Advanced;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

[assembly: WebActivator.PostApplicationStartMethod(typeof(GeekQuiz.App_Start.SimpleInjectorWebMvcInitializer), "Initialize")]

namespace GeekQuiz.App_Start
{
    using System.Web.Http;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using Microsoft.Owin.Security;

    public static class SimpleInjectorWebMvcInitializer
    {
        /// <summary>Initialize the container and register it as Web API Dependency Resolver.</summary>
        public static void Initialize()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();
            
            InitializeContainer(container);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
       
            container.Verify();
            
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
     
        private static void InitializeContainer(Container container)
        {
            container.Register<ApplicationUserManager>(Lifestyle.Scoped);
            container.Register<ApplicationSignInManager>(Lifestyle.Scoped);
            container.Register<ApplicationDbContext>(Lifestyle.Scoped);
            container.Register<IUserStore<ApplicationUser>>(() => new UserStore<ApplicationUser>(container.GetInstance<ApplicationDbContext>()), Lifestyle.Scoped);

            container.Register<IAuthenticationManager>(() =>
                AdvancedExtensions.IsVerifying(container)
                    ? new OwinContext(new Dictionary<string, object>()).Authentication
                    : HttpContext.Current.GetOwinContext().Authentication, Lifestyle.Scoped);
        }
    }
}