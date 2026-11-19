namespace Concorrencia.Api.Repositories.Estrategias;

public interface IEstrategiaCadastro
{
    EstrategiaConcorrencia Estrategia { get; }

    Task<ResultadoCadastro> AdicionarAsync(
        string nome,
        string sobrenome,
        string documento,
        CancellationToken cancellationToken);
}
