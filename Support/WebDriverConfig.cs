using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CorreiosAutomacao.Support
{
    /// <summary>
    /// Configuração singleton do ChromeDriver — otimizado para máxima performance.
    ///
    /// HEADLESS=true  → sem navegador (padrão)
    /// HEADLESS=false → com navegador visível
    ///
    /// macOS/Linux : HEADLESS=false dotnet test
    /// Windows PS  : $env:HEADLESS="false"; dotnet test
    /// </summary>
    public static class WebDriverConfig
    {
        private static IWebDriver? _driver;
        private static readonly object _lock = new();

        // Lê uma única vez e armazena — evita chamada a GetEnvironmentVariable em todo acesso
        public static readonly bool IsHeadless = ResolverHeadless();

        private static bool ResolverHeadless()
        {
            var v = Environment.GetEnvironmentVariable("HEADLESS");
            return string.IsNullOrWhiteSpace(v)
                || !v.Trim().Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        public static IWebDriver GetDriver()
        {
            if (_driver != null) return _driver;
            lock (_lock)
            {
                _driver ??= CriarDriver();
            }
            return _driver;
        }

        private static IWebDriver CriarDriver()
        {
            var options = new ChromeOptions();

            // ── Performance: desabilita tudo que não é necessário para os testes ──
            options.AddArguments(
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-extensions",
                "--disable-infobars",
                "--disable-notifications",
                "--disable-popup-blocking",
                "--disable-translate",
                "--disable-background-networking",
                "--disable-sync",
                "--disable-default-apps",
                "--disable-logging",
                "--disable-hang-monitor",
                "--disable-prompt-on-repost",
                "--metrics-recording-only",
                "--safebrowsing-disable-auto-update",
                "--no-first-run",
                "--blink-settings=imagesEnabled=false",  // sem imagens → páginas carregam mais rápido
                "--window-size=1366,768"
            );

            if (IsHeadless)
            {
                options.AddArgument("--headless=new");
                Console.WriteLine("[DRIVER] Modo: HEADLESS");
            }
            else
            {
                Console.WriteLine("[DRIVER] Modo: VISUAL");
            }

            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);

            // Bloqueia recursos desnecessários via prefs (fontes externas, mídia)
            options.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);

            var driver = new ChromeDriver(options);

            // ImplicitWait ZERO — usamos só WebDriverWait explícito com polling curto.
            // ImplicitWait + WebDriverWait juntos causam esperas dobradas (bug conhecido do Selenium).
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
            driver.Manage().Timeouts().PageLoad     = TimeSpan.FromSeconds(20);

            if (!IsHeadless)
                driver.Manage().Window.Maximize();

            return driver;
        }

        /// <summary>
        /// WebDriverWait reutilizável com polling agressivo de 200ms.
        /// </summary>
        public static WebDriverWait GetWait(int segundos = 8)
            => new(GetDriver(), TimeSpan.FromSeconds(segundos))
            {
                PollingInterval = TimeSpan.FromMilliseconds(200)
            };

        public static void FecharDriver()
        {
            if (_driver == null) return;
            try   { _driver.Quit(); }
            catch { /* ignora erro no quit */ }
            finally
            {
                _driver.Dispose();
                _driver = null;
            }
        }
    }
}
