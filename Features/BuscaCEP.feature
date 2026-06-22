#language: pt-BR
Funcionalidade: Busca CEP e Rastreamento nos Correios
  Como usuário do site dos Correios
  Quero buscar CEPs e rastrear objetos
  Para obter informações de endereço e status de entrega

  @cep_invalido
  Cenário: Buscar CEP inexistente 80700000
    Dado que acesso o site dos Correios
    Quando busco pelo CEP "80700000"
    Então devo ver uma mensagem informando que o CEP não foi encontrado
    E volto para a tela inicial

  @cep_valido
  Cenário: Buscar CEP válido 01013-001
    Dado que acesso o site dos Correios
    Quando busco pelo CEP "01013-001"
    Então devo ver o endereço "Rua Quinze de Novembro" em "São Paulo" "SP"
    E volto para a tela inicial

  @rastreamento
  Cenário: Rastrear código inválido SS987654321BR
    Dado que acesso o site dos Correios
    Quando rastreio o código "SS987654321BR"
    Então devo ver uma mensagem de código inválido
    E fecho o navegador
