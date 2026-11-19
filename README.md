# POC de Concorrência

Prova de conceito em .NET 9 + HTML para demonstrar concorrência no cadastro de pessoas em memória.

## O que é concorrência?

Concorrência acontece quando duas ou mais operações acessam ou alteram o mesmo recurso ao mesmo tempo. Sem controle, pode ocorrer uma **race condition**: duas requisições verificam que um documento não existe e ambas inserem, gerando duplicidade.

Nesta POC, o cadastro tem uma latência simulada de banco de dados (200 a 500 ms). O botão **Simular 10x** dispara 10 requisições simultâneas com o mesmo documento.

## Como executar

```bash
dotnet run --project Concorrencia.Api
```

Ou abra `Concorrencia.sln` no Visual Studio e execute com F5.

Acesse: `http://localhost:5014`

## Como testar

1. Preencha Nome, Sobrenome e Documento.
2. Escolha uma estratégia.
3. Clique em **Simular 10x**.
4. Veja o resumo e os registros duplicados na tabela.

Limpe os dados entre os testes para comparar as estratégias.

## Soluções implementadas

| Estratégia | Como funciona | Evita duplicidade? |
|---|---|---|
| **Sem proteção** | Verifica a duplicidade e insere sem sincronização | Não |
| **SemaphoreSlim** | Serializa a seção crítica de forma assíncrona | Sim |
| **lock / Monitor** | Serializa a seção crítica bloqueando a thread | Sim |
| **ConcurrentDictionary** | Usa `TryAdd` atômico por documento | Sim |
| **MemoryCache** | Usa `AddOrGetExisting` atômico (`System.Runtime.Caching`) | Sim |

### Sem proteção

A verificação de duplicidade acontece antes da latência simulada. Várias requisições passam pela verificação ao mesmo tempo e todas inserem, reproduzindo o problema.

### SemaphoreSlim

Um `SemaphoreSlim(1, 1)` garante que apenas uma requisição por vez execute a verificação e a inserção. As demais aguardam de forma assíncrona.

### lock / Monitor

Um `lock` protege a seção crítica de forma síncrona. Funciona, mas bloqueia a thread durante a latência simulada.

### ConcurrentDictionary

`ConcurrentDictionary.TryAdd` faz a inserção apenas se o documento ainda não existir, de forma atômica.

### MemoryCache

`MemoryCache.AddOrGetExisting` insere o documento apenas se ele não estiver no cache, de forma atômica. É diferente de `GetOrCreate`, que não garante execução única.

## Estrutura

```text
Concorrencia.Api/
├── Controllers/
│   └── PessoaController.cs
├── Models/
│   └── Pessoa.cs
├── Repositories/
│   ├── EstrategiaConcorrencia.cs
│   ├── PessoaRepository.cs
│   ├── PessoaStore.cs
│   └── Estrategias/
│       ├── IEstrategiaCadastro.cs
│       ├── ResultadoCadastro.cs
│       ├── SimulacaoLatencia.cs
│       ├── EstrategiaSemProtecao.cs
│       ├── EstrategiaSemaphoreSlim.cs
│       ├── EstrategiaLock.cs
│       ├── EstrategiaConcurrentDictionary.cs
│       └── EstrategiaMemoryCache.cs
└── wwwroot/
    ├── index.html
    ├── index.css
    └── index.js
```

## API

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/pessoas` | Lista as pessoas cadastradas |
| POST | `/api/pessoas?estrategia=lock` | Cadastra uma pessoa |
| POST | `/api/pessoas/limpar` | Limpa os dados em memória |

Estratégias aceitas: `sem-protecao`, `semaphore`, `lock` (ou `monitor`), `concurrent-dictionary` e `memory-cache`.

## Observação

Em um banco de dados real, a solução definitiva é uma constraint de unicidade no documento, combinada com tratamento de conflito ou concorrência otimista.
