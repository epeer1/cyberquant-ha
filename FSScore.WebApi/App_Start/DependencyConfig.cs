using System.Web.Http;
using FSScore.WebApi.DataAccess;
using FSScore.WebApi.Services;
using FSScore.WebApi.Controllers;

namespace FSScore.WebApi
{
    /// <summary>
    /// Dependency injection configuration for the Web API
    /// </summary>
    public static class DependencyConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Simple dependency resolver for .NET Framework 4.8
            var container = new SimpleDependencyContainer();
            
            // Register dependencies
            RegisterDependencies(container);
            
            // Set the dependency resolver
            config.DependencyResolver = container;
        }

        private static void RegisterDependencies(SimpleDependencyContainer container)
        {
            // Register database connection as singleton
            container.RegisterSingleton<DatabaseConnection>(() => new DatabaseConnection());
            
            // Register repositories
            container.RegisterTransient<IQuestionRepository>(resolver => 
                new QuestionRepository((DatabaseConnection)resolver.GetService(typeof(DatabaseConnection))));
            
            // Register services
            container.RegisterTransient<IQuestionService>(resolver => 
                new QuestionService((IQuestionRepository)resolver.GetService(typeof(IQuestionRepository))));
            
            // Register controllers
            container.RegisterTransient<QuestionsController>(resolver => 
                new QuestionsController((IQuestionService)resolver.GetService(typeof(IQuestionService))));
        }
    }
}