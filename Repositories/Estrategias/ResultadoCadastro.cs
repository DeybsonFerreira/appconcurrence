using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories.Estrategias;

public sealed record ResultadoCadastro(bool Sucesso, string? Mensagem, Pessoa? Pessoa)
{
    public static ResultadoCadastro Criado(Pessoa pessoa)
    {
        return new ResultadoCadastro(true, null, pessoa);
    }

    public static ResultadoCadastro Duplicado(string documento)
    {
        return new ResultadoCadastro(false, $"Documento '{documento}' já cadastrado.", null);
    }
}
