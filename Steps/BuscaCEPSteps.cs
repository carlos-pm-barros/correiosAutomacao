using NUnit.Framework;
using Reqnroll;
using CorreiosAutomacao.Pages;

namespace CorreiosAutomacao.Steps
{
    [Binding]
    public class BuscaCEPSteps
    {
        private readonly CorreiosPage _correiosPage = new();

        [Given(@"que acesso o site dos Correios")]
        public void DadoQueAcessoOSiteDosCorreios()
        {
            _correiosPage.AcessarSite();
            Console.WriteLine("[INFO] Site dos Correios acessado.");
        }

        [When(@"busco pelo CEP ""(.*)""")]
        public void QuandoBuscoPeloCep(string cep)
        {
            Console.WriteLine($"[INFO] Buscando CEP: {cep}");
            _correiosPage.BuscarCep(cep);
        }

        [When(@"rastreio o código ""(.*)""")]
        public void QuandoRastreioOCodigo(string codigo)
        {
            Console.WriteLine($"[INFO] Rastreando código: {codigo}");
            _correiosPage.AcessarRastreamento();
            _correiosPage.RastrearCodigo(codigo);
        }

        [Then(@"devo ver uma mensagem informando que o CEP não foi encontrado")]
        public void EntaoDeveriaoVerMensagemCepNaoEncontrado()
        {
            bool naoEncontrado = _correiosPage.VerificarCepNaoEncontrado();
            Console.WriteLine($"[INFO] CEP não encontrado: {naoEncontrado}");
            // CHECK POR XPATH — validado em VerificarCepNaoEncontrado()
            Assert.That(naoEncontrado, Is.True,
                "Esperava mensagem de CEP não encontrado.");
        }

        [Then(@"devo ver o endereço ""(.*)"" em ""(.*)"" ""(.*)""")]
        public void EntaoDevoVerOEndereco(string logradouro, string cidade, string uf)
        {
            bool ok = _correiosPage.VerificarEnderecoCep(logradouro, cidade, uf);
            Console.WriteLine($"[INFO] Endereço '{logradouro}, {cidade}/{uf}' encontrado: {ok}");
            // CHECK POR CSS — validado em VerificarEnderecoCep()
            Assert.That(ok, Is.True,
                $"Esperava '{logradouro}' em '{cidade}/{uf}'.");
        }

        [Then(@"devo ver uma mensagem de código inválido")]
        public void EntaoDevoVerMensagemDeCodigoInvalido()
        {
            bool invalido = _correiosPage.VerificarCodigoInvalido();
            Console.WriteLine($"[INFO] Código inválido detectado: {invalido}");
            // CHECK POR XPATH — validado em VerificarCodigoInvalido()
            Assert.That(invalido, Is.True,
                "Esperava mensagem de código inválido no rastreamento.");
        }

        [Then(@"volto para a tela inicial")]
        public void EntaoVoltoParaTelaInicial()
        {
            _correiosPage.VoltarParaHomeCorreios();
            Console.WriteLine("[INFO] Voltou para a tela inicial.");
        }

        [Then(@"fecho o navegador")]
        public void EntaoFechoONavegador()
        {
            _correiosPage.FecharNavegador();
            Console.WriteLine("[INFO] Navegador fechado.");
        }
    }
}
