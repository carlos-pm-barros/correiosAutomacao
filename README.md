# Correios Automacao — Testes Automatizados com Allure Report

Projeto de automação para o site dos Correios usando **C# (.NET 10)**, **Reqnroll**, **NUnit 4**, **Selenium WebDriver 4.44** e **Allure Report**.

O projeto gera um relatório visual, navegável e mais informativo com evidências, status dos cenários, histórico de execução, ambiente, screenshots de falha e publicação automática em **GitHub Pages**.

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 10 SDK
- Google Chrome instalado
- Java 21+ instalado para gerar o HTML do Allure localmente
- Allure Commandline instalado para visualizar/gerar o relatório local

Instalação opcional do Allure Commandline via npm:
```bash
npm install -g allure-commandline
```

### Executar os testes

**Com navegador visível:**
```bash
# macOS / Linux
HEADLESS=false dotnet test

# Windows PowerShell
$env:HEADLESS="false"; dotnet test

# Windows CMD
set HEADLESS=false && dotnet test
```

**Sem navegador — modo CI/CD padrão:**
```bash
dotnet test
# ou explicitamente:
HEADLESS=true dotnet test
```

**Com log detalhado:**
```bash
HEADLESS=false dotnet test --logger "console;verbosity=detailed"
```

**Cenário específico por tag:**
```bash
HEADLESS=false dotnet test --filter "Category=cep_invalido"
HEADLESS=false dotnet test --filter "Category=cep_valido"
HEADLESS=false dotnet test --filter "Category=rastreamento"
```

---

## 📊 Allure Report

O projeto usa o pacote **Allure.Reqnroll** para gerar automaticamente os arquivos de resultado em `allure-results` durante a execução dos testes.

### Arquivos adicionados/alterados

| Arquivo | Finalidade |
|--------|------------|
| `allureConfig.json` | Configura o diretório `allure-results` e o título do relatório |
| `CorreiosAutomacao.csproj` | Adiciona o pacote `Allure.Reqnroll` e copia o `allureConfig.json` para o output |
| `Hooks/TestHooks.cs` | Gera `environment.properties` e anexa screenshot ao Allure em caso de falha |
| `.github/workflows/ci.yml` | Executa testes, gera o Allure HTML e publica no GitHub Pages |

### Gerar relatório local

Após executar os testes:
```bash
allure generate ./bin/Debug/net10.0/allure-results --clean -o ./allure-report
```

Abrir o relatório local:
```bash
allure open ./allure-report
```

Em execução `Release`, o diretório pode ser:
```bash
./bin/Release/net10.0/allure-results
```

---

## 🌐 Publicação no GitHub Pages

A pipeline `.github/workflows/ci.yml` possui dois jobs principais:

1. **`test`**  
   Executa os testes em `ubuntu-latest`, `macos-latest` e `windows-latest`, gera TRX e coleta `allure-results`.

2. **`publish-allure`**  
   Baixa os resultados do Allure, gera o HTML com `allure-commandline` e publica o relatório no GitHub Pages.

Após o workflow rodar na branch `main`, o relatório ficará disponível em:

```text
https://<usuario>.github.io/<repositorio>/
```

Exemplo:
```text
https://carlos.pierre.barros.github.io/CorreiosAutomacao/
```

### Configuração necessária no GitHub

No repositório, habilite o Pages por GitHub Actions:

1. Acesse **Settings**
2. Clique em **Pages**
3. Em **Build and deployment**, selecione **Source: GitHub Actions**
4. Execute o workflow manualmente ou faça push na branch `main`

---

## 🔎 O que o relatório apresenta

| Informação | Onde aparece no Allure |
|-----------|-------------------------|
| Feature e cenários BDD | Aba principal do relatório |
| Status dos testes | Passed, Failed, Broken ou Skipped |
| Tempo de execução | Por cenário e por suíte |
| Tags Gherkin | Filtros e agrupamentos |
| Evidência de falha | Screenshot anexado automaticamente |
| Ambiente de execução | Arquivo `environment.properties` |
| Execuções por sistema operacional | Resultados combinados da matrix do GitHub Actions |

