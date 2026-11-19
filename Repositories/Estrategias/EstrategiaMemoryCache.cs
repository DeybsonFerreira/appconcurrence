using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories.Estrategias;

public sealed class EstrategiaMemoryCache : IEstrategiaCadastro
{
    private readonly PessoaStore _store;

    public EstrategiaMemoryCache(PessoaStore store)
    {
        _store = store;
    }

    public EstrategiaConcorrencia Estrategia => EstrategiaConcorrencia.MemoryCache;

    public async Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        CancellationToken cancellationToken)
    {
        await SimulacaoLatencia.AguardarAsync(cancellationToken);

        var pessoa = Pessoa.Criar(nome, sobrenome, documento);

        if (!_store.TentarReservarNoCache(documento, pessoa))
        {
            return ResultadoCadastro.Duplicado(documento);
        }

        _store.Adicionar(pessoa);

        return ResultadoCadastro.Criado(pessoa);
    }
}
