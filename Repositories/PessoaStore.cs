using System.Collections.Concurrent;
using System.Runtime.Caching;
using Concorrencia.Api.Models;

namespace Concorrencia.Api.Repositories;

public sealed class PessoaStore
{
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<string, Pessoa> _indice = new(StringComparer.OrdinalIgnoreCase);
    private readonly MemoryCache _cache = MemoryCache.Default;
    private readonly List<Pessoa> _pessoas = [];

    public IReadOnlyList<Pessoa> Listar()
    {
        lock (_sync)
        {
            return _pessoas
                .OrderByDescending(p => p.CriadoEm)
                .ToList();
        }
    }

    public void Limpar()
    {
        lock (_sync)
        {
            foreach (var pessoa in _pessoas)
            {
                _cache.Remove(ChaveCache(pessoa.Documento));
            }

            _pessoas.Clear();
            _indice.Clear();
        }
    }

    public bool DocumentoJaExiste(string documento)
    {
        return _indice.ContainsKey(documento);
    }

    public bool TentarReservarNoIndice(string documento, Pessoa pessoa)
    {
        return _indice.TryAdd(documento, pessoa);
    }

    public bool TentarReservarNoCache(string documento, Pessoa pessoa)
    {
        var existente = _cache.AddOrGetExisting(
            ChaveCache(documento),
            pessoa,
            ObjectCache.InfiniteAbsoluteExpiration);

        return existente is null;
    }

    public void Adicionar(Pessoa pessoa)
    {
        lock (_sync)
        {
            _pessoas.Add(pessoa);
        }

        _indice.TryAdd(pessoa.Documento, pessoa);
        _cache.Set(ChaveCache(pessoa.Documento), pessoa, ObjectCache.InfiniteAbsoluteExpiration);
    }

    private static string ChaveCache(string documento)
    {
        return $"documento:{documento.ToLowerInvariant()}";
    }
}
