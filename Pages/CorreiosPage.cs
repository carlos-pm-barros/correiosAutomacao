using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using CorreiosAutomacao.Support;

namespace CorreiosAutomacao.Pages
{
    /// <summary>
    /// Page Object Model — Correios.
    ///
    /// Otimizações de performance:
    ///  • Sem Thread.Sleep — toda espera é baseada em condição real (polling 200ms)
    ///  • ImplicitWait = 0 — evita espera dupla com WebDriverWait
    ///  • Seletores CSS preferidos ao invés de XPath com translate() no DOM todo
    ///  • PageSource lido uma única vez por verificação e em lowercase direto
    ///  • Navegação direta por URL em vez de voltar para home entre cenários
    ///  • Cookies/popup com timeout curto (2s) — não bloqueiam o fluxo principal
    ///
    /// Seletores obrigatórios (conforme especificação):
    ///   CHECK POR ID    → By.Id(...)
    ///   CHECK POR XPATH → By.XPath(...)
    ///   CHECK POR CSS   → By.CssSelector(...)
    /// </summary>
    public class CorreiosPage
    {
        private readonly IWebDriver _driver;

        private const string URL_BASE        = "https://www.correios.com.br";
        private const string URL_BUSCA_CEP   = "https://www.correios.com.br/busca-cep";
        private const string URL_RASTREIO    = "https://www.correios.com.br/rastreamento-de-objetos";

        // ── SELETORES: BUSCA CEP ─────────────────────────────────────

        /// <summary>CHECK POR ID</summary>
        private static readonly By InputCep     = By.Id("cepEntrada");

        /// <summary>CHECK POR CSS</summary>
        private static readonly By BtnBuscarCep = By.CssSelector("button[aria-label='buscar cep']");

        /// <summary>CHECK POR XPATH — mensagem de CEP não encontrado</summary>
        private static readonly By MsgCepNaoEncontrado = By.XPath(
            "//*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'cep não encontrado')" +
            " or contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'dados não encontrados')" +
            " or contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'não foi encontrado')]");

        /// <summary>CHECK POR CSS — container do resultado de endereço</summary>
        private static readonly By ResultadoCep = By.CssSelector(
            "table.resultados tr td, .resultado-cep, [class*='resultado']");

        /// <summary>CHECK POR XPATH — logradouro no resultado</summary>
        private static readonly By ResultadoLogradouro = By.XPath(
            "//td[contains(.,'Rua') or contains(.,'Avenida') or contains(.,'Praça')]");

        // ── SELETORES: RASTREAMENTO ───────────────────────────────────

        /// <summary>CHECK POR ID</summary>
        private static readonly By InputRastreio = By.Id("objetos");

        /// <summary>CHECK POR CSS</summary>
        private static readonly By BtnRastrear = By.CssSelector(
            "button[type='submit'], .btn-busca, [aria-label*='rastrear']");

        /// <summary>CHECK POR XPATH — código inválido / não encontrado</summary>
        private static readonly By MsgCodigoInvalido = By.XPath(
            "//*[contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'código inválido')" +
            " or contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'objeto não encontrado')" +
            " or contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'não localizado')" +
            " or contains(translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'inválido')]");

        // ── SELETORES: UTILITÁRIOS ────────────────────────────────────

        private static readonly By BtnCookies = By.CssSelector(
            "button[id*='accept'], button[id*='cookie'], button[class*='accept'], " +
            "button[class*='cookie'], #onetrust-accept-btn-handler, .cookie-consent-accept");

        private static readonly By BtnPopup = By.CssSelector(
            ".modal .close, .popup-close, [aria-label='Close'], [aria-label='Fechar'], " +
            "button.close, .modal-close");

        private static readonly By LinkRastreamento = By.CssSelector(
            "a[href*='rastreamento'], a[href*='rastrear']");

        // ── CONSTRUTOR ───────────────────────────────────────────────

        public CorreiosPage()
        {
            _driver = WebDriverConfig.GetDriver();
        }

        // ── HELPERS DE ESPERA ────────────────────────────────────────

