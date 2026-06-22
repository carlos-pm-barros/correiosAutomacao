using Reqnroll;
using Allure.Net.Commons;
using CorreiosAutomacao.Support;
using OpenQA.Selenium;

namespace CorreiosAutomacao.Hooks
{
    [Binding]
    public class TestHooks
    {
        private static DateTime _inicioSuite;
        private DateTime _inicioCenario;

        /// <summary>
        /// Inicializa o driver UMA VEZ antes de qualquer teste.
        /// O singleton é aquecido aqui para eliminar latência no primeiro cenário.
        /// </summary>
        [BeforeTestRun]
        public static void AntesDeRodarTodos()
        {
            _inicioSuite = DateTime.Now;
            Console.WriteLine($"[SUITE] Iniciando — {_inicioSuite:HH:mm:ss}");
            Console.WriteLine($"[DRIVER] HEADLESS={WebDriverConfig.IsHeadless}");
            // Aquece o driver antecipadamente
            _ = WebDriverConfig.GetDriver();
        }

        [BeforeScenario]
        public void AntesDeEach(ScenarioContext ctx)
        {
            _inicioCenario = DateTime.Now;
            Console.WriteLine($"\n[►] {ctx.ScenarioInfo.Title}");
        }

        [AfterScenario]
        public void DepoisDeEach(ScenarioContext ctx)
        {
            var duracao = (DateTime.Now - _inicioCenario).TotalSeconds;
            var status  = ctx.ScenarioExecutionStatus;

            Console.WriteLine($"[{(ctx.TestError == null ? "✓" : "✗")}] " +
                              $"{ctx.ScenarioInfo.Title} | {status} | {duracao:F1}s");

            if (ctx.TestError != null)
            {
                Console.WriteLine($"    ERRO: {ctx.TestError.Message}");
                TirarScreenshot(ctx.ScenarioInfo.Title);
            }
        }

        [AfterTestRun]
        public static void DepoisDeRodarTodos()
        {
            var total = (DateTime.Now - _inicioSuite).TotalSeconds;
            Console.WriteLine($"\n[SUITE] Concluída em {total:F1}s");
            GravarEnvironmentProperties();
            WebDriverConfig.FecharDriver();
        }

        private static void GravarEnvironmentProperties()
        {
            try
            {
                var pastaAllure = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "allure-results");
                Directory.CreateDirectory(pastaAllure);

                var arquivo = Path.Combine(pastaAllure, "environment.properties");
                var conteudo = string.Join(Environment.NewLine, new[]
                {
                    $"Projeto=CorreiosAutomacao",
                    $"Framework=.NET {Environment.Version}",
                    $"BDD=Reqnroll",
                    $"TestRunner=NUnit",
                    $"Browser=Google Chrome",
                    $"Headless={WebDriverConfig.IsHeadless}",
                    $"SistemaOperacional={Environment.OSVersion}",
                    $"MachineName={Environment.MachineName}"
                });

                File.WriteAllText(arquivo, conteudo);
                Console.WriteLine($"[ALLURE] environment.properties gerado em: {arquivo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ALLURE] Não foi possível gerar environment.properties: {ex.Message}");
            }
        }

        private static void TirarScreenshot(string nomeScenario)
        {
            try
            {
                if (WebDriverConfig.GetDriver() is ITakesScreenshot ss)
                {
                    var pasta   = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                    Directory.CreateDirectory(pasta);
                    var arquivo = Path.Combine(pasta,
                        $"{nomeScenario.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                    ss.GetScreenshot().SaveAsFile(arquivo);
                    Console.WriteLine($"    SCREENSHOT: {arquivo}");

                    try
                    {
                        AllureApi.AddAttachment("Screenshot da falha", "image/png", arquivo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    ALLURE ATTACHMENT ERRO: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    SCREENSHOT ERRO: {ex.Message}");
            }
        }
    }
}
