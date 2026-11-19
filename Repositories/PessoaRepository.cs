using Concorrencia.Api.Models;
using Concorrencia.Api.Repositories.Estrategias;

namespace Concorrencia.Api.Repositories;

public sealed class PessoaRepository
{
    private readonly PessoaStore _store;
    private readonly IReadOnlyDictionary<EstrategiaConcorrencia, IEstrategiaCadastro> _estrategias;

    public PessoaRepository(PessoaStore store, IEnumerable<IEstrategiaCadastro> estrategias)
    {
        _store = store;
        _estrategias = estrategias.ToDictionary(estrategia => estrategia.Estrategia);
    }

    public IReadOnlyList<Pessoa> Listar()
    {
        return _store.Listar();
    }

    public void Limpar()
    {
        _store.Limpar();
    }

    public Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        EstrategiaConcorrencia estrategia,
        CancellationToken cancellationToken)
    {
        if (!_estrategias.TryGetValue(estrategia, out var implementacao))
        {
            throw new InvalidOperationException($"Estratégia não registrada: {estrategia}.");
        }

        return implementacao.AdicionarAsync(nome, sobrenome, documento, cancellationToken);
    }
}