        /// <summary>
        /// Aguarda elemento clicável com polling de 200ms.
        /// Não lança exceção — retorna null se não encontrar dentro do timeout.
        /// </summary>
        private IWebElement? Aguardar(By by, int segundos = 8)
        {
            try
            {
                return new WebDriverWait(_driver, TimeSpan.FromSeconds(segundos))
                {
                    PollingInterval = TimeSpan.FromMilliseconds(200)
                }
                .Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(by);
                        return (el.Displayed && el.Enabled) ? el : null;
                    }
                    catch (NoSuchElementException) { return null; }
                    catch (StaleElementReferenceException) { return null; }
                });
            }
            catch { return null; }
        }

        /// <summary>
        /// Aguarda elemento visível (não necessariamente clicável).
        /// </summary>
        private IWebElement? AguardarVisivel(By by, int segundos = 8)
        {
            try
            {
                return new WebDriverWait(_driver, TimeSpan.FromSeconds(segundos))
                {
                    PollingInterval = TimeSpan.FromMilliseconds(200)
                }
                .Until(d =>
                {
                    try
                    {
                        var el = d.FindElement(by);
                        return el.Displayed ? el : null;
                    }
                    catch (NoSuchElementException) { return null; }
                    catch (StaleElementReferenceException) { return null; }
                });
            }
            catch { return null; }
        }

        /// <summary>
        /// Aguarda a página carregar (document.readyState === 'complete').
        /// Substitui Thread.Sleep fixo.
        /// </summary>
        private void AguardarPaginaCarregar(int segundos = 10)
        {
            try
            {
                new WebDriverWait(_driver, TimeSpan.FromSeconds(segundos))
                {
                    PollingInterval = TimeSpan.FromMilliseconds(150)
                }
                .Until(d => ((IJavaScriptExecutor)d)
                    .ExecuteScript("return document.readyState")?.ToString() == "complete");
            }
            catch { /* continua mesmo se JS falhar */ }
        }

        // ── UTILITÁRIOS ───────────────────────────────────────────────

        private void DismissarCookiesEPopups()
        {
            // Timeout curto (2s) — não bloqueia se não houver cookie banner
            Aguardar(BtnCookies, 2)?.Click();
            Aguardar(BtnPopup, 1)?.Click();
        }

        // ── AÇÕES PÚBLICAS ────────────────────────────────────────────

        public void AcessarSite()
        {
            _driver.Navigate().GoToUrl(URL_BASE);
            AguardarPaginaCarregar();
            DismissarCookiesEPopups();
        }

        public void VoltarParaHomeCorreios()
        {
            // Navega diretamente — mais rápido que clicar em logo/link
            _driver.Navigate().GoToUrl(URL_BASE);
            AguardarPaginaCarregar();
            DismissarCookiesEPopups();
        }

        public void BuscarCep(string cep)
        {
            // Navega direto para a página de busca — sem passar pela home
            _driver.Navigate().GoToUrl(URL_BUSCA_CEP);
            AguardarPaginaCarregar();
            DismissarCookiesEPopups();

            // CHECK POR ID — campo de CEP
            var input = Aguardar(InputCep);
            if (input != null)
            {
                input.Clear();
                input.SendKeys(cep);

                // CHECK POR CSS — botão buscar
                var btn = Aguardar(BtnBuscarCep);
                if (btn != null)
                    btn.Click();
                else
                    input.SendKeys(Keys.Enter);
            }
            else
            {
                // Fallback: parâmetro na URL
                _driver.Navigate().GoToUrl($"{URL_BUSCA_CEP}?cep={cep}");
            }
        }

        public bool VerificarCepNaoEncontrado()
        {
            // CHECK POR XPATH — mensagem de não encontrado
            var msgVisivel = AguardarVisivel(MsgCepNaoEncontrado, 6);
            if (msgVisivel != null) return true;

            // Se não apareceu mensagem, verifica se há resultado de endereço
            // Se não há resultado → CEP não encontrado
            var resultado = AguardarVisivel(ResultadoCep, 3);
            return resultado == null;
        }

        public bool VerificarEnderecoCep(string logradouro, string cidade, string uf)
        {
            // CHECK POR CSS — aguarda container de resultado aparecer
            AguardarVisivel(ResultadoCep, 8);

            // Lê pageSource uma única vez (lowercase) para todas as verificações
            var src = _driver.PageSource.ToLowerInvariant();

            if (src.Contains(logradouro.ToLowerInvariant())
             && src.Contains(cidade.ToLowerInvariant())
             && src.Contains(uf.ToLowerInvariant()))
                return true;

            // Fallback CHECK POR XPATH — texto do logradouro na tabela
            var el = AguardarVisivel(ResultadoLogradouro, 3);
            return el?.Text.ToLowerInvariant().Contains(logradouro.ToLowerInvariant()) ?? false;
        }

        public void AcessarRastreamento()
        {
            // Navega direto para URL de rastreamento
            _driver.Navigate().GoToUrl(URL_RASTREIO);
            AguardarPaginaCarregar();
            DismissarCookiesEPopups();

            // Clica no link de rastreamento se ainda não estiver na página correta
            var link = Aguardar(LinkRastreamento, 3);
            link?.Click();
        }

        public void RastrearCodigo(string codigo)
        {
            // CHECK POR ID — campo de rastreamento
            var input = Aguardar(InputRastreio, 8);
            if (input != null)
            {
                input.Clear();
                input.SendKeys(codigo);

                // CHECK POR CSS — botão rastrear
                var btn = Aguardar(BtnRastrear, 5);
                if (btn != null)
                    btn.Click();
                else
                    input.SendKeys(Keys.Enter);
            }
            else
            {
                _driver.Navigate().GoToUrl($"{URL_RASTREIO}?objetos={codigo}");
            }
        }

        public bool VerificarCodigoInvalido()
        {
            // CHECK POR XPATH — mensagem de código inválido
            var el = AguardarVisivel(MsgCodigoInvalido, 8);
            if (el != null) return true;

            // Fallback: pageSource — lido uma vez e reutilizado
            var src = _driver.PageSource.ToLowerInvariant();
            return src.Contains("inválido")
                || src.Contains("invalido")
                || src.Contains("não encontrado")
                || src.Contains("nao encontrado")
                || src.Contains("não localizado");
        }

        public void FecharNavegador() => WebDriverConfig.FecharDriver();
    }
}