---

## ⚙️ Variável de Ambiente HEADLESS

| Valor | Comportamento |
|-------|--------------|
| `true` | Sem navegador, padrão para CI/CD |
| `false` | Navegador abre na tela |
| *(não definida)* | Assume `true` |

---

## 🔄 GitHub Actions — CI/CD

O arquivo `.github/workflows/ci.yml` configura a pipeline com:

- Trigger automático em push para `main` e `develop`
- Trigger em pull requests para `main`
- Execução agendada diariamente às 06:00 UTC, equivalente a 03:00 BRT
- Execução manual via `workflow_dispatch`
- Matrix com Ubuntu, macOS e Windows
- Cache de pacotes NuGet
- Geração de resultados TRX
- Upload de artefatos de falha
- Upload de `allure-results`
- Geração do HTML do Allure
- Deploy automático no GitHub Pages

Para subir o projeto no GitHub:
```bash
git init
git add .
git commit -m "feat: adiciona Allure Report com GitHub Pages"
git remote add origin https://github.com/<usuario>/CorreiosAutomacao.git
git push -u origin main
```

---

## ⚡ Otimizações de Performance

O projeto mantém otimizações para reduzir o tempo total de execução:

- Eliminação de `Thread.Sleep` fixos
- Uso de esperas explícitas com polling reduzido
- `ImplicitWait` zerado para evitar espera dupla
- Seletores CSS priorizados quando possível
- Tratamento de `StaleElementReferenceException`
- Reutilização do driver durante a suíte
- Execução headless por padrão no CI/CD

---

## 🏗️ Estrutura do Projeto

```text
CorreiosAutomacao/
├── .github/
│   └── workflows/
│       └── ci.yml                # Pipeline GitHub Actions + GitHub Pages
├── Features/
│   └── BuscaCEP.feature          # Cenários Gherkin em pt-BR
├── Steps/
│   └── BuscaCEPSteps.cs          # Step Definitions Reqnroll
├── Pages/
│   └── CorreiosPage.cs           # Page Object Model
├── Hooks/
│   └── TestHooks.cs              # Hooks, métricas, screenshots e Allure environment
├── Support/
│   └── WebDriverConfig.cs        # Driver singleton + tuning de performance
├── allureConfig.json             # Configuração do Allure
├── reqnroll.json                 # Configuração do Reqnroll
├── CorreiosAutomacao.csproj      # Dependências do projeto
└── README.md
```

---

## 📦 Stack

| Tecnologia | Versão |
|------------|--------|
| .NET | 10.0 |
| Reqnroll.NUnit | 3.3.4 |
| NUnit | 4.6.1 |
| NUnit3TestAdapter | 6.2.0 |
| Selenium.WebDriver | 4.44.0 |
| Selenium.Support | 4.44.0 |
| Allure.Reqnroll | 2.15.0 |

---

## 🧪 Cenários

| # | CEP / Código | Validação |
|---|-------------|-----------|
| 1 | CEP `80700000` | Não encontrado |
| 2 | CEP `01013-001` | Rua Quinze de Novembro, São Paulo/SP |
| 3 | Rastreamento `SS987654321BR` | Código inválido |

---

## 🔍 Seletores Implementados

| Tipo | Onde é usado |
|------|-------------|
| **By.Id** | `cepEntrada` e `objetos` |
| **By.XPath** | Mensagens de CEP não encontrado e código inválido |
| **By.CssSelector** | Botão buscar, resultado de endereço, cookies e popups |

---

## 🌿 Git — Versionamento

```bash
# Submeter novo código
git status
git add .
git commit -m "feat: descrição da alteração"
git push origin main

# Fluxo com feature branch
git checkout -b feature/nome-da-feature
git add .
git commit -m "feat: descrição"
git push origin feature/nome-da-feature
# Abrir Pull Request no GitHub e fazer merge após aprovação
```
