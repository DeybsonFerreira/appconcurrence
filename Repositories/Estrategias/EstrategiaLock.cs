using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories.Estrategias;

public sealed class EstrategiaLock : IEstrategiaCadastro
{
    private readonly PessoaStore _store;
    private readonly object _lock = new();

    public EstrategiaLock(PessoaStore store)
    {
        _store = store;
    }

    public EstrategiaConcorrencia Estrategia => EstrategiaConcorrencia.Lock;

    public Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            SimulacaoLatencia.Aguardar();

            if (_store.DocumentoJaExiste(documento))
            {
                return Task.FromResult(ResultadoCadastro.Duplicado(documento));
            }

            var pessoa = Pessoa.Criar(nome, sobrenome, documento);
            _store.Adicionar(pessoa);

            return Task.FromResult(ResultadoCadastro.Criado(pessoa));
        }
    }
}
