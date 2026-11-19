namespace Concorrencia.Api.Models;

public sealed record Pessoa(
    Guid Id,
    string Nome,
    string Sobrenome,
    string Documento,
    DateTimeOffset CriadoEm)
{
    public static Pessoa Criar(string nome, string sobrenome, string documento)
    {
        return new Pessoa(Guid.NewGuid(), nome, sobrenome, documento, DateTimeOffset.Now);
    }
}

public sealed record CriarPessoaRequest(string? Nome, string? Sobrenome, string? Documento)
{
    public string? Validar(out string nome, out string sobrenome, out string documento)
    {
        nome = (Nome ?? string.Empty).Trim();
        sobrenome = (Sobrenome ?? string.Empty).Trim();
        documento = (Documento ?? string.Empty).Trim();

        if (nome.Length == 0)
        {
            return "Informe o nome.";
        }

        if (sobrenome.Length == 0)
        {
            return "Informe o sobrenome.";
        }

        if (documento.Length == 0)
        {
            return "Informe o documento.";
        }

        return null;
    }
}
