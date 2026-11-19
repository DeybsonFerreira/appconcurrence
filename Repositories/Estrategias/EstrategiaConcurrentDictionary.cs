using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories.Estrategias;

public sealed class EstrategiaConcurrentDictionary : IEstrategiaCadastro
{
    private readonly PessoaStore _store;

    public EstrategiaConcurrentDictionary(PessoaStore store)
    {
        _store = store;
    }

    public EstrategiaConcorrencia Estrategia => EstrategiaConcorrencia.ConcurrentDictionary;

    public async Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        CancellationToken cancellationToken)
    {
        await SimulacaoLatencia.AguardarAsync(cancellationToken);

        var pessoa = Pessoa.Criar(nome, sobrenome, documento);

        if (!_store.TentarReservarNoIndice(documento, pessoa))
        {
            return ResultadoCadastro.Duplicado(documento);
        }

        _store.Adicionar(pessoa);

        return ResultadoCadastro.Criado(pessoa);
    }
}
