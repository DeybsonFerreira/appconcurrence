using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories.Estrategias;

public sealed class EstrategiaSemProtecao : IEstrategiaCadastro
{
    private readonly PessoaStore _store;

    public EstrategiaSemProtecao(PessoaStore store)
    {
        _store = store;
    }

    public EstrategiaConcorrencia Estrategia => EstrategiaConcorrencia.SemProtecao;

    public async Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        CancellationToken cancellationToken)
    {
        var jaExiste = _store.DocumentoJaExiste(documento);

        await SimulacaoLatencia.AguardarAsync(cancellationToken);

        if (jaExiste)
        {
            return ResultadoCadastro.Duplicado(documento);
        }

        var pessoa = Pessoa.Criar(nome, sobrenome, documento);
        _store.Adicionar(pessoa);

        return ResultadoCadastro.Criado(pessoa);
    }
}
