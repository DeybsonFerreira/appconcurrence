namespace Concorrencia.Api.Repositories;

public enum EstrategiaConcorrencia
{
    SemProtecao,
    Semaphore,
    Lock,
    ConcurrentDictionary,
    MemoryCache
}
