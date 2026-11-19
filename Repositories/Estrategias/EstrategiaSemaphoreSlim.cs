using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories.Estrategias;

public sealed class EstrategiaSemaphoreSlim : IEstrategiaCadastro
{
    private readonly PessoaStore _store;
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public EstrategiaSemaphoreSlim(PessoaStore store)
    {
        _store = store;
    }

    public EstrategiaConcorrencia Estrategia => EstrategiaConcorrencia.Semaphore;

    public async Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        CancellationToken cancellationToken)
    {
        await _mutex.WaitAsync(cancellationToken);
        try
        {
            await SimulacaoLatencia.AguardarAsync(cancellationToken);

            if (_store.DocumentoJaExiste(documento))
            {
                return ResultadoCadastro.Duplicado(documento);
            }

            var pessoa = Pessoa.Criar(nome, sobrenome, documento);
            _store.Adicionar(pessoa);

            return ResultadoCadastro.Criado(pessoa);
        }
        finally
        {
            _mutex.Release();
        }
    }
}
