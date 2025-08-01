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
        public static SimpleDependencyContainer Container { get; private set; }
        
        public static void Register(HttpConfiguration config)
        {
            // Simple dependency resolver for .NET Framework 4.8
            Container = new SimpleDependencyContainer();
            
            // Register dependencies
            RegisterDependencies(Container);
            
            // Set the dependency resolver
            config.DependencyResolver = Container;
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
            
            container.RegisterTransient<IReportService>(resolver => 
                new ReportService((DatabaseConnection)resolver.GetService(typeof(DatabaseConnection))));
            
            // Register controllers
            container.RegisterTransient<QuestionsController>(resolver => 
                new QuestionsController((IQuestionService)resolver.GetService(typeof(IQuestionService))));
            
            container.RegisterTransient<ReportsController>(resolver => 
                new ReportsController((IReportService)resolver.GetService(typeof(IReportService))));
        }
    }
}