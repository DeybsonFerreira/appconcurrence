using Concorrencia.Api.Models;
using Concorrencia.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Concorrencia.Api.Controllers;

[ApiController]
[Route("api/pessoas")]
public sealed class PessoaController : ControllerBase
{
    private readonly PessoaRepository _repository;

    public PessoaController(PessoaRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<Pessoa>> Listar()
    {
        return Ok(_repository.Listar());
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(
        [FromBody] CriarPessoaRequest request,
        [FromQuery] string? estrategia,
        CancellationToken cancellationToken)
    {
        var erro = request.Validar(out var nome, out var sobrenome, out var documento);
        if (erro is not null)
        {
            return BadRequest(new { sucesso = false, mensagem = erro });
        }

        var estrategiaSelecionada = ConverterEstrategia(estrategia);
        if (estrategiaSelecionada is null)
        {
            return BadRequest(new
            {
                sucesso = false,
                mensagem = "Estratégia inválida. Use sem-protecao, semaphore, lock, concurrent-dictionary ou memory-cache."
            });
        }

        var resultado = await _repository.AdicionarAsync(
            nome,
            sobrenome,
            documento,
            estrategiaSelecionada.Value,
            cancellationToken);

        if (!resultado.Sucesso)
        {
            return Conflict(new { sucesso = false, mensagem = resultado.Mensagem });
        }

        return Created($"/api/pessoas/{resultado.Pessoa!.Id}", new
        {
            sucesso = true,
            mensagem = "Pessoa cadastrada.",
            pessoa = resultado.Pessoa
        });
    }

    [HttpPost("limpar")]
    public IActionResult Limpar()
    {
        _repository.Limpar();
        return Ok(new { sucesso = true, mensagem = "Dados em memória removidos." });
    }

    private static EstrategiaConcorrencia? ConverterEstrategia(string? valor)
    {
        return (valor ?? "sem-protecao").Trim().ToLowerInvariant() switch
        {
            "sem-protecao" => EstrategiaConcorrencia.SemProtecao,
            "semaphore" => EstrategiaConcorrencia.Semaphore,
            "lock" or "monitor" => EstrategiaConcorrencia.Lock,
            "concurrent-dictionary" => EstrategiaConcorrencia.ConcurrentDictionary,
            "memory-cache" => EstrategiaConcorrencia.MemoryCache,
            _ => null
        };
    }
}
